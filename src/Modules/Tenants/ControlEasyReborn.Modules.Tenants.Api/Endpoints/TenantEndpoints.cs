using ControlEasyReborn.Modules.Tenants.Api.Auth;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;
using ControlEasyReborn.Modules.Tenants.Application.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ControlEasyReborn.Modules.Tenants.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants")
            .RequireAuthorization(PlatformAdminRequirement.PolicyName)
            .WithTags("Tenants");

        group.MapPost("/", async (
            [FromBody] CreateTenantRequest request,
            CreateTenantHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/v1/tenants/{response.Id}", response);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetTenantHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/suspend", async (
            Guid id,
            [FromBody] SuspendTenantRequest? request,
            SuspendTenantHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request ?? new SuspendTenantRequest(null), ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/resume", async (
            Guid id,
            [FromBody] ResumeTenantRequest? request,
            ResumeTenantHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request ?? new ResumeTenantRequest(null), ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/admins", async (
            Guid id,
            [FromBody] CreateTenantAdminRequest request,
            CreateTenantAdminHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request, ct);
            return Results.Created($"/api/v1/tenants/{id}/admins/{response.UserId}", response);
        });

        group.MapGet("/{id:guid}/admins", async (
            Guid id,
            ListTenantAdminsHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/admins/{userId:guid}/revoke", async (
            Guid id,
            Guid userId,
            RevokeTenantAdminHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(userId, ct);
            return Results.NoContent();
        });

        return app;
    }

    public static IEndpointRouteBuilder MapTenantBackupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/backups")
            .RequireAuthorization(PlatformAdminRequirement.PolicyName)
            .WithTags("Admin");

        group.MapPost("/{tenantId:guid}", async (
            Guid tenantId,
            CreateTenantBackupHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(tenantId, ct);
            return Results.Ok(response);
        });

        return app;
    }
}

public static class TenantProblemDetailsWriter
{
    public static IApplicationBuilder UseTenantExceptionHandler(this IApplicationBuilder app)
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