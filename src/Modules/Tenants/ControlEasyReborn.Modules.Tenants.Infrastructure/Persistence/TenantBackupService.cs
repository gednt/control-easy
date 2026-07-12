using System.Diagnostics;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using Microsoft.Extensions.Configuration;

namespace ControlEasyReborn.Modules.Tenants.Infrastructure.Persistence;

public sealed class TenantBackupService : ITenantBackupService
{
    private readonly IConfiguration _configuration;

    public TenantBackupService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<BackupResult> CreateBackupAsync(Guid tenantId, string slug, CancellationToken ct)
    {
        var basePath = _configuration["Backup:Path"] ?? "./backups/tenants/";
        var tenantDir = Path.Combine(basePath, slug);
        Directory.CreateDirectory(tenantDir);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName = $"{timestamp}.sql.gz";
        var filePath = Path.Combine(tenantDir, fileName);

        var dbHost = _configuration["Db:Host"] ?? "localhost";
        var dbPort = _configuration["Db:Port"] ?? "3306";
        var dbDatabase = _configuration["Db:Database"] ?? "controleasydb";
        var dbUser = _configuration["Db:Username"] ?? "root";
        var dbPassword = _configuration["Db:Password"] ?? "";

        var dumpArgs = $"--host={dbHost} --port={dbPort} --user={dbUser} --password={dbPassword} --single-transaction --routines --triggers {dbDatabase}";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "mysqldump",
            Arguments = dumpArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var gzipStartInfo = new ProcessStartInfo
        {
            FileName = "gzip",
            Arguments = "-9",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var dumpProcess = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start mysqldump process.");
        using var gzipProcess = Process.Start(gzipStartInfo) ?? throw new InvalidOperationException("Failed to start gzip process.");

        var dumpTask = dumpProcess.StandardOutput.BaseStream.CopyToAsync(gzipProcess.StandardInput.BaseStream, ct);
        var gzipTask = gzipProcess.StandardOutput.BaseStream.CopyToAsync(
            new FileStream(filePath, FileMode.Create, FileAccess.Write), ct);

        await dumpTask;
        gzipProcess.StandardInput.Close();
        await gzipTask;

        await dumpProcess.WaitForExitAsync(ct);
        await gzipProcess.WaitForExitAsync(ct);

        var fileInfo = new FileInfo(filePath);
        return new BackupResult(tenantId, slug, filePath, fileInfo.Length, DateTime.UtcNow);
    }
}