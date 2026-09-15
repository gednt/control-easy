using ControlEasyReborn.Infrastructure.Bootstrap;
using ControlEasyReborn.SharedKernel.Bootstrap;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ControlEasyReborn.UnitTests.Bootstrap;

public sealed class BootstrapRegistrationOptionsTests
{
    [Fact]
    public void ShouldRunPlatformAdminBootstrap_defaults_to_true_when_unset()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        BootstrapHostingExtensions.ShouldRunPlatformAdminBootstrap(configuration).Should().BeTrue();
    }

    [Fact]
    public void ShouldRunPlatformAdminBootstrap_returns_true_when_explicitly_enabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{BootstrapOptions.SectionName}:Enabled"] = "true"
            })
            .Build();

        BootstrapHostingExtensions.ShouldRunPlatformAdminBootstrap(configuration).Should().BeTrue();
    }

    [Fact]
    public void ShouldRunPlatformAdminBootstrap_returns_false_when_disabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{BootstrapOptions.SectionName}:Enabled"] = "false"
            })
            .Build();

        BootstrapHostingExtensions.ShouldRunPlatformAdminBootstrap(configuration).Should().BeFalse();
    }
}