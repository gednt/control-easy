using System.Data;
using ControlEasyReborn.Api;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Xunit;

namespace ControlEasyReborn.IntegrationTests;

[CollectionDefinition("MySql Collection")]
public sealed class MySqlCollectionDefinition : ICollectionFixture<MySqlContainerFixture>;

[Collection("MySql Collection")]
public sealed class MySqlContainerFixture : IAsyncLifetime
{
    private readonly IContainer _container;
    private string _connectionString = string.Empty;

    public string ConnectionString => _connectionString;

    public MySqlContainerFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("mysql:8.0")
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "testpw")
            .WithEnvironment("MYSQL_DATABASE", "controleasydb")
            .WithPortBinding(3306, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(3306);
        _connectionString = $"Server=127.0.0.1;Port={port};Database=controleasydb;Uid=root;Pwd=testpw;";

        await WaitForMySqlReady();
        await RunInitScripts();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private async Task WaitForMySqlReady()
    {
        for (var i = 0; i < 30; i++)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                return;
            }
            catch
            {
                await Task.Delay(1000);
            }
        }
        throw new InvalidOperationException("MySQL container did not become ready in time.");
    }

    private async Task RunInitScripts()
    {
        var initDir = FindInitScriptsDirectory();
        if (!Directory.Exists(initDir))
            return;

        var scripts = Directory.GetFiles(initDir, "*.sql").OrderBy(f => f, StringComparer.Ordinal);

        foreach (var script in scripts)
        {
            var sql = await File.ReadAllTextAsync(script);
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var commands = sql.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var cmdText in commands)
            {
                if (string.IsNullOrWhiteSpace(cmdText)) continue;
                using var cmd = new MySqlCommand(cmdText, conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    private static string FindInitScriptsDirectory()
    {
        var candidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docker", "mysql", "init"));
        if (Directory.Exists(candidate))
            return candidate;

        candidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docker", "mysql", "init"));
        if (Directory.Exists(candidate))
            return candidate;

        candidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "docker", "mysql", "init"));
        return candidate;
    }
}