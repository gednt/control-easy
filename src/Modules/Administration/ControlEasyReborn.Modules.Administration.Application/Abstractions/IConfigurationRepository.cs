using ControlEasyReborn.Modules.Administration.Domain.Entities;

namespace ControlEasyReborn.Modules.Administration.Application.Abstractions;

public interface IConfigurationRepository
{
    Task<ConfigurationEntry?> FindAsync(Guid id, CancellationToken ct);
    Task<ConfigurationEntry?> GetByKeyAsync(Guid tenantId, string key, CancellationToken ct);
    Task<IReadOnlyList<ConfigurationEntry>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken ct);
    Task AddAsync(ConfigurationEntry entry, CancellationToken ct);
    Task UpdateAsync(ConfigurationEntry entry, CancellationToken ct);
}