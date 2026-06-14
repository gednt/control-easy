using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;
using FluentValidation;
using ValidationException = ControlEasyReborn.Modules.Tenants.Application.Errors.ValidationException;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class CreatePorteiroHandler
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantAdminRepository _adminRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreatePorteiroRequest> _validator;

    public CreatePorteiroHandler(
        ITenantRepository tenants,
        ITenantAdminRepository adminRepo,
        IPasswordHasher passwordHasher,
        IValidator<CreatePorteiroRequest> validator)
    {
        _tenants = tenants;
        _adminRepo = adminRepo;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<PorteiroResponse> HandleAsync(Guid tenantId, CreatePorteiroRequest request, CancellationToken ct)
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

        var email = request.Email.Trim();
        if (await _adminRepo.EmailExistsAsync(email, null, ct))
            throw new ConflictException("A user with email '" + email + "' already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        return await _adminRepo.CreatePorteiroAsync(tenantId, email, request.DisplayName.Trim(), passwordHash, ct);
    }
}
