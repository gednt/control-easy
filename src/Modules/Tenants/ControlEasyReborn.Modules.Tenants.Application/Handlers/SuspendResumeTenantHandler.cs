using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class SuspendTenantHandler
{
    private readonly ITenantRepository _tenants;

    public SuspendTenantHandler(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    public async Task<TenantResponse> HandleAsync(Guid id, SuspendTenantRequest request, CancellationToken ct)
    {
        _ = request;
        var tenant = await _tenants.FindAsync(id, ct);
        if (tenant is null)
        {
            throw new NotFoundException("Tenant " + id + " was not found.");
        }
        tenant.Suspend();
        await _tenants.UpdateAsync(tenant, ct);
        return CreateTenantHandler.ToResponse(tenant);
    }
}

public sealed class ResumeTenantHandler
{
    private readonly ITenantRepository _tenants;

    public ResumeTenantHandler(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    public async Task<TenantResponse> HandleAsync(Guid id, ResumeTenantRequest request, CancellationToken ct)
    {
        _ = request;
        var tenant = await _tenants.FindAsync(id, ct);
        if (tenant is null)
        {
            throw new NotFoundException("Tenant " + id + " was not found.");
        }
        tenant.Resume();
        await _tenants.UpdateAsync(tenant, ct);
        return CreateTenantHandler.ToResponse(tenant);
    }
}
