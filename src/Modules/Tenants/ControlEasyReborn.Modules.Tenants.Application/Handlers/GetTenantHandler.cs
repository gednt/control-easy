using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class GetTenantHandler
{
    private readonly ITenantRepository _tenants;

    public GetTenantHandler(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    public async Task<TenantResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(id, ct);
        if (tenant is null)
        {
            throw new NotFoundException("Tenant " + id + " was not found.");
        }
        return CreateTenantHandler.ToResponse(tenant);
    }
}
