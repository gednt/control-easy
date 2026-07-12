using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Errors;

namespace ControlEasyReborn.Modules.Vehicles.Application.Handlers;

public sealed class GetVehicleHandler
{
    private readonly IVehicleRepository _vehicles;

    public GetVehicleHandler(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    public async Task<VehicleResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var vehicle = await _vehicles.FindAsync(id, ct);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle " + id + " was not found.");
        }
        return CreateVehicleHandler.ToResponse(vehicle);
    }
}