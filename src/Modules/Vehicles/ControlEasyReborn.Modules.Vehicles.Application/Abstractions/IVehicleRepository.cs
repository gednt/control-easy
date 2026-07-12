using ControlEasyReborn.Modules.Vehicles.Domain.Entities;

namespace ControlEasyReborn.Modules.Vehicles.Application.Abstractions;

public interface IVehicleRepository
{
    Task<Vehicle?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Vehicle>> ListAsync(string? search, int skip, int take, CancellationToken ct);
    Task AddAsync(Vehicle vehicle, CancellationToken ct);
    Task UpdateAsync(Vehicle vehicle, CancellationToken ct);
}