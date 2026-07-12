using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;

public sealed class ConfigurationRepository : IConfigurationRepository
{
    private const string TableName = "Configurations";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ConfigurationRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<ConfigurationEntry?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, `Key`, Value, Description, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<ConfigurationEntry?> GetByKeyAsync(Guid tenantId, string key, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, `Key`, Value, Description, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "TenantId = @param0 AND `Key` = @param1",
            parameters: new object[] { tenantId, key },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<ConfigurationEntry>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, `Key`, Value, Description, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "TenantId = @param0",
            parameters: new object[] { tenantId },
            ct: ct);
        return MapList(rows);
    }

    public async Task AddAsync(ConfigurationEntry entry, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Key", "Value", "Description", "CreatedAtUtc", "UpdatedAtUtc" },
            TableName,
            new object?[] { entry.Id, entry.TenantId, entry.Key, entry.Value, (object?)entry.Description ?? DBNull.Value, entry.CreatedAtUtc, (object?)entry.UpdatedAtUtc ?? DBNull.Value },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(ConfigurationEntry entry, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "Value", "UpdatedAtUtc" },
            TableName,
            new[] { entry.Value, entry.UpdatedAtUtc.HasValue ? entry.UpdatedAtUtc.Value.ToString("o") : string.Empty },
            "Id = @param0",
            new object[] { entry.Id },
            ct: ct);
    }

    private static ConfigurationEntry? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<ConfigurationEntry> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<ConfigurationEntry>();
        var list = new List<ConfigurationEntry>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static ConfigurationEntry? MapRow(DataRow r)
    {
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new ConfigurationEntry(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            key: r["Key"]?.ToString() ?? string.Empty,
            value: r["Value"]?.ToString() ?? string.Empty,
            description: string.IsNullOrEmpty(r["Description"]?.ToString()) ? null : r["Description"].ToString(),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}