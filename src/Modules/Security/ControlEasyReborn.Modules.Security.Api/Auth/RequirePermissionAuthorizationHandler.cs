using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ControlEasyReborn.Modules.Security.Api.Auth;

public sealed class RequirePermissionAuthorizationHandler : AuthorizationHandler<RequirePermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequirePermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions").Select(c => c.Value);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}