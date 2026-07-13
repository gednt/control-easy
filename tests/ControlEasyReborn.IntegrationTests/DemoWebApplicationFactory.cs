using ControlEasyReborn.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

public sealed class DemoWebApplicationFactory : TenantAwareWebApplicationFactory
{
    private readonly MySqlContainerFixture _mySql;

    public DemoWebApplicationFactory(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Db:Host"] = _mySql.Host,
                ["Db:Port"] = new MySqlConnector.MySqlConnectionStringBuilder(_mySql.ConnectionString).Port.ToString(),
                ["Db:Database"] = "controleasydb",
                ["Db:Username"] = "root",
                ["Db:Password"] = "testpw",
                ["Jwt:SigningKey"] = JwtTestHelper.Secret,
                ["Jwt:Issuer"] = JwtTestHelper.Issuer,
                ["Jwt:Audience"] = JwtTestHelper.Audience,
                ["Demo:Enabled"] = "true",
                ["Demo:SeedVersion"] = "2",
                ["Demo:DisableOutboundEmail"] = "true",
            });
        });
    }
}
