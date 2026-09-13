using ControlEasyReborn.Modules.Administration.Domain.Entities;

namespace ControlEasyReborn.Modules.Administration.Application.Abstractions;

public interface ICondominiumSettingsRepository
{
    Task<CondominiumSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct);
    Task SaveAsync(CondominiumSettings settings, CancellationToken ct);
}
