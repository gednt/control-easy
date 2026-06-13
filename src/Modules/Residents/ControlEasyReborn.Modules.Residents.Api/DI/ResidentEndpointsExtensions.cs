using ControlEasyReborn.Modules.Residents.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Residents.Api.DI;

public static class ResidentEndpointsExtensions
{
    public static IEndpointRouteBuilder MapResidentsApi(this IEndpointRouteBuilder app)
    {
        app.MapResidentEndpoints();
        return app;
    }
}