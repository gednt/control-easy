using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IAccessCredentialRepository
{
    Task<AccessCredential?> FindActiveAsync(Guid tenantId, SubjectType subjectType, Guid subjectId, CancellationToken ct);
    Task<AccessCredential?> FindByIdAsync(Guid tenantId, Guid credentialId, CancellationToken ct);
    Task<IReadOnlyList<AccessCredential>> ListAllAsync(Guid tenantId, CancellationToken ct);
    Task<IReadOnlyList<AccessCredential>> ListAsync(Guid tenantId, int skip, int take, CancellationToken ct, Guid? subjectId = null, SubjectType? subjectType = null, CredentialStatus? status = null);
    Task AddAsync(AccessCredential credential, CancellationToken ct);
    Task ReplaceAsync(AccessCredential predecessor, AccessCredential successor, CredentialLifecycleAction action, CancellationToken ct);
    Task UpdateStatusAsync(AccessCredential credential, CancellationToken ct);
}
