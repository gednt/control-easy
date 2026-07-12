using ControlEasyReborn.Modules.ServiceProviders.Application.Abstractions;
using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;
using ControlEasyReborn.Modules.ServiceProviders.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Handlers;

public sealed class UpdateServiceProviderHandler
{
    private readonly IServiceProviderRepository _providers;
    private readonly IValidator<UpdateServiceProviderRequest> _validator;

    public UpdateServiceProviderHandler(IServiceProviderRepository providers, IValidator<UpdateServiceProviderRequest> validator)
    {
        _providers = providers;
        _validator = validator;
    }

    public async Task<ServiceProviderResponse> HandleAsync(Guid id, UpdateServiceProviderRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var provider = await _providers.FindAsync(id, ct);
        if (provider is null)
        {
            throw new NotFoundException("ServiceProvider " + id + " was not found.");
        }

        provider.UpdateDetails(request.Name, request.Document, request.Phone, request.Email, request.ServiceType, request.Company);
        await _providers.UpdateAsync(provider, ct);
        return CreateServiceProviderHandler.ToResponse(provider);
    }
}