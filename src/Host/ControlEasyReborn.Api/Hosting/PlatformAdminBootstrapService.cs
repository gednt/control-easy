using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.SharedKernel.Bootstrap;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography;

namespace ControlEasyReborn.Api.Hosting;

public sealed class PlatformAdminBootstrapService : IHostedService
{
    private const int MaxAttempts = 6;

    private static readonly TimeSpan[] RetryBackoff =
    [
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(16),
        TimeSpan.FromSeconds(16),
    ];

    public Func<TimeSpan, CancellationToken, Task> DelayAsync { get; set; } =
        static (delay, ct) => Task.Delay(delay, ct);

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
        var platformTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var attempts = 0;
        while (true)
        {
            attempts++;
            ct.ThrowIfCancellationRequested();

            var existing = await bypassClient.SelectAsync(
                fields: "Id",
                table: "Users",
                whereClause: "Roles LIKE '%PlatformAdmin%'",
                parameters: Array.Empty<object>(),
                ct: ct);

            if (!string.IsNullOrWhiteSpace(bypassClient.Error))
            {
                if (attempts >= MaxAttempts)
                {
                    _logger.LogError(
                        "PlatformAdmin bootstrap failed after {Attempts} attempts. Last error: {Error}",
                        attempts, bypassClient.Error);
                    throw new InvalidOperationException(
                        $"PlatformAdmin bootstrap failed after {attempts} attempts: {bypassClient.Error}");
                }

                _logger.LogWarning(
                    "PlatformAdmin bootstrap pre-check failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s. Error: {Error}",
                    attempts, MaxAttempts, RetryBackoff[attempts - 1].TotalSeconds, bypassClient.Error);
                await DelayAsync(RetryBackoff[attempts - 1], ct);
                continue;
            }

            if (existing is not null && existing.Rows.Count > 0)
            {
                _logger.LogInformation("PlatformAdmin exists; checking attendant profile.");

                var profile = await bypassClient.SelectAsync(
                    fields: "Id",
                    table: "AttendantProfiles",
                    whereClause: "UserId = @param0",
                    parameters: new object[] { existing.Rows[0]["Id"].ToString() ?? string.Empty },
                    ct: ct);

                if (!string.IsNullOrWhiteSpace(bypassClient.Error))
                {
                    if (attempts >= MaxAttempts)
                    {
                        _logger.LogError(
                            "PlatformAdmin bootstrap profile pre-check failed after {Attempts} attempts. Last error: {Error}",
                            attempts, bypassClient.Error);
                        throw new InvalidOperationException(
                            $"PlatformAdmin bootstrap failed after {attempts} attempts: {bypassClient.Error}");
                    }

                    _logger.LogWarning(
                        "PlatformAdmin bootstrap profile pre-check failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s. Error: {Error}",
                        attempts, MaxAttempts, RetryBackoff[attempts - 1].TotalSeconds, bypassClient.Error);
                    await DelayAsync(RetryBackoff[attempts - 1], ct);
                    continue;
                }

                if (profile is not null && profile.Rows.Count > 0)
                {
                    _logger.LogInformation("PlatformAdmin already exists; skipping bootstrap.");
                    return;
                }

                var profileRepairInserted = await bypassClient.InsertAsync(
                    new[] { "Id", "TenantId", "UserId", "DisplayName", "ShiftId", "GatehouseId", "Permissions", "Active", "CreatedAtUtc", "tenant_id" },
                    "AttendantProfiles",
                    new object?[]
                    {
                        Guid.NewGuid(), platformTenantId, existing.Rows[0]["Id"], "Platform Admin",
                        DBNull.Value, DBNull.Value, "platform:*", true, DateTime.UtcNow, platformTenantId
                    },
                    primaryKeyName: "Id",
                    autoIncrement: false,
                    ct: ct);

                if (profileRepairInserted)
                {
                    _logger.LogInformation("Missing PlatformAdmin attendant profile repaired.");
                    return;
                }

                if (attempts >= MaxAttempts)
                {
                    var error = string.IsNullOrWhiteSpace(bypassClient.Error)
                        ? "Profile insert failed (InsertAsync returned false)"
                        : bypassClient.Error;
                    _logger.LogError(
                        "PlatformAdmin bootstrap failed after {Attempts} attempts. Last error: {Error}",
                        attempts, error);
                    throw new InvalidOperationException(
                        $"PlatformAdmin bootstrap failed after {attempts} attempts: {error}");
                }

                _logger.LogWarning(
                    "PlatformAdmin bootstrap profile seeding failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s. Error: {Error}",
                    attempts, MaxAttempts, RetryBackoff[attempts - 1].TotalSeconds, bypassClient.Error);
                await DelayAsync(RetryBackoff[attempts - 1], ct);
                continue;
            }

            var email = string.IsNullOrWhiteSpace(_bootstrapOptions.PlatformAdminEmail)
                ? $"platform-admin-{GenerateRandomString(8)}@controleasy.local"
                : _bootstrapOptions.PlatformAdminEmail.Trim();
            var password = string.IsNullOrWhiteSpace(_bootstrapOptions.PlatformAdminPassword)
                ? GenerateRandomString(16)
                : _bootstrapOptions.PlatformAdminPassword;
            var passwordHash = passwordHasher.Hash(password);
            var id = Guid.NewGuid();

            var now = DateTime.UtcNow;

            var userInserted = await bypassClient.InsertAsync(
                new[] { "Id", "TenantId", "Email", "PasswordHash", "DisplayName", "Active", "MustChangePassword", "Roles", "CreatedAtUtc", "tenant_id" },
                "Users",
                new object[] { id, platformTenantId, email, passwordHash, "Platform Admin", true, true, "PlatformAdmin", now, platformTenantId },
                primaryKeyName: "Id",
                autoIncrement: false,
                ct: ct);

            var profileInserted = userInserted && await bypassClient.InsertAsync(
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

            if (userInserted && profileInserted)
            {
                _credentialsStore.Set(email, password);
                _logger.LogWarning("// CHANGE IMMEDIATELY — PlatformAdmin seeded. Email: {Email}, Password: {Password}", email, password);
                return;
            }

            if (attempts >= MaxAttempts)
            {
                var error = string.IsNullOrWhiteSpace(bypassClient.Error)
                    ? $"InsertAsync returned false (user={userInserted}, profile={profileInserted})"
                    : bypassClient.Error;
                _logger.LogError(
                    "PlatformAdmin bootstrap failed after {Attempts} attempts. Last error: {Error}",
                    attempts, error);
                throw new InvalidOperationException(
                    $"PlatformAdmin bootstrap failed after {attempts} attempts: {error}");
            }

            _logger.LogWarning(
                "PlatformAdmin bootstrap seeding failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s. Error: {Error}",
                attempts, MaxAttempts, RetryBackoff[attempts - 1].TotalSeconds, bypassClient.Error);
            await DelayAsync(RetryBackoff[attempts - 1], ct);
        }
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
