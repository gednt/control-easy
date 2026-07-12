using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Contracts;

namespace ControlEasyReborn.Modules.Vehicles.Application.Handlers;

public sealed class ListVehiclesHandler
{
    private readonly IVehicleRepository _vehicles;

    public ListVehiclesHandler(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    public async Task<IReadOnlyList<VehicleResponse>> HandleAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var vehicles = await _vehicles.ListAsync(search, skip, take, ct);
        return vehicles.Select(CreateVehicleHandler.ToResponse).ToList();
    }
}