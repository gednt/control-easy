using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Residents.Infrastructure.Persistence;

public sealed class ResidentRepository : IResidentRepository
{
    private const string TableName = "Residents";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ResidentRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Resident?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Name, Cpf, Email, Phone, ApartmentId, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Resident>> ListAsync(string? search, int skip, int take, CancellationToken ct, Guid? apartmentId = null)
    {
        var db = _factory.Create(_ctx);
        var whereParts = new List<string>();
        var paramList = new List<object>();
        var paramIndex = 0;

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereParts.Add("(Name LIKE @param" + paramIndex + " OR Cpf LIKE @param" + paramIndex + " OR Email LIKE @param" + paramIndex + ")");
            paramList.Add("%" + search + "%");
            paramIndex++;
        }

        if (apartmentId.HasValue)
        {
            whereParts.Add("ApartmentId = @param" + paramIndex);
            paramList.Add(apartmentId.Value.ToString());
            paramIndex++;
        }

        var whereClause = whereParts.Count > 0 ? string.Join(" AND ", whereParts) : "1=1";

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Name, Cpf, Email, Phone, ApartmentId, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: paramList.ToArray(),
            ct: ct);

        return MapList(rows);
    }

    public async Task AddAsync(Resident resident, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Name", "Cpf", "Email", "Phone", "ApartmentId", "Active", "CreatedAtUtc", "UpdatedAtUtc", "tenant_id" },
            TableName,
            new object?[] { resident.Id, resident.TenantId, resident.Name, resident.Cpf, resident.Email, resident.Phone, (object?)resident.ApartmentId ?? DBNull.Value, resident.Active, resident.CreatedAtUtc, (object?)resident.UpdatedAtUtc ?? DBNull.Value, resident.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Resident resident, CancellationToken ct)
    {
        const int fieldCount = 7;
        var db = _factory.Create(_ctx);
        var updated = await db.UpdateAsync(
            new[] { "Name", "Cpf", "Email", "Phone", "ApartmentId", "Active", "UpdatedAtUtc" },
            TableName,
            new[] { resident.Name, resident.Cpf, resident.Email ?? string.Empty, resident.Phone ?? string.Empty, resident.ApartmentId.HasValue ? resident.ApartmentId.Value.ToString() : string.Empty, resident.Active ? "1" : "0", resident.UpdatedAtUtc.HasValue ? resident.UpdatedAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty },
            $"Id = @param{fieldCount}",
            new object[] { resident.Id.ToString() },
            ct: ct);

        if (!updated)
            throw new InvalidOperationException($"Failed to update resident {resident.Id}. {db.Error}");
    }

    private static Resident? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Resident> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Resident>();
        var list = new List<Resident>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Resident? MapRow(DataRow r)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new Resident(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            name: r["Name"]?.ToString() ?? string.Empty,
            cpf: r["Cpf"]?.ToString() ?? string.Empty,
            email: string.IsNullOrEmpty(r["Email"]?.ToString()) ? null : r["Email"].ToString(),
            phone: string.IsNullOrEmpty(r["Phone"]?.ToString()) ? null : r["Phone"].ToString(),
            apartmentId: string.IsNullOrEmpty(apartmentIdStr) ? null : Guid.Parse(apartmentIdStr),
            active: Convert.ToBoolean(r["Active"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}