using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Infrastructure.Demo;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class ApartmentEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;
    private readonly MySqlContainerFixture _mySql;

    public ApartmentEndpointTests(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task CreateApartment_AsTenantA_ReturnsCreated()
    {
        var client = _factory.AsTenantA();
        var request = new CreateApartmentRequest("A", "101");

        var response = await client.PostAsJsonAsync("/api/v1/apartments", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ApartmentResponse>();
        body.Should().NotBeNull();
        body!.Block.Should().Be("A");
        body.Unit.Should().Be("101");
        body.Active.Should().BeTrue();
    }

    [Fact]
    public async Task GetApartment_AsTenantA_ReturnsOk()
    {
        var client = _factory.AsTenantA();
        var createResponse = await client.PostAsJsonAsync("/api/v1/apartments", new CreateApartmentRequest("B", "202"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApartmentResponse>();

        var getResponse = await client.GetAsync($"/api/v1/apartments/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResponse.Content.ReadFromJsonAsync<ApartmentResponse>();
        body!.Block.Should().Be("B");
        body.Unit.Should().Be("202");
    }

    [Fact]
    public async Task UpdateApartment_Deactivate_ReturnsOk()
    {
        var client = _factory.AsTenantA();
        var createResponse = await client.PostAsJsonAsync("/api/v1/apartments", new CreateApartmentRequest("C", "303"));
        var created = await createResponse.Content.ReadFromJsonAsync<ApartmentResponse>();

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/apartments/{created!.Id}",
            new UpdateApartmentRequest("C", "303", Active: false));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await updateResponse.Content.ReadFromJsonAsync<ApartmentResponse>();
        body!.Active.Should().BeFalse();
    }

    [Fact]
    public async Task CreateApartment_DuplicateBlockUnit_Returns409()
    {
        var client = _factory.AsTenantA();
        await client.PostAsJsonAsync("/api/v1/apartments", new CreateApartmentRequest("D", "404"));

        var response = await client.PostAsJsonAsync("/api/v1/apartments", new CreateApartmentRequest("D", "404"));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CrossTenant_GetApartment_AsDifferentTenant_Returns404()
    {
        var clientA = _factory.AsTenantA();
        var clientB = _factory.AsTenantB();

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/apartments", new CreateApartmentRequest("E", "505"));
        var created = await createResponse.Content.ReadFromJsonAsync<ApartmentResponse>();

        var getResponse = await clientB.GetAsync($"/api/v1/apartments/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DemoSeed_apartment_ids_match_DemoIds_stable_guids()
    {
        var demoFactory = new DemoWebApplicationFactory(_mySql);
        await WaitForDemoSeedAsync(demoFactory);

        var token = await LoginAsync(demoFactory, "porteiro@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(demoFactory, token);

        var expectedId = DemoIds.ApartmentId(DemoIds.AuroraTenantId, "A", "14");
        var response = await client.GetAsync($"/api/v1/apartments/{expectedId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApartmentResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(expectedId);
        body.Block.Should().Be("A");
        body.Unit.Should().Be("14");
    }

    private static HttpClient CreateAuthorizedClient(WebApplicationFactory<Program> factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task WaitForDemoSeedAsync(WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        for (var i = 0; i < 60; i++)
        {
            var info = await client.GetFromJsonAsync<DemoEndpoints.DemoInfoResponse>("/api/v1/demo/info");
            if (info?.Enabled == true)
            {
                var token = await TryLoginAsync(client, "porteiro@controleasy.app", "demo123");
                if (token is not null)
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var expectedId = DemoIds.ApartmentId(DemoIds.AuroraTenantId, "A", "14");
                    var aptResponse = await client.GetAsync($"/api/v1/apartments/{expectedId}");
                    if (aptResponse.IsSuccessStatusCode)
                        return;
                }
            }
            await Task.Delay(500);
        }
        throw new TimeoutException("Demo apartment seed did not complete within the expected time.");
    }

    private static async Task<string?> TryLoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/security/auth/login", new LoginRequest(email, password));
        if (!response.IsSuccessStatusCode)
            return null;
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body?.Token;
    }

    private static async Task<string> LoginAsync(WebApplicationFactory<Program> factory, string email, string password)
    {
        var client = factory.CreateClient();
        var token = await TryLoginAsync(client, email, password);
        token.Should().NotBeNull($"login failed for {email}");
        return token!;
    }
}
