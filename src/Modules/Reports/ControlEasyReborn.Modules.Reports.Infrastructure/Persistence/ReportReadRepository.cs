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
}
