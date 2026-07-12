using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class ServiceProviderEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public ServiceProviderEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateServiceProvider_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var request = new CreateServiceProviderRequest("Clean Co", "11222333000181", "11999990000", null, "Cleaning", null);

        var response = await client.PostAsJsonAsync("/api/v1/service-providers", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ServiceProviderResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Clean Co");
    }

    [Fact]
    public async Task CrossTenant_GetServiceProvider_AsDifferentTenant_Returns404()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/service-providers",
            new CreateServiceProviderRequest("Tenant A Provider", "99888777000166", null, null, null, null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ServiceProviderResponse>();
        created.Should().NotBeNull();

        var getResponse = await clientB.GetAsync($"/api/v1/service-providers/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
