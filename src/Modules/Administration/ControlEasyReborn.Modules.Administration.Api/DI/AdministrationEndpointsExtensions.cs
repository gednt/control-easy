using ControlEasyReborn.Modules.Administration.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Administration.Api.DI;

public static class AdministrationEndpointsExtensions
{
    public static IEndpointRouteBuilder MapAdministrationApi(this IEndpointRouteBuilder app)
    {
        app.MapAdministrationEndpoints();
        return app;
    }
}