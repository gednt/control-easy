using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class ListPorteirosHandler
{
    private readonly ITenantAdminRepository _adminRepo;

    public ListPorteirosHandler(ITenantAdminRepository adminRepo)
    {
        _adminRepo = adminRepo;
    }

    public async Task<IReadOnlyList<PorteiroResponse>> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        return await _adminRepo.ListPorteirosAsync(tenantId, ct);
    }
}
