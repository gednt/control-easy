using System.Security.Claims;
using ControlEasyReborn.Modules.Security.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Security;

public sealed class RequirePermissionAuthorizationHandlerTests
{
    private readonly RequirePermissionAuthorizationHandler _sut = new();

    [Fact]
    public async Task Handler_succeeds_when_user_has_required_permission()
    {
        var requirement = new RequirePermissionRequirement("Visits.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("permissions", "Visits.Read"),
            new Claim("permissions", "Residents.Read"),
        ], "TestAuth"));

        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_does_not_succeed_when_user_lacks_required_permission()
    {
        var requirement = new RequirePermissionRequirement("Visits.Write");
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("permissions", "Visits.Read"),
        ], "TestAuth"));

        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_does_not_succeed_when_user_has_no_permissions_claims()
    {
        var requirement = new RequirePermissionRequirement("Visits.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth"));

        var context = new AuthorizationHandlerContext([requirement], user, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}