using ControlEasyReborn.Modules.Security.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Security.Api.DI;

public static class SecurityEndpointsExtensions
{
    public static IEndpointRouteBuilder MapSecurityApi(this IEndpointRouteBuilder app)
    {
        app.MapSecurityEndpoints();
        return app;
    }
}