using ControlEasyReborn.Modules.Vehicles.Api.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.Vehicles.Api.DI;

public static class VehicleEndpointsExtensions
{
    public static IEndpointRouteBuilder MapVehiclesApi(this IEndpointRouteBuilder app)
    {
        app.MapVehicleEndpoints();
        return app;
    }
}