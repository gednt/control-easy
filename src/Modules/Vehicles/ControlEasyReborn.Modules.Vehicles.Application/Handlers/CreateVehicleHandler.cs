using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Errors;
using ControlEasyReborn.Modules.Vehicles.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Vehicles.Application.Handlers;

public sealed class CreateVehicleHandler
{
    private readonly IVehicleRepository _vehicles;
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<CreateVehicleRequest> _validator;

    public CreateVehicleHandler(
        IVehicleRepository vehicles,
        IApartmentRepository apartments,
        IValidator<CreateVehicleRequest> validator)
    {
        _vehicles = vehicles;
        _apartments = apartments;
        _validator = validator;
    }

    public async Task<VehicleResponse> HandleAsync(CreateVehicleRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        if (request.ApartmentId.HasValue && !await _apartments.ExistsAsync(request.ApartmentId.Value, ct))
        {
            throw new NotFoundException("Apartment " + request.ApartmentId + " was not found.");
        }

        var vehicle = new Vehicle(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            plate: request.Plate,
            brand: request.Brand,
            model: request.Model,
            color: request.Color,
            apartmentId: request.ApartmentId,
            ownerName: request.OwnerName,
            vehicleType: request.VehicleType ?? Domain.Entities.VehicleType.Car,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _vehicles.AddAsync(vehicle, ct);
        return ToResponse(vehicle);
    }

    internal static VehicleResponse ToResponse(Vehicle v) =>
        new(v.Id, v.TenantId, v.Plate, v.Brand, v.Model, v.Color, v.ApartmentId, v.OwnerName, v.VehicleType, v.Active, v.CreatedAtUtc, v.UpdatedAtUtc);
}