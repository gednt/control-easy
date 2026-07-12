using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.SharedKernel.Demo;
using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class GetSessionHandler
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly ITenantRepository _tenants;
    private readonly ITenantContext _tenantContext;

    public GetSessionHandler(
        IUserRepository users,
        IAttendantProfileRepository profiles,
        ITenantRepository tenants,
        ITenantContext tenantContext)
    {
        _users = users;
        _profiles = profiles;
        _tenants = tenants;
        _tenantContext = tenantContext;
    }

    public async Task<SessionResponse> HandleAsync(CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("No tenant context in current token.");

        var profileId = _tenantContext.ProfileId
            ?? throw new UnauthorizedException("No active profile in current token.");

        var profile = await _profiles.FindAsync(profileId, ct)
            ?? throw new NotFoundException("Attendant profile " + profileId + " was not found.");

        var user = await _users.FindAsync(profile.UserId, ct)
            ?? throw new NotFoundException("User not found.");

        var currentTenant = await _tenants.FindAsync(tenantId, ct)
            ?? throw new NotFoundException("Tenant " + tenantId + " was not found.");

        var isPlatformAdmin = UserTenantAccess.IsPlatformAdmin(user.Roles);
        var switchableTenants = isPlatformAdmin
            ? Array.Empty<SessionTenantResponse>()
            : await BuildSwitchableTenantsAsync(user, ct);

        return new SessionResponse(
            TenantId: currentTenant.Id,
            TenantSlug: currentTenant.Slug,
            TenantDisplayName: currentTenant.DisplayName,
            UserDisplayName: profile.DisplayName ?? user.DisplayName,
            Roles: user.Roles,
            IsDemoPersona: DemoPersonas.IsDemoPersona(user.Email),
            SwitchableTenants: switchableTenants);
    }

    private async Task<IReadOnlyList<SessionTenantResponse>> BuildSwitchableTenantsAsync(
        User user,
        CancellationToken ct)
    {
        var userProfiles = await _profiles.ListByUserAsync(user.Id, ct);
        var results = new List<SessionTenantResponse>();

        foreach (var userProfile in userProfiles.Where(p => p.Active))
        {
            if (userProfile.TenantId == PlatformTenant.Id)
                continue;

            var tenant = await _tenants.FindAsync(userProfile.TenantId, ct);
            if (tenant is null)
                continue;

            results.Add(new SessionTenantResponse(
                TenantId: tenant.Id,
                Slug: tenant.Slug,
                DisplayName: tenant.DisplayName,
                UserDisplayName: userProfile.DisplayName ?? user.DisplayName));
        }

        return results;
    }
}
