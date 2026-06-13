using ControlEasyReborn.Modules.Vehicles.Domain.Entities;

namespace ControlEasyReborn.Modules.Vehicles.Application.Contracts;

public sealed record CreateVehicleRequest(string Plate, string? Brand, string? Model, string? Color, Guid? ApartmentId, string? OwnerName, VehicleType? VehicleType);

public sealed record UpdateVehicleRequest(string Plate, string? Brand, string? Model, string? Color, Guid? ApartmentId, string? OwnerName, VehicleType? VehicleType);

public sealed record VehicleResponse(
    Guid Id,
    Guid TenantId,
    string Plate,
    string? Brand,
    string? Model,
    string? Color,
    Guid? ApartmentId,
    string? OwnerName,
    VehicleType VehicleType,
    bool Active,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);