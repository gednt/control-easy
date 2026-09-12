using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Photos.Api.Endpoints;

public static class ConsentPolicyEndpoints
{
    public static IEndpointRouteBuilder MapConsentPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/consent-policy")
            .RequireAuthorization()
            .WithTags("Consent Policy");

        group.MapGet("/{subjectCategory}", async (
            string subjectCategory,
            ITenantContext tenantContext,
            UpdateConsentPolicyHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleGetAsync(tenantId, subjectCategory, ct);
            return response is null ? Results.NotFound() : Results.Ok(response);
        })
        .RequireAuthorization("Permission_Photos.Read");

        group.MapPut("/", async (
            [FromBody] UpdateConsentPolicyRequest request,
            ITenantContext tenantContext,
            UpdateConsentPolicyHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, tenantContext.ProfileId, ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("Permission_Photos.Write");

        return app;
    }
}