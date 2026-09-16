using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Vehicles.Application.Handlers;

public sealed class UpdateVehicleHandler
{
    private readonly IVehicleRepository _vehicles;
    private readonly IResidentDirectory _residents;
    private readonly IValidator<UpdateVehicleRequest> _validator;

    public UpdateVehicleHandler(IVehicleRepository vehicles, IResidentDirectory residents, IValidator<UpdateVehicleRequest> validator)
    {
        _vehicles = vehicles;
        _residents = residents;
        _validator = validator;
    }

    public async Task<VehicleResponse> HandleAsync(Guid id, UpdateVehicleRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var vehicle = await _vehicles.FindAsync(id, ct);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle " + id + " was not found.");
        }

        if (request.OwnerResidentId.HasValue && request.OwnerResidentId.Value != Guid.Empty)
        {
            var tenantId = vehicle.TenantId;
            var owner = await _residents.FindByIdAsync(tenantId, request.OwnerResidentId.Value, ct);
            if (owner is null)
            {
                throw new Errors.ValidationException(new Dictionary<string, string[]>
                {
                    ["OwnerResidentId"] = new[] { "Owner resident must belong to the current tenant." }
                });
            }
        }

        vehicle.UpdateDetails(request.Plate, request.Brand, request.Model, request.Color, request.ApartmentId, request.OwnerName, request.VehicleType ?? Domain.Entities.VehicleType.Car, request.OwnerResidentId);
        await _vehicles.UpdateAsync(vehicle, ct);
        return CreateVehicleHandler.ToResponse(vehicle);
    }
}