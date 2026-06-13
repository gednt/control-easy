using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Application.Handlers;
using ControlEasyReborn.Modules.Security.Application.Validators;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class LoginHandlerTests
{
    private readonly IUserRepository _users;
    private readonly IAttendantProfileRepository _profiles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<LoginRequest> _validator;
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _users = Substitute.For<IUserRepository>();
        _profiles = Substitute.For<IAttendantProfileRepository>();
        _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        _jwtService = Substitute.For<IJwtTokenService>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _validator = new LoginRequestValidator();
        _sut = new LoginHandler(_users, _profiles, _refreshTokens, _jwtService, _passwordHasher, _validator);
    }

    [Fact]
    public async Task HandleAsync_with_valid_credentials_returns_LoginResponse()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var user = new User(userId, tenantId, "admin@test.com", "hash", "Admin", true, false, "TenantAdmin", DateTime.UtcNow);
        var profile = new AttendantProfile(profileId, tenantId, userId, "Admin", null, null, "Visits.Read", true, DateTime.UtcNow);

        _users.FindByEmailAsync("admin@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hash").Returns(true);
        _profiles.ListByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(new List<AttendantProfile> { profile });
        _jwtService.GenerateAccessToken(userId, tenantId, profileId, "TenantAdmin", "Visits.Read").Returns("access-token");
        _jwtService.GenerateRefreshTokenAsync(userId, Arg.Any<CancellationToken>()).Returns(Task.FromResult("refresh-token"));

        var result = await _sut.HandleAsync(new LoginRequest("admin@test.com", "password123"), CancellationToken.None);

        result.Token.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task HandleAsync_with_wrong_password_throws_UnauthorizedException()
    {
        var user = new User(Guid.NewGuid(), Guid.NewGuid(), "admin@test.com", "hash", "Admin", true, false, "TenantAdmin", DateTime.UtcNow);
        _users.FindByEmailAsync("admin@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong", "hash").Returns(false);

        var act = () => _sut.HandleAsync(new LoginRequest("admin@test.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task HandleAsync_with_inactive_user_throws_UnauthorizedException()
    {
        var user = new User(Guid.NewGuid(), Guid.NewGuid(), "admin@test.com", "hash", "Admin", false, false, "TenantAdmin", DateTime.UtcNow);
        _users.FindByEmailAsync("admin@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("password123", "hash").Returns(true);

        var act = () => _sut.HandleAsync(new LoginRequest("admin@test.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User account is inactive.");
    }

    [Fact]
    public async Task HandleAsync_with_unknown_email_throws_UnauthorizedException()
    {
        _users.FindByEmailAsync("nobody@test.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _sut.HandleAsync(new LoginRequest("nobody@test.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Invalid email or password.");
    }
}