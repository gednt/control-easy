using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.Auditing;
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
            new[]
            {
                "Id",
                "TenantId",
                "tenant_id",
                "Category",
                "Action",
                "EntityType",
                "EntityId",
                "Severity",
                "PerformedByUserId",
                "PerformedByName",
                "Details",
                "MetadataJson",
                "CreatedAtUtc"
            },
            TableName,
            new object?[]
            {
                entry.Id,
                entry.TenantId,
                entry.TenantId,
                entry.Category,
                entry.Action,
                entry.EntityType,
                entry.EntityId,
                (int)entry.Severity,
                entry.PerformedByUserId,
                (object?)entry.PerformedByName ?? DBNull.Value,
                (object?)entry.Details ?? DBNull.Value,
                (object?)entry.MetadataJson ?? DBNull.Value,
                entry.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss")
            },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "*",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);

        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(
        Guid tenantId,
        string? category,
        AuditSeverity? severity,
        string? entityType,
        string? action,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? searchTerm,
        int skip,
        int take,
        CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereParts = new List<string> { "TenantId = @param0" };
        var parameters = new List<object> { tenantId };
        var paramIndex = 1;

        if (!string.IsNullOrWhiteSpace(category))
        {
            whereParts.Add($"Category = @param{paramIndex}");
            parameters.Add(category);
            paramIndex++;
        }

        if (severity.HasValue)
        {
            whereParts.Add($"Severity = @param{paramIndex}");
            parameters.Add((int)severity.Value);
            paramIndex++;
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            whereParts.Add($"EntityType = @param{paramIndex}");
            parameters.Add(entityType);
            paramIndex++;
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            whereParts.Add($"Action = @param{paramIndex}");
            parameters.Add(action);
            paramIndex++;
        }

        if (fromUtc.HasValue)
        {
            whereParts.Add($"CreatedAtUtc >= @param{paramIndex}");
            parameters.Add(fromUtc.Value);
            paramIndex++;
        }

        if (toUtc.HasValue)
        {
            whereParts.Add($"CreatedAtUtc <= @param{paramIndex}");
            parameters.Add(toUtc.Value);
            paramIndex++;
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            whereParts.Add($"(Action LIKE @param{paramIndex} OR EntityType LIKE @param{paramIndex} OR PerformedByName LIKE @param{paramIndex} OR Details LIKE @param{paramIndex})");
            parameters.Add(pattern);
            paramIndex++;
        }

        var whereClause = string.Join(" AND ", whereParts);

        var rows = await db.SelectAsync(
            fields: "*",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters.ToArray(),
            ct: ct);

        var list = MapList(rows)
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ThenByDescending(entry => entry.Id)
            .ToList();
        if (skip > 0 || take > 0)
        {
            var paged = list.AsEnumerable();
            if (skip > 0) paged = paged.Skip(skip);
            if (take > 0) paged = paged.Take(take);
            return paged.ToList();
        }

        return list;
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetByEntityAsync(Guid entityId, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "*",
            table: TableName,
            whereClause: "EntityId = @param0",
            parameters: new object[] { entityId },
            ct: ct);

        var list = MapList(rows)
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ThenByDescending(entry => entry.Id)
            .ToList();
        if (skip > 0 || take > 0)
        {
            var paged = list.AsEnumerable();
            if (skip > 0) paged = paged.Skip(skip);
            if (take > 0) paged = paged.Take(take);
            return paged.ToList();
        }

        return list;
    }

    private static AuditLogEntry? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
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
        var severityVal = r.Table.Columns.Contains("Severity") && r["Severity"] != DBNull.Value
            ? (AuditSeverity)Convert.ToInt32(r["Severity"])
            : AuditSeverity.Info;

        var categoryVal = r.Table.Columns.Contains("Category") && r["Category"] != DBNull.Value
            ? r["Category"].ToString() ?? AuditCategory.System
            : AuditCategory.System;

        var metadataJsonVal = r.Table.Columns.Contains("MetadataJson") && r["MetadataJson"] != DBNull.Value
            ? r["MetadataJson"].ToString()
            : null;

        return new AuditLogEntry(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            action: r["Action"]?.ToString() ?? string.Empty,
            entityType: r["EntityType"]?.ToString() ?? string.Empty,
            entityId: Guid.Parse(r["EntityId"].ToString() ?? string.Empty),
            performedByUserId: Guid.Parse(r["PerformedByUserId"].ToString() ?? string.Empty),
            performedByName: string.IsNullOrEmpty(r["PerformedByName"]?.ToString()) ? null : r["PerformedByName"].ToString(),
            details: string.IsNullOrEmpty(r["Details"]?.ToString()) ? null : r["Details"].ToString(),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            category: categoryVal,
            severity: severityVal,
            metadataJson: metadataJsonVal);
    }
}
