using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using ControlEasyReborn.Modules.Tenants.Application.Errors;
using ControlEasyReborn.Modules.Tenants.Domain;
using FluentValidation;

namespace ControlEasyReborn.Modules.Tenants.Application.Handlers;

public sealed class CreateTenantHandler
{
    private readonly ITenantRepository _tenants;
    private readonly IValidator<CreateTenantRequest> _validator;

    public CreateTenantHandler(ITenantRepository tenants, IValidator<CreateTenantRequest> validator)
    {
        _tenants = tenants;
        _validator = validator;
    }

    public async Task<TenantResponse> HandleAsync(CreateTenantRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new ControlEasyReborn.Modules.Tenants.Application.Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var existing = await _tenants.FindBySlugAsync(request.Slug, ct);
        if (existing is not null)
        {
            throw new ConflictException("A tenant with slug '" + request.Slug + "' already exists.");
        }

        var tenant = new Tenant(
            id: Guid.NewGuid(),
            slug: request.Slug,
            displayName: request.DisplayName,
            status: TenantStatus.Active,
            createdAtUtc: DateTime.UtcNow);

        await _tenants.AddAsync(tenant, ct);
        return ToResponse(tenant);
    }

    internal static TenantResponse ToResponse(Tenant t) =>
        new(t.Id, t.Slug, t.DisplayName, t.Status.ToString(), t.CreatedAtUtc);
}
