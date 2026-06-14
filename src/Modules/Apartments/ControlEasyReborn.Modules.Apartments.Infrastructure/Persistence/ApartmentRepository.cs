using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Apartments.Infrastructure.Persistence;

public sealed class ApartmentRepository : IApartmentRepository
{
    private const string TableName = "Apartments";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ApartmentRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Apartment?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Block, Unit, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<Apartment?> FindByBlockUnitAsync(string block, string unit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Block, Unit, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Block = @param0 AND Unit = @param1",
            parameters: new object[] { block, unit },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        var apartment = await FindAsync(id, ct);
        return apartment is not null;
    }

    public async Task<IReadOnlyList<Apartment>> ListAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereClause = string.IsNullOrWhiteSpace(search)
            ? "1=1"
            : "(Block LIKE @param0 OR Unit LIKE @param0)";
        var parameters = string.IsNullOrWhiteSpace(search)
            ? Array.Empty<object>()
            : new object[] { "%" + search + "%" };

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Block, Unit, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters,
            ct: ct);

        return MapList(rows);
    }

    public async Task AddAsync(Apartment apartment, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Block", "Unit", "Active", "CreatedAtUtc", "UpdatedAtUtc", "tenant_id" },
            TableName,
            new object?[] { apartment.Id, apartment.TenantId, apartment.Block, apartment.Unit, apartment.Active, apartment.CreatedAtUtc, (object?)apartment.UpdatedAtUtc ?? DBNull.Value, apartment.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Apartment apartment, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "Block", "Unit", "Active", "UpdatedAtUtc" },
            TableName,
            new[] { apartment.Block, apartment.Unit, apartment.Active ? "1" : "0", apartment.UpdatedAtUtc.HasValue ? apartment.UpdatedAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff") : null! },
            $"Id = '{apartment.Id}'",
            Array.Empty<object>(),
            ct: ct);
    }

    private static Apartment? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Apartment> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Apartment>();
        var list = new List<Apartment>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Apartment? MapRow(DataRow r)
    {
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new Apartment(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            block: r["Block"]?.ToString() ?? string.Empty,
            unit: r["Unit"]?.ToString() ?? string.Empty,
            active: Convert.ToBoolean(r["Active"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}
