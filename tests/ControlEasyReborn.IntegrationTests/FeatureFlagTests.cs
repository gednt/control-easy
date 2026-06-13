using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class FeatureFlagTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public FeatureFlagTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task GetFeatures_returns_ok_with_feature_flags()
    {
        var client = _factory.AsTenantA();

        var response = await client.GetAsync("/api/v1/features");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        body.Should().NotBeNull();
        body.Should().ContainKey("Residents.UseWeb");
        body.Should().ContainKey("Visits.UseWeb");
        body.Should().ContainKey("Vehicles.UseWeb");
        body.Should().ContainKey("ServiceProviders.UseWeb");
        body.Should().ContainKey("Administration.UseWeb");
    }

    [Fact]
    public async Task ResidentsUseWeb_defaults_to_false()
    {
        var client = _factory.AsTenantA();

        var response = await client.GetAsync("/api/v1/features");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        body.Should().NotBeNull();
        body!["Residents.UseWeb"].Should().BeFalse();
    }
}