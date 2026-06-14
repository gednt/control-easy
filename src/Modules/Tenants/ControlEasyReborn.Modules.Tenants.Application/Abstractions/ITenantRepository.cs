using ControlEasyReborn.Modules.Tenants.Domain;

namespace ControlEasyReborn.Modules.Tenants.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> FindAsync(Guid id, CancellationToken ct);
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken ct);
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken ct);
    Task AddAsync(Tenant tenant, CancellationToken ct);
    Task UpdateAsync(Tenant tenant, CancellationToken ct);
}
