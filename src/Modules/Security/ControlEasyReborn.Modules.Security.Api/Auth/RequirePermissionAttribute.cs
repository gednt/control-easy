using Microsoft.AspNetCore.Authorization;

namespace ControlEasyReborn.Modules.Security.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission) : base("Permission_" + permission)
    {
        Permission = permission;
    }
}