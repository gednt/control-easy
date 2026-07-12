using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ControlEasyReborn.Modules.Tenants.Api.Auth;

public sealed class PlatformAdminAuthorizationHandler : AuthorizationHandler<PlatformAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformAdminRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(context.User.FindAll("roles").Select(c => c.Value));
        if (roles.Any(r => string.Equals(r, "PlatformAdmin", StringComparison.Ordinal)))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
