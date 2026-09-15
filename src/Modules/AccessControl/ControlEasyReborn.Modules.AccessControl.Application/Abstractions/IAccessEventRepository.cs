using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IAccessEventRepository
{
    Task<AccessEvent?> FindByScanAttemptAsync(Guid tenantId, Guid scanAttemptId, CancellationToken ct);
    Task<AccessEvent?> FindByIdAsync(Guid tenantId, Guid accessEventId, CancellationToken ct);
    Task<IReadOnlyList<AccessEvent>> ListAsync(Guid tenantId, DateTime? fromUtc, DateTime? toUtc, CycleDirection? direction, SubjectType? subjectType, Guid? subjectId, AccessMethod? accessMethod, CredentialStatus? credentialStatus, Guid? gatehouseId, Guid? attendantProfileId, int skip, int take, CancellationToken ct);
    Task AddAsync(AccessEvent accessEvent, CancellationToken ct);
}
