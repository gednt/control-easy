using ControlEasyReborn.SharedKernel.Demo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ControlEasyReborn.Infrastructure.Demo;

public static class DemoServiceCollectionExtensions
{
    public static IServiceCollection AddControlEasyDemo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DemoOptions>(configuration.GetSection(DemoOptions.SectionName));
        services.AddSingleton<DemoSeederService>();
        services.AddHostedService(sp => sp.GetRequiredService<DemoSeederService>());
        return services;
    }

    public static bool IsDemoEnabled(this IServiceProvider services) =>
        services.GetRequiredService<IOptions<DemoOptions>>().Value.Enabled;
}

public static class DemoEndpoints
{
    public sealed record DemoInfoResponse(bool Enabled, int? SeedVersion = null, IReadOnlyList<DemoTenantInfo>? Tenants = null);
    public sealed record DemoTenantInfo(string Slug, string DisplayName);

    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/demo")
            .WithTags("Demo");

        group.MapGet("/info", (IOptions<DemoOptions> options) =>
        {
            if (!options.Value.Enabled)
                return Results.Ok(new DemoInfoResponse(false));

            return Results.Ok(new DemoInfoResponse(
                Enabled: true,
                SeedVersion: options.Value.SeedVersion,
                Tenants:
                [
                    new DemoTenantInfo("demo-aurora", "[Demo] Residencial Aurora"),
                    new DemoTenantInfo("demo-parque-verde", "[Demo] Condomínio Parque Verde")
                ]));
        }).AllowAnonymous();

        group.MapPost("/reset", async (
            DemoSeederService seeder,
            IOptions<DemoOptions> options,
            CancellationToken ct) =>
        {
            if (!options.Value.Enabled)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Demo mode is not enabled.",
                    Type = "https://httpstatuses.io/404"
                });
            }

            await seeder.SeedAsync(force: true, ct);
            return Results.NoContent();
        })
        .RequireAuthorization("PlatformAdminOnly");

        return app;
    }
}
