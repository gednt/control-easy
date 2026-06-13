using ControlEasyReborn.Modules.ServiceProviders.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.ServiceProviders.Api.DI;

public static class ServiceProviderEndpointsExtensions
{
    public static IEndpointRouteBuilder MapServiceProvidersApi(this IEndpointRouteBuilder app)
    {
        app.MapServiceProviderEndpoints();
        return app;
    }
}