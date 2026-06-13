using ControlEasyReborn.Api.Hosting;
using ControlEasyReborn.Infrastructure.Demo;
using ControlEasyReborn.Modules.Security.Infrastructure.DI;
using ControlEasyReborn.SharedKernel.Demo;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ControlEasyReborn.UnitTests.Demo;

public sealed class DemoBootstrapRegistrationTests
{
    [Fact]
    public void When_demo_disabled_registers_PlatformAdminBootstrapService()
    {
        var services = BuildServices(demoEnabled: false);
        var hosted = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        hosted.Should().Contain(d => d.ImplementationType == typeof(PlatformAdminBootstrapService));
        hosted.Should().NotContain(d => d.ImplementationType == typeof(DemoSeederService));
    }

    [Fact]
    public void When_demo_enabled_skips_PlatformAdminBootstrapService()
    {
        var services = BuildServices(demoEnabled: true);
        var hosted = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        hosted.Should().NotContain(d => d.ImplementationType == typeof(PlatformAdminBootstrapService));
        hosted.Any(d => d.ImplementationType == typeof(DemoSeederService) || d.ImplementationFactory is not null)
            .Should().BeTrue();
    }

    private static IServiceCollection BuildServices(bool demoEnabled)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{DemoOptions.SectionName}:Enabled"] = demoEnabled.ToString().ToLowerInvariant(),
                [$"{DemoOptions.SectionName}:SeedVersion"] = "1",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSecurityModule();
        services.AddControlEasyDemo(configuration);

        if (DemoHostingExtensions.ShouldRegisterPlatformAdminBootstrap(configuration))
        {
            services.AddHostedService<PlatformAdminBootstrapService>();
        }

        return services;
    }
}
