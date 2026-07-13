using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using DBTools.Configuration;
using DBTools.Core;
using DBTools.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ControlEasyReborn.Infrastructure.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControlEasyDbTools(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<DbToolsOptions>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new DbToolsOptions
            {
                Host = config["Db:Host"] ?? throw new InvalidOperationException("Db:Host is not configured."),
                Database = config["Db:Database"] ?? throw new InvalidOperationException("Db:Database is not configured."),
                Username = config["Db:Username"] ?? throw new InvalidOperationException("Db:Username is not configured."),
                Password = config["Db:Password"] ?? throw new InvalidOperationException("Db:Password is not configured."),
                Port = config["Db:Port"] ?? "3306",
                Provider = ResolveProvider(config["Db:Provider"] ?? "MySQL")
            };
        });

        services.TryAddSingleton<ISqlValidator, SqlValidator>();
        services.TryAddSingleton<ISqlQueryBuilder>(sp =>
            new SqlQueryBuilder(sp.GetRequiredService<ISqlValidator>()));
        services.TryAddSingleton<IDbProvider>(sp =>
            DbProviderFactory.Create(sp.GetRequiredService<DbToolsOptions>().Provider));
        services.TryAddSingleton<IDbConfiguration>(sp =>
            new ControlEasyDbConfiguration(sp.GetRequiredService<DbToolsOptions>()));
        services.TryAddScoped<IAsyncSqlClient>(sp =>
        {
            var config = sp.GetRequiredService<IDbConfiguration>();
            var validator = sp.GetRequiredService<ISqlValidator>();
            var queryBuilder = sp.GetRequiredService<ISqlQueryBuilder>();
            var provider = sp.GetRequiredService<IDbProvider>();
            return new AsyncSqlClient(config, validator, queryBuilder, provider);
        });

        services.TryAddSingleton<TenantAwareLinqFactory>();
        services.TryAddSingleton<ITenantAwareLinqFactory>(sp => sp.GetRequiredService<TenantAwareLinqFactory>());
        services.TryAddScoped<ITenantContext, HttpTenantContext>();

        return services;
    }

    private static DatabaseProvider ResolveProvider(string providerName)
    {
        return providerName.ToUpperInvariant() switch
        {
            "MYSQL" => DatabaseProvider.MySQL,
            "SQLSERVER" or "MSSQL" => DatabaseProvider.SqlServer,
            "POSTGRESQL" or "POSTGRES" => DatabaseProvider.PostgreSQL,
            "SQLITE" => DatabaseProvider.SQLite,
            _ => throw new InvalidOperationException($"Unsupported database provider: {providerName}")
        };
    }
}

internal sealed class ControlEasyDbConfiguration : IDbConfiguration
{
    public string Host { get; }
    public string Database { get; }
    public string Uid { get; }
    public string Password { get; }
    public string Port { get; }
    public string ConnectionString { get; }
    public string Provider { get; }

    public ControlEasyDbConfiguration(DbToolsOptions options)
    {
        Host = options.Host ?? string.Empty;
        Database = options.Database ?? string.Empty;
        Uid = options.Username ?? string.Empty;
        Password = options.Password ?? string.Empty;
        Port = options.Port ?? "1433";
        Provider = options.Provider.ToString();
        ConnectionString = BuildConnectionString(options);
    }

    private static string BuildConnectionString(DbToolsOptions options)
    {
        if (!string.IsNullOrEmpty(options.ConnectionString))
            return options.ConnectionString;

        return options.Provider switch
        {
            DatabaseProvider.SqlServer => $"Data Source=tcp:{options.Host},{options.Port};Initial Catalog={options.Database};User ID={options.Username};Password={options.Password};TrustServerCertificate={options.TrustServerCertificate};",
            DatabaseProvider.PostgreSQL => $"Host={options.Host};Port={options.Port};Database={options.Database};Username={options.Username};Password={options.Password};",
            DatabaseProvider.MySQL => $"Server={options.Host};Port={options.Port};Database={options.Database};Uid={options.Username};Pwd={options.Password};",
            DatabaseProvider.SQLite => $"Data Source={options.Database};",
            _ => throw new InvalidOperationException($"Unknown database provider: {options.Provider}")
        };
    }
}