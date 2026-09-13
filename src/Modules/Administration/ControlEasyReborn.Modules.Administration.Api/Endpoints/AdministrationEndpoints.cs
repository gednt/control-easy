using System.Security.Claims;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Administration.Api.Endpoints;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/administration")
            .RequireAuthorization()
            .WithTags("Administration");

        // Settings Endpoints
        group.MapGet("/settings", async (
            GetCondominiumSettingsHandler handler,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(tenantId, ct);
            return Results.Ok(response);
        });

        group.MapPut("/settings", async (
            [FromBody] UpdateCondominiumSettingsRequest request,
            ClaimsPrincipal principal,
            UpdateCondominiumSettingsHandler handler,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");

            Guid? userId = null;
            var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;
            if (Guid.TryParse(subClaim, out var parsedId))
            {
                userId = parsedId;
            }

            var userName = principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst("name")?.Value
                ?? principal.FindFirst(ClaimTypes.Email)?.Value;

            var response = await handler.HandleAsync(tenantId, request, userId, userName, ct);
            return Results.Ok(response);
        });

        // Audit Log Endpoints
        group.MapGet("/audit-logs", async (
            string? category,
            string? severity,
            string? entityType,
            string? action,
            DateTime? fromUtc,
            DateTime? toUtc,
            string? searchTerm,
            int? skip,
            int? take,
            ListAuditLogHandler handler,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(
                tenantId,
                category,
                severity,
                entityType,
                action,
                fromUtc,
                toUtc,
                searchTerm,
                skip ?? 0,
                take ?? 50,
                ct);
            return Results.Ok(response);
        });

        group.MapGet("/audit-logs/{id:guid}", async (
            Guid id,
            GetAuditLogByIdHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapPost("/audit-logs", async (
            [FromBody] CreateAuditLogRequest request,
            ITenantContext tenantContext,
            CreateAuditLogHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, ct);
            return Results.Created($"/api/v1/administration/audit-logs/{response.Id}", response);
        });

        group.MapGet("/audit-logs/entity/{entityId:guid}", async (
            Guid entityId,
            int? skip,
            int? take,
            GetEntityAuditLogHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(entityId, skip ?? 0, take ?? 50, ct);
            return Results.Ok(response);
        });

        // Legacy Configuration Endpoints (Retained for API backwards compatibility)
        group.MapGet("/configurations", async (
            int? skip,
            int? take,
            ListConfigurationsHandler handler,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(tenantId, skip ?? 0, take ?? 50, ct);
            return Results.Ok(response);
        });

        group.MapGet("/configurations/{id:guid}", async (
            Guid id,
            GetConfigurationHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, ct);
            return Results.Ok(response);
        });

        group.MapGet("/configurations/key/{key}", async (
            string key,
            ITenantContext tenantContext,
            GetConfigurationByKeyHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(tenantId, key, ct);
            return Results.Ok(response);
        });

        group.MapPost("/configurations", async (
            [FromBody] CreateConfigurationRequest request,
            ITenantContext tenantContext,
            CreateConfigurationHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, ct);
            return Results.Created($"/api/v1/administration/configurations/{response.Id}", response);
        });

        group.MapPut("/configurations/{id:guid}", async (
            Guid id,
            [FromBody] UpdateConfigurationRequest request,
            UpdateConfigurationHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(id, request, ct);
            return Results.Ok(response);
        });

        return app;
    }
}