using Microsoft.AspNetCore.Authorization;

namespace ControlEasyReborn.Modules.Tenants.Api.Auth;

public sealed class PlatformAdminRequirement : IAuthorizationRequirement
{
    public const string PolicyName = "PlatformAdminOnly";
}
