using ControlEasyReborn.Modules.Residents.Domain.Entities;

namespace ControlEasyReborn.Modules.Residents.Application.Abstractions;

public interface IResidentRepository
{
    Task<Resident?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Resident>> ListAsync(string? search, int skip, int take, CancellationToken ct, Guid? apartmentId = null);
    Task AddAsync(Resident resident, CancellationToken ct);
    Task UpdateAsync(Resident resident, CancellationToken ct);
}