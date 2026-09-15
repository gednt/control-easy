using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Domain.Entities;
using ControlEasyReborn.Modules.Vehicles.Infrastructure.Persistence;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Vehicles.Infrastructure.Persistence;

public sealed class VehicleDirectoryRepository : IVehicleDirectory
{
    private const string TableName = "Vehicles";
    private const string Fields = "Id, TenantId, Plate, Brand, Model, Color, ApartmentId, OwnerResidentId, OwnerName, VehicleType, Active, CreatedAtUtc, UpdatedAtUtc";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public VehicleDirectoryRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Vehicle?> FindActiveAsync(Guid tenantId, Guid vehicleId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "Id = @param0 AND Active = 1",
            parameters: new object[] { vehicleId },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Vehicle>> ListByOwnerResidentAsync(Guid tenantId, Guid residentId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "OwnerResidentId = @param0 AND Active = 1",
            parameters: new object[] { residentId },
            ct: ct);
        return MapList(rows).ToList();
    }

    public async Task<IReadOnlyList<Vehicle>> ListByApartmentAsync(Guid tenantId, Guid apartmentId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "ApartmentId = @param0 AND Active = 1",
            parameters: new object[] { apartmentId },
            ct: ct);
        return MapList(rows).ToList();
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
        var ownerResidentIdStr = r["OwnerResidentId"]?.ToString();
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
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            ownerResidentId: string.IsNullOrEmpty(ownerResidentIdStr) ? null : Guid.Parse(ownerResidentIdStr));
    }
}
