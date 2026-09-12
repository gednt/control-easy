using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Photos.Api.Endpoints;

public static class EntryLogEndpoints
{
    public static IEndpointRouteBuilder MapEntryLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/entry-log")
            .RequireAuthorization()
            .WithTags("Entry Log");

        group.MapPost("/", async (
            [FromBody] ControlEasyReborn.Modules.Photos.Application.Contracts.CreateEntryLogRequest request,
            ITenantContext tenantContext,
            CreateEntryLogHandler handler,
            CancellationToken ct) =>
        {
            var tenantId = tenantContext.TenantId
                ?? throw new InvalidOperationException("Tenant context is not resolved.");
            var response = await handler.HandleAsync(request, tenantId, tenantContext.ProfileId, ct);
            return Results.Created($"/api/v1/entry-log/{response.Id}", response);
        })
        .RequireAuthorization("Permission_Photos.Write");

        group.MapGet("/", async (
            string? entryState,
            string? subjectType,
            DateTime? fromUtc,
            DateTime? toUtc,
            int? skip,
            int? take,
            ListEntryLogsHandler handler,
            CancellationToken ct) =>
        {
            var entries = await handler.HandleAsync(entryState, subjectType, fromUtc, toUtc, skip ?? 0, take ?? 50, ct);
            return Results.Ok(entries);
        })
        .RequireAuthorization("Permission_Photos.Read");

        group.MapGet("/export", async (
            string? entryState,
            string? subjectType,
            DateTime? fromUtc,
            DateTime? toUtc,
            ExportEntryLogCsvHandler handler,
            CancellationToken ct) =>
        {
            var csv = await handler.HandleAsync(entryState, subjectType, fromUtc, toUtc, ct);
            return Results.Text(csv, "text/csv", System.Text.Encoding.UTF8);
        })
        .RequireAuthorization("Permission_Photos.Read");

        return app;
    }
}