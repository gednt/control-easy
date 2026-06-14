using System.Net;
using System.Net.Http.Json;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class TenantEndpointTests
{
    private readonly TestcontainersWebApplicationFactory _factory;

    public TenantEndpointTests(MySqlContainerFixture mySql)
    {
        _factory = new TestcontainersWebApplicationFactory(mySql);
    }

    [Fact]
    public async Task ListTenants_AsPlatformAdmin_ReturnsOk()
    {
        var client = _factory.AsPlatformAdmin();

        var response = await client.GetAsync("/api/v1/tenants");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TenantResponse[]>();
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTenant_AsPlatformAdmin_ReturnsCreated()
    {
        var client = _factory.AsPlatformAdmin();
        var slug = $"test-tenant-{Guid.NewGuid():N}"[..24];
        var request = new CreateTenantRequest(slug, "Integration Test Condominium");

        var response = await client.PostAsJsonAsync("/api/v1/tenants", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Slug.Should().Be(slug);
        body.DisplayName.Should().Be("Integration Test Condominium");
        body.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateTenant_AsTenantA_ReturnsForbidden()
    {
        var client = _factory.AsTenantA();
        var request = new CreateTenantRequest("forbidden-tenant", "Forbidden");

        var response = await client.PostAsJsonAsync("/api/v1/tenants", request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuspendTenant_AsPlatformAdmin_ReturnsOk()
    {
        var client = _factory.AsPlatformAdmin();
        var slug = $"suspend-{Guid.NewGuid():N}"[..20];
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/tenants",
            new CreateTenantRequest(slug, "Suspend Test"));
        var created = await createResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var suspendResponse = await client.PostAsJsonAsync(
            $"/api/v1/tenants/{created!.Id}/suspend",
            new SuspendTenantRequest(null));
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await suspendResponse.Content.ReadFromJsonAsync<TenantResponse>();
        body!.Status.Should().Be("Suspended");
    }

    [Fact]
    public async Task CreateTenantAdmin_then_login_succeeds()
    {
        var platformClient = _factory.AsPlatformAdmin();
        var slug = $"admin-login-{Guid.NewGuid():N}"[..22];
        var createTenantResponse = await platformClient.PostAsJsonAsync(
            "/api/v1/tenants",
            new CreateTenantRequest(slug, "Admin Login Test"));
        var tenant = await createTenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var adminEmail = $"admin-{Guid.NewGuid():N}@condo.test";
        const string adminPassword = "TempAdmin1!";
        var createAdminResponse = await platformClient.PostAsJsonAsync(
            $"/api/v1/tenants/{tenant!.Id}/admins",
            new CreateTenantAdminRequest(adminEmail, "Test Admin", adminPassword));
        createAdminResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginClient = _factory.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/v1/security/auth/login",
            new LoginRequest(adminEmail, adminPassword));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.TenantId.Should().Be(tenant.Id);
        loginBody.Roles.Should().Contain("TenantAdmin");
        loginBody.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTenantAdmin_AsPlatformAdmin_ReturnsOk()
    {
        var client = _factory.AsPlatformAdmin();
        var slug = $"upd-admin-{Guid.NewGuid():N}"[..20];
        var tenantResponse = await client.PostAsJsonAsync(
            "/api/v1/tenants",
            new CreateTenantRequest(slug, "Update Admin Test"));
        var tenant = await tenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var adminEmail = $"admin-{Guid.NewGuid():N}@condo.test";
        var createAdminResponse = await client.PostAsJsonAsync(
            $"/api/v1/tenants/{tenant!.Id}/admins",
            new CreateTenantAdminRequest(adminEmail, "Original Name", "TempAdmin1!"));
        var admin = await createAdminResponse.Content.ReadFromJsonAsync<TenantAdminResponse>();

        var newEmail = $"updated-{Guid.NewGuid():N}@condo.test";
        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/tenants/{tenant.Id}/admins/{admin!.UserId}",
            new UpdateTenantAdminRequest(newEmail, "Updated Name"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<TenantAdminResponse>();
        updated!.Email.Should().Be(newEmail);
        updated.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task CreatePorteiro_then_login_succeeds()
    {
        var platformClient = _factory.AsPlatformAdmin();
        var slug = $"porteiro-{Guid.NewGuid():N}"[..20];
        var tenantResponse = await platformClient.PostAsJsonAsync(
            "/api/v1/tenants",
            new CreateTenantRequest(slug, "Porteiro Test"));
        var tenant = await tenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var porteiroEmail = $"porteiro-{Guid.NewGuid():N}@condo.test";
        const string porteiroPassword = "Porteiro1!";
        var createResponse = await platformClient.PostAsJsonAsync(
            $"/api/v1/tenants/{tenant!.Id}/porteiros",
            new CreatePorteiroRequest(porteiroEmail, "Gate Keeper", porteiroPassword));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await platformClient.GetAsync($"/api/v1/tenants/{tenant.Id}/porteiros");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var porteiros = await listResponse.Content.ReadFromJsonAsync<PorteiroResponse[]>();
        porteiros.Should().ContainSingle(p => p.Email == porteiroEmail);

        var loginClient = _factory.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/v1/security/auth/login",
            new LoginRequest(porteiroEmail, porteiroPassword));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.TenantId.Should().Be(tenant.Id);
        loginBody.Roles.Should().Contain("AttendantProfile");
        loginBody.MustChangePassword.Should().BeTrue();
    }
}
