using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class CreateTenantAdminHandler
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantAdminRepository _adminRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateTenantAdminRequest> _validator;

    public CreateTenantAdminHandler(
        ITenantRepository tenants,
        ITenantAdminRepository adminRepo,
        IPasswordHasher passwordHasher,
        IValidator<CreateTenantAdminRequest> validator)
    {
        _tenants = tenants;
        _adminRepo = adminRepo;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<TenantAdminResponse> HandleAsync(Guid tenantId, CreateTenantAdminRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new ControlEasyReborn.Modules.Tenants.Application.Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var tenant = await _tenants.FindAsync(tenantId, ct);
        if (tenant is null)
        {
            throw new ControlEasyReborn.Modules.Tenants.Application.Errors.NotFoundException("Tenant " + tenantId + " was not found.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        return await _adminRepo.CreateAdminAsync(tenantId, request.Email, request.DisplayName, passwordHash, ct);
    }
}