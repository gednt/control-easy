using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class TenantLookupHandler
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly ITenantRepository _tenants;

    public TenantLookupHandler(
        IUserRepository users,
        IAttendantProfileRepository profiles,
        ITenantRepository tenants)
    {
        _users = users;
        _profiles = profiles;
        _tenants = tenants;
    }

    public async Task<IReadOnlyList<TenantLookupResponse>> HandleAsync(string email, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email, ct);
        if (user is null)
        {
            return Array.Empty<TenantLookupResponse>();
        }

        var userProfiles = await _profiles.ListByUserAsync(user.Id, ct);
        var activeProfiles = userProfiles.Where(p => p.Active).ToList();
        if (activeProfiles.Count == 0)
        {
            return Array.Empty<TenantLookupResponse>();
        }

        var results = new List<TenantLookupResponse>();
        foreach (var profile in activeProfiles)
        {
            var tenant = await _tenants.FindAsync(profile.TenantId, ct);
            if (tenant is null) continue;

            results.Add(new TenantLookupResponse(
                TenantId: tenant.Id,
                Slug: tenant.Slug,
                DisplayName: tenant.DisplayName,
                UserDisplayName: profile.DisplayName ?? user.DisplayName));
        }

        return results;
    }
}