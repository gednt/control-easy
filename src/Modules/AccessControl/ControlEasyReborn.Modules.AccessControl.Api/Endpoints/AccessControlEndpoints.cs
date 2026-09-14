using Microsoft.AspNetCore.Routing;

namespace ControlEasyReborn.Modules.AccessControl.Api.Endpoints;

public static class AccessControlEndpoints
{
    public static IEndpointRouteBuilder MapAccessControlApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/access-control")
            .RequireAuthorization()
            .WithTags("AccessControl");

        group.MapGet("/_meta", () => Results.Ok(new
        {
            module = "AccessControl",
            status = "scaffold"
        }));

        return app;
    }
}
