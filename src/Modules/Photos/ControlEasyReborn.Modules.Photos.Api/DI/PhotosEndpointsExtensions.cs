using ControlEasyReborn.Modules.Photos.Api.Endpoints;
using ControlEasyReborn.Modules.Photos.Infrastructure.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ControlEasyReborn.Modules.Photos.Api.DI;

public static class PhotosEndpointsExtensions
{
    public static IServiceCollection AddPhotosApi(this IServiceCollection services)
    {
        services.AddPhotosModule();
        return services;
    }

    public static IEndpointRouteBuilder MapPhotosApi(this IEndpointRouteBuilder app)
    {
        app.MapPhotosEndpoints();
        app.MapEntryLogEndpoints();
        app.MapConsentPolicyEndpoints();
        return app;
    }
}