using Microsoft.AspNetCore.Authorization;

namespace ControlEasyReborn.Modules.Security.Api.Auth;

public sealed class RequirePermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public RequirePermissionRequirement(string permission)
    {
        Permission = permission;
    }
}