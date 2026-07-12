using System.Security.Claims;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ControlEasyReborn.Infrastructure.MultiTenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (tenantContext is HttpTenantContext http)
        {
            http.Reset();
        }

        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(user.FindAll("roles").Select(c => c.Value))
            .Distinct()
            .ToArray();
        var permissions = user.FindAll("permissions").Select(c => c.Value).Distinct().ToArray();
        var isPlatformAdmin = roles.Contains("PlatformAdmin");

        Guid? tenantId = null;
        var tenantClaim = user.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantClaim) && Guid.TryParse(tenantClaim, out var parsed))
        {
            tenantId = parsed;
        }

        Guid? profileId = null;
        var profileClaim = user.FindFirst("profile_id")?.Value;
        if (!string.IsNullOrWhiteSpace(profileClaim) && Guid.TryParse(profileClaim, out var parsedProfile))
        {
            profileId = parsedProfile;
        }

        if (!isPlatformAdmin && tenantId is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.io/403",
                title = "Tenant context required",
                status = 403,
                detail = "Requests must carry a tenant_id claim unless the caller has the PlatformAdmin role."
            });
            return;
        }

        tenantContext.Set(tenantId, profileId, roles, permissions);
        await _next(context);
    }
}
