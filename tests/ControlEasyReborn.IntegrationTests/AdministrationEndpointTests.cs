using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class AdministrationEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public AdministrationEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateConfiguration_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var request = new CreateConfigurationRequest("gatehouse.hours", "06:00-22:00", "Gatehouse operating hours");

        var response = await client.PostAsJsonAsync("/api/v1/administration/configurations", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ConfigurationResponse>();
        body.Should().NotBeNull();
        body!.Key.Should().Be("gatehouse.hours");
    }

    [Fact]
    public async Task CrossTenant_GetConfiguration_AsDifferentTenant_Returns404()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/administration/configurations",
            new CreateConfigurationRequest("tenant.a.only", "secret", null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ConfigurationResponse>();
        created.Should().NotBeNull();

        var getResponse = await clientB.GetAsync($"/api/v1/administration/configurations/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
