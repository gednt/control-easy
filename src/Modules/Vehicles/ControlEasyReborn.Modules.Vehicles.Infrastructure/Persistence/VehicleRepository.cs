using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Vehicles.Infrastructure.Persistence;

public sealed class VehicleRepository : IVehicleRepository
{
    private const string TableName = "Vehicles";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public VehicleRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Vehicle?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Plate, Brand, Model, Color, ApartmentId, OwnerName, VehicleType, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Vehicle>> ListAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereClause = string.IsNullOrWhiteSpace(search)
            ? "1=1"
            : "(Plate LIKE @param0 OR Brand LIKE @param0 OR Model LIKE @param0 OR OwnerName LIKE @param0)";
        var parameters = string.IsNullOrWhiteSpace(search)
            ? Array.Empty<object>()
            : new object[] { "%" + search + "%" };

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Plate, Brand, Model, Color, ApartmentId, OwnerName, VehicleType, Active, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters,
            ct: ct);

        return MapList(rows);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Plate", "Brand", "Model", "Color", "ApartmentId", "OwnerName", "VehicleType", "Active", "CreatedAtUtc", "UpdatedAtUtc" },
            TableName,
            new object?[] { vehicle.Id, vehicle.TenantId, vehicle.Plate, vehicle.Brand, vehicle.Model, vehicle.Color, (object?)vehicle.ApartmentId ?? DBNull.Value, vehicle.OwnerName, (int)vehicle.VehicleType, vehicle.Active, vehicle.CreatedAtUtc, (object?)vehicle.UpdatedAtUtc ?? DBNull.Value },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Vehicle vehicle, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "Plate", "Brand", "Model", "Color", "ApartmentId", "OwnerName", "VehicleType", "Active", "UpdatedAtUtc" },
            TableName,
            new[] { vehicle.Plate, vehicle.Brand ?? string.Empty, vehicle.Model ?? string.Empty, vehicle.Color ?? string.Empty, vehicle.ApartmentId.HasValue ? vehicle.ApartmentId.Value.ToString() : string.Empty, vehicle.OwnerName ?? string.Empty, ((int)vehicle.VehicleType).ToString(), vehicle.Active ? "1" : "0", vehicle.UpdatedAtUtc.HasValue ? vehicle.UpdatedAtUtc.Value.ToString("o") : string.Empty },
            "Id = @param0",
            new object[] { vehicle.Id },
            ct: ct);
    }

    private static Vehicle? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Vehicle> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Vehicle>();
        var list = new List<Vehicle>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Vehicle? MapRow(DataRow r)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new Vehicle(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            plate: r["Plate"]?.ToString() ?? string.Empty,
            brand: string.IsNullOrEmpty(r["Brand"]?.ToString()) ? null : r["Brand"].ToString(),
            model: string.IsNullOrEmpty(r["Model"]?.ToString()) ? null : r["Model"].ToString(),
            color: string.IsNullOrEmpty(r["Color"]?.ToString()) ? null : r["Color"].ToString(),
            apartmentId: string.IsNullOrEmpty(apartmentIdStr) ? null : Guid.Parse(apartmentIdStr),
            ownerName: string.IsNullOrEmpty(r["OwnerName"]?.ToString()) ? null : r["OwnerName"].ToString(),
            vehicleType: (VehicleType)Convert.ToInt32(r["VehicleType"]),
            active: Convert.ToBoolean(r["Active"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}