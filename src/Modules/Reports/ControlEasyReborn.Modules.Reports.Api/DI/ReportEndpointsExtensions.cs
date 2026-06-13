using ControlEasyReborn.Modules.Reports.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Reports.Api.DI;

public static class ReportEndpointsExtensions
{
    public static IEndpointRouteBuilder MapReportsApi(this IEndpointRouteBuilder app)
    {
        app.MapReportEndpoints();
        return app;
    }
}
