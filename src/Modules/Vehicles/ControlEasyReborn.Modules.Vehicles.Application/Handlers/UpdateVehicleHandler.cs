using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Vehicles.Application.Handlers;

public sealed class UpdateVehicleHandler
{
    private readonly IVehicleRepository _vehicles;
    private readonly IValidator<UpdateVehicleRequest> _validator;

    public UpdateVehicleHandler(IVehicleRepository vehicles, IValidator<UpdateVehicleRequest> validator)
    {
        _vehicles = vehicles;
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

        vehicle.UpdateDetails(request.Plate, request.Brand, request.Model, request.Color, request.ApartmentId, request.OwnerName, request.VehicleType ?? Domain.Entities.VehicleType.Car);
        await _vehicles.UpdateAsync(vehicle, ct);
        return CreateVehicleHandler.ToResponse(vehicle);
    }
}