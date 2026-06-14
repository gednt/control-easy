using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

internal static class UserTenantAccess
{
    public static bool IsPlatformAdmin(string roles) =>
        roles.Contains("PlatformAdmin", StringComparison.Ordinal);

    public static bool IsCondominiumTenant(Guid tenantId, string roles) =>
        IsPlatformAdmin(roles) || tenantId != PlatformTenant.Id;
}
