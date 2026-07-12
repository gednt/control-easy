using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Errors;
using ControlEasyReborn.Modules.Residents.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Residents.Api.Endpoints;

public static class ResidentEndpoints
{
    public static IEndpointRouteBuilder MapResidentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/residents")
            .RequireAuthorization()
            .WithTags("Residents");

        group.MapGet("/", async (
            string? search,
            int? skip,
            int? take,
            Guid? apartmentId,
            ListResidentsHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(search, skip ?? 0, take ?? 50, ct, apartmentId);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetResidentHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/", async (
            [FromBody] CreateResidentRequest request,
            ITenantContext tenantContext,
            CreateResidentHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, ct);
            return Results.Created($"/api/v1/residents/{response.Id}", response);
        }).RequireAuthorization("Permission_Residents.Write");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateResidentRequest request,
            UpdateResidentHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request, ct);
            return Results.Ok(response);
        }).RequireAuthorization("Permission_Residents.Write");

        return app;
    }
}

public static class ResidentProblemDetailsWriter
{
    public static IApplicationBuilder UseResidentExceptionHandler(this IApplicationBuilder app)
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