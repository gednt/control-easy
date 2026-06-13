using ControlEasyReborn.Modules.Reports.Application.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Reports.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports")
            .RequireAuthorization()
            .WithTags("Reports");

        group.MapGet("/visit-counts-by-day", async (
            DateOnly? from,
            DateOnly? to,
            GetVisitCountsByDayHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(from, to, ct);
            return Results.Ok(response);
        });

        group.MapGet("/residents-per-apartment", async (
            GetResidentsPerApartmentHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(ct);
            return Results.Ok(response);
        });

        return app;
    }
}
