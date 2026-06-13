using ControlEasyReborn.Modules.Security.Domain.Entities;

namespace ControlEasyReborn.Modules.Security.Application.Abstractions;

public interface IAttendantProfileRepository
{
    Task<AttendantProfile?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AttendantProfile>> ListByTenantAsync(Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<AttendantProfile>> ListByUserAsync(Guid userId, CancellationToken ct);
    Task CreateAsync(AttendantProfile profile, CancellationToken ct);
    Task UpdateAsync(AttendantProfile profile, CancellationToken ct);
    Task DeactivateAsync(AttendantProfile profile, CancellationToken ct);
}