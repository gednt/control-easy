using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;
using FluentValidation;
using ValidationException = ControlEasyReborn.Modules.Tenants.Application.Errors.ValidationException;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class UpdateTenantAdminHandler
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantAdminRepository _adminRepo;
    private readonly IValidator<UpdateTenantAdminRequest> _validator;

    public UpdateTenantAdminHandler(
        ITenantRepository tenants,
        ITenantAdminRepository adminRepo,
        IValidator<UpdateTenantAdminRequest> validator)
    {
        _tenants = tenants;
        _adminRepo = adminRepo;
        _validator = validator;
    }

    public async Task<TenantAdminResponse> HandleAsync(Guid tenantId, Guid userId, UpdateTenantAdminRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
            throw new NotFoundException("Tenant " + tenantId + " was not found.");

        var admins = await _adminRepo.ListAdminsAsync(tenantId, ct);
        if (!admins.Any(a => a.UserId == userId))
            throw new NotFoundException($"Tenant admin {userId} was not found for tenant {tenantId}.");

        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();
        if (await _adminRepo.EmailExistsAsync(email, userId, ct))
            throw new ConflictException("A user with email '" + email + "' already exists.");

        return await _adminRepo.UpdateAdminAsync(tenantId, userId, email, displayName, ct);
    }
}
