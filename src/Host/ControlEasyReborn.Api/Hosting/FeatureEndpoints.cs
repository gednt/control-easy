using ControlEasyReborn.SharedKernel.FeatureFlags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;

namespace ControlEasyReborn.Api.Hosting;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/features")
            .RequireAuthorization()
            .WithTags("Features");

        group.MapGet("/", async (IFeatureManagerSnapshot featureManager, CancellationToken ct) =>
        {
            var flags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                [FeatureFlags.ResidentsUseWeb] = await featureManager.IsEnabledAsync(FeatureFlags.ResidentsUseWeb),
            };

            return Results.Ok(flags);
        });

        return app;
    }
}