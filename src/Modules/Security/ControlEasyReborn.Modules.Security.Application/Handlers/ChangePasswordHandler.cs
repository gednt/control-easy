using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.SharedKernel.Bootstrap;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class ChangePasswordHandler
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<ChangePasswordRequest> _validator;
    private readonly IBootstrapCredentialsStore _bootstrapCredentialsStore;

    public ChangePasswordHandler(
        IUserRepository users,
        IAttendantProfileRepository profiles,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext,
        IValidator<ChangePasswordRequest> validator,
        IBootstrapCredentialsStore bootstrapCredentialsStore)
    {
        _users = users;
        _profiles = profiles;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
        _validator = validator;
        _bootstrapCredentialsStore = bootstrapCredentialsStore;
    }

    public async Task HandleAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var user = await FindCurrentUserAsync(ct);
        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        await _users.UpdateAsync(user, ct);

        if (user.Roles.Contains("PlatformAdmin", StringComparison.Ordinal))
            _bootstrapCredentialsStore.Clear();
    }

    private async Task<Domain.Entities.User> FindCurrentUserAsync(CancellationToken ct)
    {
        var profileId = _tenantContext.ProfileId
            ?? throw new UnauthorizedException("No profile in current token.");

        var profile = await _profiles.FindAsync(profileId, ct)
            ?? throw new UnauthorizedException("Current profile not found.");

        return await _users.FindAsync(profile.UserId, ct)
            ?? throw new UnauthorizedException("User not found.");
    }
}
