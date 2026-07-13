using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Infrastructure.Demo;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class ResidentEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;
    private readonly MySqlContainerFixture _mySql;

    public ResidentEndpointTests(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
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
            Cpf: "76576734352",
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

    [Fact]
    public async Task ListResidents_FilteredByApartmentId_ReturnsOnlyMatchingResidents()
    {
        var demoFactory = new DemoWebApplicationFactory(_mySql);
        await WaitForDemoSeedAsync(demoFactory);
        var token = await LoginAsync(demoFactory, "porteiro@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(demoFactory, token);

        var residents = await GetResidentsAsync(client);
        var florinda = residents.Single(r => r.Name == "Dona Florinda");
        var kratos = residents.Single(r => r.Name == "Kratos");
        florinda.ApartmentId.Should().NotBeNull();

        var listResponse = await client.GetAsync($"/api/v1/residents?apartmentId={florinda.ApartmentId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var filtered = await listResponse.Content.ReadFromJsonAsync<List<ResidentResponse>>();
        filtered.Should().NotBeNull();
        filtered!.Should().Contain(r => r.Name == "Dona Florinda");
        filtered.Should().Contain(r => r.Name == "Quico");
        filtered.Should().NotContain(r => r.Name == "Kratos");
        filtered.Should().OnlyContain(r => r.ApartmentId == florinda.ApartmentId);
    }

    [Fact]
    public async Task UpdateResident_Deactivate_ReturnsInactive()
    {
        var demoFactory = new DemoWebApplicationFactory(_mySql);
        await WaitForDemoSeedAsync(demoFactory);
        var token = await LoginAsync(demoFactory, "admin@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(demoFactory, token);

        var residents = await GetResidentsAsync(client);
        var target = residents.Single(r => r.Name == "Hades");
        target.Active.Should().BeTrue();

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/residents/{target.Id}",
            new UpdateResidentRequest(
                target.Name,
                target.Cpf,
                target.Email,
                target.Phone,
                target.ApartmentId,
                Active: false));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ResidentResponse>();
        updated.Should().NotBeNull();
        updated!.Active.Should().BeFalse();

        var getResponse = await client.GetAsync($"/api/v1/residents/{target.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ResidentResponse>();
        fetched!.Active.Should().BeFalse();
    }

    [Fact]
    public async Task CrossTenant_ListResidents_FilteredByApartmentId_ReturnsEmptyList()
    {
        var demoFactory = new DemoWebApplicationFactory(_mySql);
        await WaitForDemoSeedAsync(demoFactory);
        var auroraToken = await LoginAsync(demoFactory, "porteiro@controleasy.app", "demo123");
        var auroraClient = CreateAuthorizedClient(demoFactory, auroraToken);
        var auroraResidents = await GetResidentsAsync(auroraClient);
        var kratos = auroraResidents.Single(r => r.Name == "Kratos");
        kratos.ApartmentId.Should().NotBeNull();

        var multiToken = await LoginAsync(demoFactory, "multi@controleasy.app", "demo123");
        var switchClient = CreateAuthorizedClient(demoFactory, multiToken);
        var parqueVerdeId = Guid.Parse("00000000-0000-0000-0000-000000000020");
        var switchResponse = await switchClient.PostAsJsonAsync(
            "/api/v1/security/tenant-switch",
            new TenantSwitchRequest(parqueVerdeId));
        switchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var switched = await switchResponse.Content.ReadFromJsonAsync<TenantSwitchResponse>();

        var parqueClient = CreateAuthorizedClient(demoFactory, switched!.Token);
        var listResponse = await parqueClient.GetAsync($"/api/v1/residents?apartmentId={kratos.ApartmentId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResponse.Content.ReadFromJsonAsync<List<ResidentResponse>>();
        body.Should().NotBeNull();
        body!.Should().BeEmpty();
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
        using var scope = factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoSeederService>();
        await seeder.SeedAsync(force: true, CancellationToken.None);
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

    private static async Task<List<ResidentResponse>> GetResidentsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/residents");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ResidentResponse>>();
        return body ?? [];
    }
}