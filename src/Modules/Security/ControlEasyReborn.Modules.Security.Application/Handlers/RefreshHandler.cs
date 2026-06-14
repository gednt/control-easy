using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using ITenantAdminRepository = ControlEasyReborn.Modules.Tenants.Application.Abstractions.ITenantAdminRepository;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class RefreshHandler
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly ITenantAdminRepository _adminProfiles;
    private readonly IJwtTokenService _jwtService;
    private readonly IValidator<RefreshRequest> _validator;

    public RefreshHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IAttendantProfileRepository profiles,
        ITenantAdminRepository adminProfiles,
        IJwtTokenService jwtService,
        IValidator<RefreshRequest> validator)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _profiles = profiles;
        _adminProfiles = adminProfiles;
        _jwtService = jwtService;
        _validator = validator;
    }

    public async Task<RefreshResponse> HandleAsync(RefreshRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var entry = await _refreshTokens.FindByTokenAsync(request.RefreshToken, ct);
        if (entry is null || entry.IsRevoked || entry.IsExpired)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        await _refreshTokens.RevokeAsync(entry, ct);

        var user = await _users.FindAsync(entry.UserId, ct);
        if (user is null || !user.Active)
        {
            throw new UnauthorizedException("User not found or inactive.");
        }

        var activeProfile = await AttendantProfileLoginSupport.ResolveActiveProfileAsync(
            user, _profiles, _adminProfiles, ct);
        if (activeProfile is null)
        {
            throw new UnauthorizedException("No active attendant profile found for user.");
        }

        var token = _jwtService.GenerateAccessToken(user.Id, activeProfile.TenantId, activeProfile.Id, user.Roles, activeProfile.Permissions);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id, ct);

        return new RefreshResponse(token, refreshToken);
    }
}