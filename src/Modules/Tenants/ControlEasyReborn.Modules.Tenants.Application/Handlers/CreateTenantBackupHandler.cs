using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class CreateTenantBackupHandler
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantBackupService _backupService;

    public CreateTenantBackupHandler(ITenantRepository tenants, ITenantBackupService backupService)
    {
        _tenants = tenants;
        _backupService = backupService;
    }

    public async Task<BackupResult> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException("Tenant " + tenantId + " was not found.");
        }

        return await _backupService.CreateBackupAsync(tenantId, tenant.Slug, ct);
    }
}