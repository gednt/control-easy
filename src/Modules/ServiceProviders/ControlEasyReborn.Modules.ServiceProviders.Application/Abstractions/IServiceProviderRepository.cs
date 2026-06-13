using ControlEasyReborn.Modules.ServiceProviders.Domain.Entities;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Abstractions;

public interface IServiceProviderRepository
{
    Task<ServiceProvider?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ServiceProvider>> ListAsync(string? search, int skip, int take, CancellationToken ct);
    Task AddAsync(ServiceProvider provider, CancellationToken ct);
    Task UpdateAsync(ServiceProvider provider, CancellationToken ct);
}