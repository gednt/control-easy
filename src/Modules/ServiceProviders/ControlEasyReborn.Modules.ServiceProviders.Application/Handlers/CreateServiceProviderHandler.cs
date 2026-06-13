using ControlEasyReborn.Modules.ServiceProviders.Application.Abstractions;
using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;
using ControlEasyReborn.Modules.ServiceProviders.Application.Errors;
using ControlEasyReborn.Modules.ServiceProviders.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Handlers;

public sealed class CreateServiceProviderHandler
{
    private readonly IServiceProviderRepository _providers;
    private readonly IValidator<CreateServiceProviderRequest> _validator;

    public CreateServiceProviderHandler(IServiceProviderRepository providers, IValidator<CreateServiceProviderRequest> validator)
    {
        _providers = providers;
        _validator = validator;
    }

    public async Task<ServiceProviderResponse> HandleAsync(CreateServiceProviderRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var provider = new ServiceProvider(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            name: request.Name,
            document: request.Document,
            phone: request.Phone,
            email: request.Email,
            serviceType: request.ServiceType,
            company: request.Company,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _providers.AddAsync(provider, ct);
        return ToResponse(provider);
    }

    internal static ServiceProviderResponse ToResponse(ServiceProvider p) =>
        new(p.Id, p.TenantId, p.Name, p.Document, p.Phone, p.Email, p.ServiceType, p.Company, p.Active, p.CreatedAtUtc, p.UpdatedAtUtc);
}