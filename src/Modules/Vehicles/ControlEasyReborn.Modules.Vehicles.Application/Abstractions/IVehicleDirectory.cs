using ControlEasyReborn.Modules.Vehicles.Domain.Entities;

namespace ControlEasyReborn.Modules.Vehicles.Application.Abstractions;

public interface IVehicleDirectory
{
    Task<Vehicle?> FindActiveAsync(Guid tenantId, Guid vehicleId, CancellationToken ct);
    Task<IReadOnlyList<Vehicle>> ListByOwnerResidentAsync(Guid tenantId, Guid residentId, CancellationToken ct);
    Task<IReadOnlyList<Vehicle>> ListByApartmentAsync(Guid tenantId, Guid apartmentId, CancellationToken ct);
}
