using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Security.Infrastructure.Persistence;

public sealed class AttendantProfileRepository : IAttendantProfileRepository
{
    private const string TableName = "AttendantProfiles";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public AttendantProfileRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<AttendantProfile?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, UserId, DisplayName, ShiftId, GatehouseId, Permissions, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<AttendantProfile>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, UserId, DisplayName, ShiftId, GatehouseId, Permissions, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "TenantId = @param0",
            parameters: new object[] { tenantId },
            ct: ct);
        return MapList(rows);
    }

    public async Task<IReadOnlyList<AttendantProfile>> ListByUserAsync(Guid userId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx, bypassTenantFilter: true);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, UserId, DisplayName, ShiftId, GatehouseId, Permissions, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "UserId = @param0",
            parameters: new object[] { userId },
            ct: ct);
        return MapList(rows);
    }

    public async Task CreateAsync(AttendantProfile profile, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "UserId", "DisplayName", "ShiftId", "GatehouseId", "Permissions", "Active", "CreatedAtUtc", "UpdatedAtUtc" },
            TableName,
            new object?[] { profile.Id, profile.TenantId, profile.UserId, (object?)profile.DisplayName ?? DBNull.Value, (object?)profile.ShiftId ?? DBNull.Value, (object?)profile.GatehouseId ?? DBNull.Value, profile.Permissions, profile.Active, profile.CreatedAtUtc, (object?)profile.UpdatedAtUtc ?? DBNull.Value },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(AttendantProfile profile, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "DisplayName", "ShiftId", "GatehouseId", "Permissions", "UpdatedAtUtc" },
            TableName,
            new[] { profile.DisplayName ?? string.Empty, profile.ShiftId.HasValue ? profile.ShiftId.Value.ToString() : string.Empty, profile.GatehouseId.HasValue ? profile.GatehouseId.Value.ToString() : string.Empty, profile.Permissions, profile.UpdatedAtUtc.HasValue ? profile.UpdatedAtUtc.Value.ToString("o") : string.Empty },
            "Id = @param0",
            new object[] { profile.Id },
            ct: ct);
    }

    public async Task DeactivateAsync(AttendantProfile profile, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "Active", "UpdatedAtUtc" },
            TableName,
            new[] { profile.Active ? "1" : "0", profile.UpdatedAtUtc.HasValue ? profile.UpdatedAtUtc.Value.ToString("o") : string.Empty },
            "Id = @param0",
            new object[] { profile.Id },
            ct: ct);
    }

    private static AttendantProfile? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<AttendantProfile> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<AttendantProfile>();
        var list = new List<AttendantProfile>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static AttendantProfile? MapRow(DataRow r)
    {
        var shiftIdStr = r["ShiftId"]?.ToString();
        var gatehouseIdStr = r["GatehouseId"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new AttendantProfile(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            userId: Guid.Parse(r["UserId"].ToString() ?? string.Empty),
            displayName: string.IsNullOrEmpty(r["DisplayName"]?.ToString()) ? null : r["DisplayName"].ToString(),
            shiftId: string.IsNullOrEmpty(shiftIdStr) ? null : Guid.Parse(shiftIdStr),
            gatehouseId: string.IsNullOrEmpty(gatehouseIdStr) ? null : Guid.Parse(gatehouseIdStr),
            permissions: r["Permissions"]?.ToString() ?? string.Empty,
            active: Convert.ToBoolean(r["Active"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}