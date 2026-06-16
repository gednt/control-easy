using ControlEasyReborn.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ControlEasyReborn.IntegrationTests;

public abstract class TenantAwareWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TenantAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid TenantBId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid PlatformAdminId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly string PlatformAdminEmail = "platform-admin-test@controleasy.local";

    private HttpClient? _tenantAClient;
    private HttpClient? _tenantBClient;
    private HttpClient? _platformAdminClient;

    private static readonly string[] TenantAdminPermissions =
    [
        "Visits.CheckIn", "Visits.CheckOut", "Visits.Read",
        "Apartments.Read", "Apartments.Write",
        "Residents.Read", "Residents.Write",
        "Vehicles.Read", "Vehicles.Write",
        "ServiceProviders.Read", "ServiceProviders.Write",
        "Reports.Read",
    ];

    public HttpClient AsTenantA()
    {
        if (_tenantAClient is not null)
            return _tenantAClient;

        _tenantAClient = CreateClient();
        var token = JwtTestHelper.GenerateTenantToken(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01"),
            "admin@tenanta.test",
            TenantAId,
            roles: new[] { "Admin" },
            permissions: TenantAdminPermissions);

        _tenantAClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return _tenantAClient;
    }

    public HttpClient AsTenantB()
    {
        if (_tenantBClient is not null)
            return _tenantBClient;

        _tenantBClient = CreateClient();
        var token = JwtTestHelper.GenerateTenantToken(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01"),
            "admin@tenantb.test",
            TenantBId,
            roles: new[] { "Admin" },
            permissions: TenantAdminPermissions);

        _tenantBClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return _tenantBClient;
    }

    public HttpClient AsPlatformAdmin()
    {
        if (_platformAdminClient is not null)
            return _platformAdminClient;

        _platformAdminClient = CreateClient();
        var token = JwtTestHelper.GeneratePlatformAdminToken(PlatformAdminId, PlatformAdminEmail);

        _platformAdminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return _platformAdminClient;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tenantAClient?.Dispose();
            _tenantBClient?.Dispose();
            _platformAdminClient?.Dispose();
        }

        base.Dispose(disposing);
    }
}