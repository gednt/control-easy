using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class ResidentEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public ResidentEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateResident_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var request = new CreateResidentRequest(
            Name: "Maria Silva",
            Cpf: "52998224725",
            Email: "maria@tenanta.test",
            Phone: "11999990001",
            ApartmentId: null);

        var response = await client.PostAsJsonAsync("/api/v1/residents", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ResidentResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Maria Silva");
        body.Cpf.Should().Be("52998224725");
    }

    [Fact]
    public async Task GetResidents_AsTenantA_ReturnsOk()
    {
        var client = _factory.AsTenantA();

        var response = await client.GetAsync("/api/v1/residents");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ResidentResponse>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task CrossTenant_GetResident_AsDifferentTenant_Returns404()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var request = new CreateResidentRequest(
            Name: "Joao Santos",
            Cpf: "94515224008",
            Email: "joao@tenanta.test",
            Phone: null,
            ApartmentId: null);

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/residents", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ResidentResponse>();
        created.Should().NotBeNull();

        var getResponse = await clientB.GetAsync($"/api/v1/residents/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrossTenant_ListResidents_AsDifferentTenant_ReturnsEmptyList()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var request = new CreateResidentRequest(
            Name: "Ana Costa",
            Cpf: "32579923008",
            Email: "ana@tenanta.test",
            Phone: null,
            ApartmentId: null);

        await clientA.PostAsJsonAsync("/api/v1/residents", request);

        var listResponse = await clientB.GetAsync("/api/v1/residents");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResponse.Content.ReadFromJsonAsync<List<ResidentResponse>>();
        body.Should().NotBeNull();
        body!.Should().BeEmpty();
    }
}