using ControlEasyReborn.Modules.Apartments.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Apartments.Api.DI;

public static class ApartmentEndpointsExtensions
{
    public static IEndpointRouteBuilder MapApartmentsApi(this IEndpointRouteBuilder app)
    {
        app.MapApartmentEndpoints();
        return app;
    }
}
