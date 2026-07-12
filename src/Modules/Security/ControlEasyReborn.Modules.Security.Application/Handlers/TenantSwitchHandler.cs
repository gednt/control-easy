using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.SharedKernel.Demo;
using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class TenantSwitchHandler
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _jwtService;
    private readonly ITenantContext _tenantContext;

    public TenantSwitchHandler(
        IUserRepository users,
        IAttendantProfileRepository profiles,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwtService,
        ITenantContext tenantContext)
    {
        _users = users;
        _profiles = profiles;
        _refreshTokens = refreshTokens;
        _jwtService = jwtService;
        _tenantContext = tenantContext;
    }

    public async Task<TenantSwitchResponse> HandleAsync(TenantSwitchRequest request, CancellationToken ct)
    {
        var userIdClaim = _tenantContext.ProfileId;
        if (_tenantContext.TenantId is null)
        {
            throw new UnauthorizedException("No tenant context in current token.");
        }

        var userProfiles = await _profiles.ListByUserAsync(
            await GetUserIdFromContext(ct), ct);

        var targetProfile = userProfiles.FirstOrDefault(p =>
            p.TenantId == request.TenantId && p.Active);

        if (targetProfile is null)
        {
            throw new NotFoundException("No active profile found for tenant " + request.TenantId);
        }

        var user = await _users.FindAsync(targetProfile.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!UserTenantAccess.IsCondominiumTenant(request.TenantId, user.Roles))
        {
            throw new UnauthorizedException("You do not have access to that condominium.");
        }

        var token = _jwtService.GenerateAccessToken(
            user.Id, targetProfile.TenantId, targetProfile.Id,
            user.Roles, targetProfile.Permissions);

        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id, ct);

        return new TenantSwitchResponse(
            token, refreshToken,
            targetProfile.TenantId, targetProfile.Id,
            user.Roles, targetProfile.Permissions,
            DemoPersonas.IsDemoPersona(user.Email));
    }

    private async Task<Guid> GetUserIdFromContext(CancellationToken ct)
    {
        var profileId = _tenantContext.ProfileId
            ?? throw new UnauthorizedException("No profile in current token.");

        var profile = await _profiles.FindAsync(profileId, ct);
        return profile?.UserId ?? throw new UnauthorizedException("Current profile not found.");
    }
}