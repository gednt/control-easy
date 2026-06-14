using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Application.Handlers;
using ControlEasyReborn.Modules.Security.Application.Validators;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using ControlEasyReborn.SharedKernel.Bootstrap;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class ChangePasswordHandlerTests
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<ChangePasswordRequest> _validator;
    private readonly IBootstrapCredentialsStore _bootstrapCredentialsStore;
    private readonly ChangePasswordHandler _sut;

    public ChangePasswordHandlerTests()
    {
        _users = Substitute.For<IUserRepository>();
        _profiles = Substitute.For<IAttendantProfileRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tenantContext = Substitute.For<ITenantContext>();
        _validator = new ChangePasswordRequestValidator();
        _bootstrapCredentialsStore = Substitute.For<IBootstrapCredentialsStore>();
        _sut = new ChangePasswordHandler(
            _users, _profiles, _passwordHasher, _tenantContext, _validator, _bootstrapCredentialsStore);
    }

    [Fact]
    public async Task HandleAsync_with_valid_request_updates_password_and_clears_must_change()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new User(userId, tenantId, "admin@test.com", "old-hash", "Admin", true, true, "PlatformAdmin", DateTime.UtcNow);
        var profile = new AttendantProfile(profileId, tenantId, userId, "Admin", null, null, "platform:*", true, DateTime.UtcNow);

        _tenantContext.ProfileId.Returns(profileId);
        _profiles.FindAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("current", "old-hash").Returns(true);
        _passwordHasher.Hash("new-password").Returns("new-hash");

        await _sut.HandleAsync(new ChangePasswordRequest("current", "new-password"), CancellationToken.None);

        user.MustChangePassword.Should().BeFalse();
        user.PasswordHash.Should().Be("new-hash");
        await _users.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        _bootstrapCredentialsStore.Received(1).Clear();
    }

    [Fact]
    public async Task HandleAsync_with_wrong_current_password_throws_UnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new User(userId, tenantId, "admin@test.com", "old-hash", "Admin", true, true, "PlatformAdmin", DateTime.UtcNow);
        var profile = new AttendantProfile(profileId, tenantId, userId, "Admin", null, null, "platform:*", true, DateTime.UtcNow);

        _tenantContext.ProfileId.Returns(profileId);
        _profiles.FindAsync(profileId, Arg.Any<CancellationToken>()).Returns(profile);
        _users.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong", "old-hash").Returns(false);

        var act = () => _sut.HandleAsync(new ChangePasswordRequest("wrong", "new-password"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Current password is incorrect.");
    }
}
