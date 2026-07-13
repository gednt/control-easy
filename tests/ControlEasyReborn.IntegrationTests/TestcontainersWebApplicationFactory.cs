using ControlEasyReborn.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[Collection("MySql Collection")]
public sealed class TestcontainersWebApplicationFactory : TenantAwareWebApplicationFactory
{
    private readonly MySqlContainerFixture _mySql;

    public TestcontainersWebApplicationFactory(MySqlContainerFixture mySql)
    {
        _mySql = mySql;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((ctx, config) =>
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
            });
        });

        builder.ConfigureServices(services =>
        {
        });
    }
}