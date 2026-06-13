using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class VisitEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public VisitEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateVisit_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var request = new CreateVisitRequest("Ana Costa", "12345678901", null, null, "Delivery");

        var response = await client.PostAsJsonAsync("/api/v1/visits", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<VisitResponse>();
        body.Should().NotBeNull();
        body!.VisitorName.Should().Be("Ana Costa");
    }

    [Fact]
    public async Task CrossTenant_GetVisit_AsDifferentTenant_Returns404()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/visits",
            new CreateVisitRequest("Cross Tenant Visit", "98765432100", null, null, null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<VisitResponse>();
        created.Should().NotBeNull();

        var getResponse = await clientB.GetAsync($"/api/v1/visits/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
