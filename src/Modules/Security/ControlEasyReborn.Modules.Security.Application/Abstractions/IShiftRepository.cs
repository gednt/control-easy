using ControlEasyReborn.Modules.Security.Domain.Entities;

namespace ControlEasyReborn.Modules.Security.Application.Abstractions;

public interface IShiftRepository
{
    Task<Shift?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Shift>> ListByTenantAsync(Guid tenantId, CancellationToken ct);
    Task AddAsync(Shift shift, CancellationToken ct);
    Task UpdateAsync(Shift shift, CancellationToken ct);
}