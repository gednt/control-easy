using ControlEasyReborn.Modules.Apartments.Domain.Entities;

namespace ControlEasyReborn.Modules.Apartments.Application.Abstractions;

public interface IApartmentRepository
{
    Task<Apartment?> FindAsync(Guid id, CancellationToken ct);
    Task<Apartment?> FindByBlockUnitAsync(string block, string unit, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Apartment>> ListAsync(string? search, int skip, int take, CancellationToken ct);
    Task AddAsync(Apartment apartment, CancellationToken ct);
    Task UpdateAsync(Apartment apartment, CancellationToken ct);
}
