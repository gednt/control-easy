using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private const string TableName = "AuditLog";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public AuditLogRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task AddAsync(AuditLogEntry entry, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Action", "EntityType", "EntityId", "PerformedByUserId", "PerformedByName", "Details", "CreatedAtUtc" },
            TableName,
            new object?[] { entry.Id, entry.TenantId, entry.Action, entry.EntityType, entry.EntityId, entry.PerformedByUserId, (object?)entry.PerformedByName ?? DBNull.Value, (object?)entry.Details ?? DBNull.Value, entry.CreatedAtUtc },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(Guid tenantId, string? entityType, string? action, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereParts = new List<string> { "TenantId = @param0" };
        var parameters = new List<object> { tenantId };
        var paramIndex = 1;

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            whereParts.Add("EntityType = @param" + paramIndex);
            parameters.Add(entityType);
            paramIndex++;
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            whereParts.Add("Action = @param" + paramIndex);
            parameters.Add(action);
            paramIndex++;
        }

        var whereClause = string.Join(" AND ", whereParts);

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Action, EntityType, EntityId, PerformedByUserId, PerformedByName, Details, CreatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters.ToArray(),
            ct: ct);

        return MapList(rows);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetByEntityAsync(Guid entityId, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Action, EntityType, EntityId, PerformedByUserId, PerformedByName, Details, CreatedAtUtc",
            table: TableName,
            whereClause: "EntityId = @param0",
            parameters: new object[] { entityId },
            ct: ct);

        return MapList(rows);
    }

    private static IReadOnlyList<AuditLogEntry> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<AuditLogEntry>();
        var list = new List<AuditLogEntry>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static AuditLogEntry? MapRow(DataRow r)
    {
        return new AuditLogEntry(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            action: r["Action"]?.ToString() ?? string.Empty,
            entityType: r["EntityType"]?.ToString() ?? string.Empty,
            entityId: Guid.Parse(r["EntityId"].ToString() ?? string.Empty),
            performedByUserId: Guid.Parse(r["PerformedByUserId"].ToString() ?? string.Empty),
            performedByName: string.IsNullOrEmpty(r["PerformedByName"]?.ToString()) ? null : r["PerformedByName"].ToString(),
            details: string.IsNullOrEmpty(r["Details"]?.ToString()) ? null : r["Details"].ToString(),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]));
    }
}