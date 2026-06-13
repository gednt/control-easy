using ControlEasyReborn.Modules.Tenants.Application.Abstractions;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class RevokeTenantAdminHandler
{
    private readonly ITenantAdminRepository _adminRepo;

    public RevokeTenantAdminHandler(ITenantAdminRepository adminRepo)
    {
        _adminRepo = adminRepo;
    }

    public async Task HandleAsync(Guid userId, CancellationToken ct)
    {
        await _adminRepo.RevokeAdminAsync(userId, ct);
    }
}