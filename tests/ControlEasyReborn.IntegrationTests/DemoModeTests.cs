using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Infrastructure.Demo;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class DemoModeTests
{
    private readonly MySqlContainerFixture _mySql;
    private readonly DemoWebApplicationFactory _demoFactory;
    private readonly TestcontainersWebApplicationFactory _normalFactory;

    public DemoModeTests(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
        _demoFactory = new DemoWebApplicationFactory(mySql);
        _normalFactory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task DemoInfo_when_enabled_returns_enabled_true()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var client = _demoFactory.CreateClient();

        var response = await client.GetAsync("/api/v1/demo/info");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<DemoEndpoints.DemoInfoResponse>();
        body.Should().NotBeNull();
        body!.Enabled.Should().BeTrue();
        body.SeedVersion.Should().Be(1);
        body.Tenants.Should().HaveCount(2);
    }

    [Fact]
    public async Task DemoInfo_when_disabled_returns_enabled_false()
    {
        var client = _normalFactory.CreateClient();

        var response = await client.GetAsync("/api/v1/demo/info");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<DemoEndpoints.DemoInfoResponse>();
        body!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task DemoReset_when_disabled_returns_404()
    {
        var client = _normalFactory.AsPlatformAdmin();

        var response = await client.PostAsync("/api/v1/demo/reset", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DemoReset_when_enabled_restores_resident_count()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var token = await LoginAsync(_demoFactory, "porteiro@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(_demoFactory, token);

        var before = await GetResidentsAsync(client);
        before.Should().HaveCountGreaterThanOrEqualTo(61);

        var adminClient = _demoFactory.AsPlatformAdmin();
        var resetResponse = await adminClient.PostAsync("/api/v1/demo/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await WaitForDemoSeedAsync(_demoFactory);
        var after = await GetResidentsAsync(client);
        after.Should().HaveCountGreaterThanOrEqualTo(61);
        after.Should().Contain(r => r.Name == "Kratos");
    }

    [Fact]
    public async Task DemoSeed_includes_thematic_residents_and_cohabitation()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var token = await LoginAsync(_demoFactory, "porteiro@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(_demoFactory, token);

        var residents = await GetResidentsAsync(client);
        residents.Should().HaveCountGreaterThanOrEqualTo(61);
        residents.Should().Contain(r => r.Name == "Kratos");
        residents.Should().Contain(r => r.Name == "Atreus");
        residents.Should().Contain(r => r.Name == "Chaves");
        residents.Should().Contain(r => r.Name == "Zeus");

        var florinda = residents.Single(r => r.Name == "Dona Florinda");
        var quico = residents.Single(r => r.Name == "Quico");
        var girafales = residents.Single(r => r.Name == "Prof. Girafales");
        florinda.ApartmentId.Should().NotBeNull();
        florinda.ApartmentId.Should().Be(quico.ApartmentId);
        florinda.ApartmentId.Should().Be(girafales.ApartmentId);

        var kratos = residents.Single(r => r.Name == "Kratos");
        var atreus = residents.Single(r => r.Name == "Atreus");
        kratos.ApartmentId.Should().Be(atreus.ApartmentId);
    }

    [Fact]
    public async Task DemoSeed_second_startup_does_not_duplicate_residents()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var token = await LoginAsync(_demoFactory, "porteiro@controleasy.app", "demo123");
        var firstCount = (await GetResidentsAsync(CreateAuthorizedClient(_demoFactory, token))).Count;

        await using var secondFactory = new DemoWebApplicationFactory(_mySql);
        await WaitForDemoSeedAsync(secondFactory);
        var secondToken = await LoginAsync(secondFactory, "porteiro@controleasy.app", "demo123");
        var secondCount = (await GetResidentsAsync(CreateAuthorizedClient(secondFactory, secondToken))).Count;

        secondCount.Should().Be(firstCount);
    }

    [Fact]
    public async Task MultiTenant_login_lookup_returns_both_demo_tenants()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var client = _demoFactory.CreateClient();

        var response = await client.GetAsync("/api/v1/security/tenants?email=multi@controleasy.app");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tenants = await response.Content.ReadFromJsonAsync<List<TenantLookupResponse>>();
        tenants.Should().NotBeNull();
        tenants!.Should().HaveCount(2);
        tenants.Select(t => t.DisplayName).Should().Contain("[Demo] Residencial Aurora");
        tenants.Select(t => t.DisplayName).Should().Contain("[Demo] Condomínio Parque Verde");
    }

    [Fact]
    public async Task CrossTenant_demo_resident_not_visible_in_other_tenant()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var multiToken = await LoginAsync(_demoFactory, "multi@controleasy.app", "demo123");

        var switchClient = CreateAuthorizedClient(_demoFactory, multiToken);
        var parqueVerdeId = Guid.Parse("00000000-0000-0000-0000-000000000020");
        var switchResponse = await switchClient.PostAsJsonAsync(
            "/api/v1/security/tenant-switch",
            new TenantSwitchRequest(parqueVerdeId));
        switchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var switched = await switchResponse.Content.ReadFromJsonAsync<TenantSwitchResponse>();

        var parqueClient = CreateAuthorizedClient(_demoFactory, switched!.Token);
        var parqueResidents = await GetResidentsAsync(parqueClient);
        parqueResidents.Should().NotContain(r => r.Name == "Kratos");
        parqueResidents.Should().Contain(r => r.Name == "Rogelio");
    }

    [Fact]
    public async Task DemoOff_regression_demo_users_do_not_exist()
    {
        var client = _normalFactory.CreateClient();
        var request = new LoginRequest("porteiro@controleasy.app", "demo123");

        var response = await client.PostAsJsonAsync("/api/v1/security/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DemoSeed_visit_timestamps_are_within_last_24_hours()
    {
        await WaitForDemoSeedAsync(_demoFactory);
        var token = await LoginAsync(_demoFactory, "porteiro@controleasy.app", "demo123");
        var client = CreateAuthorizedClient(_demoFactory, token);

        var response = await client.GetAsync("/api/v1/visits");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var visits = await response.Content.ReadFromJsonAsync<List<VisitResponse>>();
        visits.Should().NotBeNull();
        visits!.Should().NotBeEmpty();

        var cutoff = DateTime.UtcNow.AddHours(-24);
        visits.Should().OnlyContain(v => v.CreatedAtUtc >= cutoff);
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
                    var residents = await client.GetFromJsonAsync<List<ResidentResponse>>("/api/v1/residents");
                    if (residents is { Count: >= 61 })
                        return;
                }
            }
            await Task.Delay(500);
        }
        throw new TimeoutException("Demo seed did not complete within the expected time.");
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
