using ControlEasyReborn.Modules.AccessControl.Domain.Entities;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface ICredentialLifecycleActionRepository
{
    Task AddAsync(CredentialLifecycleAction action, CancellationToken ct);
    Task<IReadOnlyList<CredentialLifecycleAction>> ListAsync(Guid tenantId, Guid credentialId, CancellationToken ct);
}
