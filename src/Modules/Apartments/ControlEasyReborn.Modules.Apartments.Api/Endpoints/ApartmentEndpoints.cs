using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Apartments.Api.Endpoints;

public static class ApartmentEndpoints
{
    public static IEndpointRouteBuilder MapApartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/apartments")
            .RequireAuthorization()
            .WithTags("Apartments");

        group.MapGet("/", async (
            string? search,
            int? skip,
            int? take,
            ListApartmentsHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(search, skip ?? 0, take ?? 50, ct);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetApartmentHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/", async (
            [FromBody] CreateApartmentRequest request,
            ITenantContext tenantContext,
            CreateApartmentHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, ct);
            return Results.Created($"/api/v1/apartments/{response.Id}", response);
        }).RequireAuthorization("Permission_Apartments.Write");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateApartmentRequest request,
            UpdateApartmentHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request, ct);
            return Results.Ok(response);
        }).RequireAuthorization("Permission_Apartments.Write");

        return app;
    }
}
