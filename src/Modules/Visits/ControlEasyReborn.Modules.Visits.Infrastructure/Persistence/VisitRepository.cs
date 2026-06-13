using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Visits.Infrastructure.Persistence;

public sealed class VisitRepository : IVisitRepository
{
    private const string TableName = "Visits";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public VisitRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Visit?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Visit>> ListAsync(Guid tenantId, string? status, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereClause = string.IsNullOrWhiteSpace(status)
            ? "1=1"
            : "Status = @param1";
        var parameters = string.IsNullOrWhiteSpace(status)
            ? Array.Empty<object>()
            : new object[] { (int)Enum.Parse(typeof(VisitStatus), status, true) };

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters,
            ct: ct);

        return MapList(rows);
    }

    public async Task<IReadOnlyList<Visit>> ListOpenAsync(Guid tenantId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Status IN (0, 1)",
            parameters: Array.Empty<object>(),
            ct: ct);

        return MapList(rows);
    }

    public async Task AddAsync(Visit visit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "VisitorName", "VisitorDocument", "VisitorPhone", "ApartmentId", "Purpose", "Status", "AttendantProfileId", "GatehouseId", "CheckedInAtUtc", "CheckedOutAtUtc", "CreatedAtUtc", "UpdatedAtUtc" },
            TableName,
            new object?[] { visit.Id, visit.TenantId, visit.VisitorName, visit.VisitorDocument, (object?)visit.VisitorPhone ?? DBNull.Value, (object?)visit.ApartmentId ?? DBNull.Value, (object?)visit.Purpose ?? DBNull.Value, (int)visit.Status, (object?)visit.AttendantProfileId ?? DBNull.Value, (object?)visit.GatehouseId ?? DBNull.Value, (object?)visit.CheckedInAtUtc ?? DBNull.Value, (object?)visit.CheckedOutAtUtc ?? DBNull.Value, visit.CreatedAtUtc, (object?)visit.UpdatedAtUtc ?? DBNull.Value },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Visit visit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "VisitorName", "VisitorDocument", "VisitorPhone", "ApartmentId", "Purpose", "Status", "AttendantProfileId", "GatehouseId", "CheckedInAtUtc", "CheckedOutAtUtc", "UpdatedAtUtc" },
            TableName,
            new[] { visit.VisitorName, visit.VisitorDocument, visit.VisitorPhone ?? string.Empty, visit.ApartmentId.HasValue ? visit.ApartmentId.Value.ToString() : string.Empty, visit.Purpose ?? string.Empty, ((int)visit.Status).ToString(), visit.AttendantProfileId.HasValue ? visit.AttendantProfileId.Value.ToString() : string.Empty, visit.GatehouseId.HasValue ? visit.GatehouseId.Value.ToString() : string.Empty, visit.CheckedInAtUtc.HasValue ? visit.CheckedInAtUtc.Value.ToString("o") : string.Empty, visit.CheckedOutAtUtc.HasValue ? visit.CheckedOutAtUtc.Value.ToString("o") : string.Empty, visit.UpdatedAtUtc.HasValue ? visit.UpdatedAtUtc.Value.ToString("o") : string.Empty },
            "Id = @param0",
            new object[] { visit.Id },
            ct: ct);
    }

    private static Visit? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Visit> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Visit>();
        var list = new List<Visit>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Visit? MapRow(DataRow r)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var attendantProfileIdStr = r["AttendantProfileId"]?.ToString();
        var gatehouseIdStr = r["GatehouseId"]?.ToString();
        var checkedInAtStr = r["CheckedInAtUtc"]?.ToString();
        var checkedOutAtStr = r["CheckedOutAtUtc"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new Visit(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            visitorName: r["VisitorName"]?.ToString() ?? string.Empty,
            visitorDocument: r["VisitorDocument"]?.ToString() ?? string.Empty,
            visitorPhone: string.IsNullOrEmpty(r["VisitorPhone"]?.ToString()) ? null : r["VisitorPhone"].ToString(),
            apartmentId: string.IsNullOrEmpty(apartmentIdStr) ? null : Guid.Parse(apartmentIdStr),
            purpose: string.IsNullOrEmpty(r["Purpose"]?.ToString()) ? null : r["Purpose"].ToString(),
            status: (VisitStatus)Convert.ToInt32(r["Status"]),
            attendantProfileId: string.IsNullOrEmpty(attendantProfileIdStr) ? null : Guid.Parse(attendantProfileIdStr),
            gatehouseId: string.IsNullOrEmpty(gatehouseIdStr) ? null : Guid.Parse(gatehouseIdStr),
            checkedInAtUtc: string.IsNullOrEmpty(checkedInAtStr) ? null : DateTime.Parse(checkedInAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            checkedOutAtUtc: string.IsNullOrEmpty(checkedOutAtStr) ? null : DateTime.Parse(checkedOutAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}