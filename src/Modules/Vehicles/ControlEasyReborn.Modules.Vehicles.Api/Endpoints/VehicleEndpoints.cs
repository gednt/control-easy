using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using ControlEasyReborn.Modules.Vehicles.Application.Errors;
using ControlEasyReborn.Modules.Vehicles.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Vehicles.Api.Endpoints;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/vehicles")
            .RequireAuthorization()
            .WithTags("Vehicles");

        group.MapGet("/", async (
            string? search,
            int? skip,
            int? take,
            ListVehiclesHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(search, skip ?? 0, take ?? 50, ct);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetVehicleHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/", async (
            [FromBody] CreateVehicleRequest request,
            ITenantContext tenantContext,
            CreateVehicleHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, ct);
            return Results.Created($"/api/v1/vehicles/{response.Id}", response);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateVehicleRequest request,
            UpdateVehicleHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request, ct);
            return Results.Ok(response);
        });

        return app;
    }
}

public static class VehicleProblemDetailsWriter
{
    public static IApplicationBuilder UseVehicleExceptionHandler(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (NotFoundException ex)
            {
                await Write(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
            }
            catch (ConflictException ex)
            {
                await Write(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
            }
            catch (ValidationException ex)
            {
                var pd = new
                {
                    type = "https://httpstatuses.io/400",
                    title = "Validation failed",
                    status = 400,
                    detail = ex.Message,
                    errors = ex.Errors
                };
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(pd);
            }
        });
    }

    private static async Task Write(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.io/" + status,
            title,
            status,
            detail
        });
    }
}