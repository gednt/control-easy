using ControlEasyReborn.Modules.Visits.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Visits.Api.DI;

public static class VisitEndpointsExtensions
{
    public static IEndpointRouteBuilder MapVisitsApi(this IEndpointRouteBuilder app)
    {
        app.MapVisitEndpoints();
        return app;
    }
}