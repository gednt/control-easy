using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class ListTenantsHandler
{
    private readonly ITenantRepository _tenants;

    public ListTenantsHandler(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    public async Task<IReadOnlyList<TenantResponse>> HandleAsync(CancellationToken ct)
    {
        var tenants = await _tenants.ListAsync(ct);
        return tenants.Select(CreateTenantHandler.ToResponse).ToList();
    }
}
