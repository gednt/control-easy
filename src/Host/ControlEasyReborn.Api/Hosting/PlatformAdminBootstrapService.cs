using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ControlEasyReborn.Api.Hosting;

public sealed class PlatformAdminBootstrapService : IHostedService
{
    private readonly IAsyncSqlClient _db;
    private readonly ITenantAwareLinqFactory _linqFactory;
    private readonly ILogger<PlatformAdminBootstrapService> _logger;

    public PlatformAdminBootstrapService(
        IAsyncSqlClient db,
        ITenantAwareLinqFactory linqFactory,
        ILogger<PlatformAdminBootstrapService> logger)
    {
        _db = db;
        _linqFactory = linqFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var bypassClient = _linqFactory.Create(NullTenantContext.Instance, bypassTenantFilter: true);

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

        var email = $"platform-admin-{GenerateRandomString(8)}@controleasy.local";
        var password = GenerateRandomString(16);
        var passwordHash = HashPassword(password);
        var id = Guid.NewGuid();
        var platformTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        await bypassClient.InsertAsync(
            new[] { "Id", "Email", "PasswordHash", "DisplayName", "Active", "MustChangePassword", "Roles", "CreatedAtUtc", "tenant_id" },
            "Users",
            new object[] { id, email, passwordHash, "Platform Admin", true, true, "PlatformAdmin", DateTime.UtcNow, platformTenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);

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

    private static string HashPassword(string password)
    {
        // TODO: Replace with BCrypt.Net-Next once the Security module is implemented (task 1.12).
        // This SHA256 placeholder is NOT suitable for production.
        using var sha = SHA256.Create();
        var salt = "ControlEasyReborn-PlaceholderSalt-CHANGE-ME";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + password));
        return Convert.ToBase64String(bytes);
    }
}