using System.Data;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class RevokeTenantAdminHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public RevokeTenantAdminHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        var admins = await _adminRepo.ListAdminsAsync(tenantId, ct);
        if (admins.All(a => a.UserId != userId))
        {
            throw new NotFoundException($"Tenant admin {userId} was not found for tenant {tenantId}.");
        }

        if (admins.Count(a => a.Active && a.UserId != userId) == 0 && admins.Count(a => a.Active) > 0)
        {
            throw new Errors.ConflictException(
                "This is the last active administrator of this condominium. Add another administrator before revoking this one.");
        }

        await _adminRepo.RevokeAdminAsync(userId, ct);
    }
}

public sealed class SuspendTenantAdminHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public SuspendTenantAdminHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        await _adminRepo.SetAdminActiveAsync(tenantId, userId, active: false, ct);
    }
}

public sealed class ResumeTenantAdminHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public ResumeTenantAdminHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        await _adminRepo.SetAdminActiveAsync(tenantId, userId, active: true, ct);
    }
}

public sealed class DeleteTenantAdminHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public DeleteTenantAdminHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        var admins = await _adminRepo.ListAdminsAsync(tenantId, ct);
        var target = admins.FirstOrDefault(a => a.UserId == userId);
        if (target is null)
        {
            throw new NotFoundException($"Tenant admin {userId} was not found for tenant {tenantId}.");
        }

        if (admins.Count(a => a.Active && a.UserId != userId) == 0)
        {
            throw new Errors.ConflictException(
                "This is the last administrator of this condominium. Add another administrator before removing this one.");
        }

        await _adminRepo.DeleteAdminAsync(tenantId, userId, ct);
    }
}

public sealed class UpdateTenantPorteiroHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public UpdateTenantPorteiroHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task<PorteiroResponse> HandleAsync(Guid tenantId, Guid userId, UpdatePorteiroRequest request, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        var porteiros = await _adminRepo.ListPorteirosAsync(tenantId, ct);
        if (porteiros.All(p => p.UserId != userId))
        {
            throw new NotFoundException($"Porteiro {userId} was not found for tenant {tenantId}.");
        }

        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();
        if (await _adminRepo.EmailExistsAsync(email, userId, ct))
            throw new Errors.ConflictException($"A user with email '{email}' already exists.");

        await _adminRepo.UpdatePorteiroAsync(tenantId, userId, email, displayName, ct);

        var updated = porteiros
            .Where(p => p.UserId == userId)
            .Select(p => p with { Email = email, DisplayName = displayName })
            .First();
        return updated;
    }
}

public sealed class SuspendTenantPorteiroHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public SuspendTenantPorteiroHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        await _adminRepo.SetPorteiroActiveAsync(tenantId, userId, active: false, ct);
    }
}

public sealed class ResumeTenantPorteiroHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public ResumeTenantPorteiroHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        await _adminRepo.SetPorteiroActiveAsync(tenantId, userId, active: true, ct);
    }
}

public sealed class DeleteTenantPorteiroHandler
{
    private readonly ITenantAdminRepository _adminRepo;
    private readonly ITenantRepository _tenants;

    public DeleteTenantPorteiroHandler(ITenantAdminRepository adminRepo, ITenantRepository tenants)
    {
        _adminRepo = adminRepo;
        _tenants = tenants;
    }

    public async Task HandleAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new NotFoundException($"Tenant {tenantId} was not found.");
        }

        await _adminRepo.DeletePorteiroAsync(tenantId, userId, ct);
    }
}