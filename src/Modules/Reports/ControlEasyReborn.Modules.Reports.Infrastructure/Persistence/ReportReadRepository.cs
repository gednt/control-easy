using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Reports.Application.Abstractions;
using ControlEasyReborn.Modules.Reports.Application.Contracts;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using System.Data;

namespace ControlEasyReborn.Modules.Reports.Infrastructure.Persistence;

public sealed class ReportReadRepository : IReportReadRepository
{
    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ReportReadRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<IReadOnlyList<VisitCountByDayResponse>> GetVisitCountsByDayAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereClause = BuildDateRangeClause(from, to, out var parameters);

        var rows = await db.SelectAsync(
            fields: "CreatedAtUtc",
            table: "Visits",
            whereClause: whereClause,
            parameters: parameters,
            ct: ct);

        return rows.AsEnumerable()
            .Select(MapVisitDate)
            .AsQueryable()
            .GroupBy(d => d)
            .Select(g => new VisitCountByDayResponse(g.Key, g.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<ResidentsPerApartmentResponse>> GetResidentsPerApartmentAsync(
        CancellationToken ct)
    {
        var db = _factory.Create(_ctx);

        var rows = await db.SelectAsync(
            fields: "ApartmentId",
            table: "Residents",
            whereClause: "Active = 1",
            parameters: Array.Empty<object>(),
            ct: ct);

        return rows.AsEnumerable()
            .Select(MapApartmentId)
            .AsQueryable()
            .GroupBy(a => a)
            .Select(g => new ResidentsPerApartmentResponse(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken ct)
    {
        var db = _factory.Create(_ctx);

        var residentRows = await db.SelectAsync(
            fields: "Active",
            table: "Residents",
            whereClause: "1=1",
            parameters: Array.Empty<object>(),
            ct: ct);

        var totalResidents = residentRows.AsEnumerable().Count();
        var activeResidents = residentRows.AsEnumerable().Count(r => Convert.ToBoolean(r["Active"]));

        var vehicleRows = await db.SelectAsync(
            fields: "Active",
            table: "Vehicles",
            whereClause: "1=1",
            parameters: Array.Empty<object>(),
            ct: ct);

        var totalVehicles = vehicleRows.AsEnumerable().Count();
        var activeVehicles = vehicleRows.AsEnumerable().Count(r => Convert.ToBoolean(r["Active"]));

        var apartmentRows = await db.SelectAsync(
            fields: "Id, Block, Unit",
            table: "Apartments",
            whereClause: "1=1",
            parameters: Array.Empty<object>(),
            ct: ct);

        var totalApartments = apartmentRows.AsEnumerable().Count();

        var tenantId = _ctx.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved.");
        var occupiedApartmentRows = await db.SelectRawAsync(
            "SELECT DISTINCT a.Id FROM Apartments a INNER JOIN Residents r ON a.Id = r.ApartmentId WHERE a.tenant_id = @param0 AND r.tenant_id = @param1 AND r.Active = 1",
            new object[] { tenantId, tenantId },
            ct);

        var occupiedApartments = occupiedApartmentRows.AsEnumerable().Count();

        var openVisitRows = await db.SelectAsync(
            fields: "Id",
            table: "Visits",
            whereClause: "Status IN (0, 1)",
            parameters: Array.Empty<object>(),
            ct: ct);

        var openVisits = openVisitRows.AsEnumerable().Count();

        var today = DateTime.UtcNow.Date;
        var todayVisitRows = await db.SelectAsync(
            fields: "Id",
            table: "Visits",
            whereClause: "CreatedAtUtc >= @param0",
            parameters: new object[] { today },
            ct: ct);

        var todayVisits = todayVisitRows.AsEnumerable().Count();

        var recentVisitRows = await db.SelectAsync(
            fields: "Id, VisitorName, Purpose, Status, ApartmentId, CreatedAtUtc",
            table: "Visits",
            whereClause: "1=1",
            parameters: Array.Empty<object>(),
            ct: ct);

        var apartmentLookup = new Dictionary<string, (string? Block, string? Unit)>();
        foreach (DataRow ar in apartmentRows.AsEnumerable())
        {
            var aptId = ar["Id"]?.ToString();
            if (aptId is not null)
            {
                apartmentLookup[aptId] = (ar["Block"]?.ToString(), ar["Unit"]?.ToString());
            }
        }

        var recentVisits = recentVisitRows.AsEnumerable()
            .OrderByDescending(r => Convert.ToDateTime(r["CreatedAtUtc"]))
            .Take(10)
            .Select(r => MapRecentVisitWithApartment(r, apartmentLookup))
            .ToList();

        return new DashboardStatsResponse(
            totalResidents,
            activeResidents,
            totalVehicles,
            activeVehicles,
            totalApartments,
            occupiedApartments,
            openVisits,
            todayVisits,
            recentVisits);
    }

    private static string BuildDateRangeClause(DateOnly? from, DateOnly? to, out object[] parameters)
    {
        var clauses = new List<string>();
        var paramList = new List<object>();

        if (from.HasValue)
        {
            clauses.Add($"CreatedAtUtc >= @param{paramList.Count}");
            paramList.Add(from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (to.HasValue)
        {
            clauses.Add($"CreatedAtUtc < @param{paramList.Count}");
            paramList.Add(to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        parameters = paramList.ToArray();
        return clauses.Count == 0 ? "1=1" : string.Join(" AND ", clauses);
    }

    private static DateOnly MapVisitDate(DataRow row)
    {
        var value = Convert.ToDateTime(row["CreatedAtUtc"]);
        return DateOnly.FromDateTime(value.ToUniversalTime());
    }

    private static Guid? MapApartmentId(DataRow row)
    {
        var raw = row["ApartmentId"]?.ToString();
        return string.IsNullOrWhiteSpace(raw) ? null : Guid.Parse(raw);
    }

    private static RecentVisitDto MapRecentVisitWithApartment(DataRow r, Dictionary<string, (string? Block, string? Unit)> apartmentLookup)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var statusInt = Convert.ToInt32(r["Status"]);
        var status = statusInt switch
        {
            0 => "Pending",
            1 => "CheckedIn",
            2 => "CheckedOut",
            3 => "Cancelled",
            _ => "Unknown"
        };
        string? apartmentLabel = null;
        if (!string.IsNullOrEmpty(apartmentIdStr) && apartmentLookup.TryGetValue(apartmentIdStr, out var apt))
        {
            if (!string.IsNullOrEmpty(apt.Block) && !string.IsNullOrEmpty(apt.Unit))
            {
                apartmentLabel = $"{apt.Block}-{apt.Unit}";
            }
        }

        return new RecentVisitDto(
            Id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            VisitorName: r["VisitorName"]?.ToString() ?? string.Empty,
            Purpose: string.IsNullOrEmpty(r["Purpose"]?.ToString()) ? null : r["Purpose"].ToString(),
            Status: status,
            ApartmentLabel: apartmentLabel,
            CreatedAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]));
    }
}
