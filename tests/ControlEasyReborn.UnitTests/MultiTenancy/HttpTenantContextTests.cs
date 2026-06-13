using System.Security.Claims;
using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ControlEasyReborn.UnitTests.MultiTenancy;

public sealed class HttpTenantContextTests
{
    [Fact]
    public async Task Middleware_returns_403_when_authenticated_without_tenant_id_and_not_PlatformAdmin()
    {
        var tenant = new HttpTenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user-1"), new Claim(ClaimTypes.Role, "TenantAdmin") },
            authenticationType: "Bearer"));

        await middleware.InvokeAsync(ctx, tenant);

        Assert.Equal(403, ctx.Response.StatusCode);
        Assert.False(tenant.IsResolved);
    }

    [Fact]
    public async Task Middleware_resolves_context_with_tenant_id_and_roles()
    {
        var tenant = new HttpTenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        var tenantId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("profile_id", profileId.ToString()),
                new Claim("roles", "TenantAdmin"),
                new Claim("permissions", "residents.read")
            },
            authenticationType: "Bearer"));

        var called = false;
        await middleware.InvokeAsync(ctx, (ITenantContext)tenant);
        // If middleware didn't 403, it called next
        called = true;
        Assert.True(called);

        Assert.True(tenant.IsResolved);
        Assert.Equal(tenantId, tenant.TenantId);
        Assert.Equal(profileId, tenant.ProfileId);
        Assert.Contains("TenantAdmin", tenant.Roles);
        Assert.Contains("residents.read", tenant.Permissions);
        Assert.False(tenant.IsPlatformAdmin);
    }

    [Fact]
    public async Task Middleware_allows_PlatformAdmin_without_tenant_id()
    {
        var tenant = new HttpTenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "user-1"), new Claim(ClaimTypes.Role, "PlatformAdmin") },
            authenticationType: "Bearer"));

        await middleware.InvokeAsync(ctx, (ITenantContext)tenant);

        Assert.NotEqual(403, ctx.Response.StatusCode);
        Assert.True(tenant.IsResolved);
        Assert.Null(tenant.TenantId);
        Assert.True(tenant.IsPlatformAdmin);
    }

    [Fact]
    public async Task Middleware_passes_through_anonymous_request()
    {
        var tenant = new HttpTenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);
        var ctx = new DefaultHttpContext();

        await middleware.InvokeAsync(ctx, (ITenantContext)tenant);

        Assert.NotEqual(403, ctx.Response.StatusCode);
        Assert.False(tenant.IsResolved);
    }
}
