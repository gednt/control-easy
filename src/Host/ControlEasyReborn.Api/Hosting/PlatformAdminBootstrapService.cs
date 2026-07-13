using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.SharedKernel.Bootstrap;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace ControlEasyReborn.Api.Hosting;

public sealed class PlatformAdminBootstrapService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBootstrapCredentialsStore _credentialsStore;
    private readonly BootstrapOptions _bootstrapOptions;
    private readonly ILogger<PlatformAdminBootstrapService> _logger;

    public PlatformAdminBootstrapService(
        IServiceScopeFactory scopeFactory,
        IBootstrapCredentialsStore credentialsStore,
        IOptions<BootstrapOptions> bootstrapOptions,
        ILogger<PlatformAdminBootstrapService> logger)
    {
        _scopeFactory = scopeFactory;
        _credentialsStore = credentialsStore;
        _bootstrapOptions = bootstrapOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var demoOptions = scope.ServiceProvider.GetRequiredService<IOptions<ControlEasyReborn.SharedKernel.Demo.DemoOptions>>().Value;
        if (demoOptions.Enabled)
        {
            _logger.LogInformation("Demo mode is enabled; skipping platform admin bootstrap.");
            return;
        }

        var linqFactory = scope.ServiceProvider.GetRequiredService<ITenantAwareLinqFactory>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var bypassClient = linqFactory.Create(NullTenantContext.Instance, bypassTenantFilter: true);

        var existing = await bypassClient.SelectAsync(
            fields: "Id",
            table: "Users",
            whereClause: "Roles LIKE '%PlatformAdmin%'",
            parameters: Array.Empty<object>(),
            ct: ct);

        if (existing is not null && existing.Rows.Count > 0)
        {
            _logger.LogInformation("PlatformAdmin already exists; skipping bootstrap.");
            return;
        }

        var email = string.IsNullOrWhiteSpace(_bootstrapOptions.PlatformAdminEmail)
            ? $"platform-admin-{GenerateRandomString(8)}@controleasy.local"
            : _bootstrapOptions.PlatformAdminEmail.Trim();
        var password = string.IsNullOrWhiteSpace(_bootstrapOptions.PlatformAdminPassword)
            ? GenerateRandomString(16)
            : _bootstrapOptions.PlatformAdminPassword;
        var passwordHash = passwordHasher.Hash(password);
        var id = Guid.NewGuid();
        var platformTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var now = DateTime.UtcNow;

        await bypassClient.InsertAsync(
            new[] { "Id", "TenantId", "Email", "PasswordHash", "DisplayName", "Active", "MustChangePassword", "Roles", "CreatedAtUtc", "tenant_id" },
            "Users",
            new object[] { id, platformTenantId, email, passwordHash, "Platform Admin", true, true, "PlatformAdmin", now, platformTenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);

        await bypassClient.InsertAsync(
            new[] { "Id", "TenantId", "UserId", "DisplayName", "ShiftId", "GatehouseId", "Permissions", "Active", "CreatedAtUtc", "tenant_id" },
            "AttendantProfiles",
            new object?[]
            {
                Guid.NewGuid(), platformTenantId, id, "Platform Admin",
                DBNull.Value, DBNull.Value, "platform:*", true, now, platformTenantId
            },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);

        _credentialsStore.Set(email, password);
        _logger.LogWarning("// CHANGE IMMEDIATELY — PlatformAdmin seeded. Email: {Email}, Password: {Password}", email, password);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private static string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";
        var result = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);
        for (var i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}