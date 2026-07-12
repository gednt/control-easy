using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Abstractions;

public interface ITenantBackupService
{
    Task<BackupResult> CreateBackupAsync(Guid tenantId, string slug, CancellationToken ct);
}