using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.SharedKernel.Demo;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;
using ITenantAdminRepository = ControlEasyReborn.Modules.Tenants.Application.Abstractions.ITenantAdminRepository;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class LoginHandler
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly ITenantAdminRepository _adminProfiles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<LoginRequest> _validator;

    public LoginHandler(
        IUserRepository users,
        IAttendantProfileRepository profiles,
        ITenantAdminRepository adminProfiles,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        IValidator<LoginRequest> validator)
    {
        _users = users;
        _profiles = profiles;
        _adminProfiles = adminProfiles;
        _refreshTokens = refreshTokens;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var user = await _users.FindByEmailAsync(request.Email, ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.Active)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        var activeProfile = await AttendantProfileLoginSupport.ResolveActiveProfileAsync(
            user, _profiles, _adminProfiles, ct);
        if (activeProfile is null)
        {
            throw new UnauthorizedException("No active attendant profile found for user.");
        }

        var roles = user.Roles;
        var permissions = activeProfile.Permissions;
        var tenantId = activeProfile.TenantId;
        var profileId = activeProfile.Id;

        var token = _jwtService.GenerateAccessToken(user.Id, tenantId, profileId, roles, permissions);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id, ct);

        return new LoginResponse(
            token, refreshToken, tenantId, profileId, roles, permissions,
            user.MustChangePassword, DemoPersonas.IsDemoPersona(user.Email));
    }
}