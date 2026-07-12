using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class ListTenantAdminsHandler
{
    private readonly ITenantAdminRepository _adminRepo;

    public ListTenantAdminsHandler(ITenantAdminRepository adminRepo)
    {
        _adminRepo = adminRepo;
    }

    public async Task<IReadOnlyList<TenantAdminResponse>> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        return await _adminRepo.ListAdminsAsync(tenantId, ct);
    }
}