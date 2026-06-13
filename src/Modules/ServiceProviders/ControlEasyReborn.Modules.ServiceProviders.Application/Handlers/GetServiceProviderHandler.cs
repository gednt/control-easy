using ControlEasyReborn.Modules.ServiceProviders.Application.Abstractions;
using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;
using ControlEasyReborn.Modules.ServiceProviders.Application.Errors;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Handlers;

public sealed class GetServiceProviderHandler
{
    private readonly IServiceProviderRepository _providers;

    public GetServiceProviderHandler(IServiceProviderRepository providers)
    {
        _providers = providers;
    }

    public async Task<ServiceProviderResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var provider = await _providers.FindAsync(id, ct);
        if (provider is null)
        {
            throw new NotFoundException("ServiceProvider " + id + " was not found.");
        }
        return CreateServiceProviderHandler.ToResponse(provider);
    }
}