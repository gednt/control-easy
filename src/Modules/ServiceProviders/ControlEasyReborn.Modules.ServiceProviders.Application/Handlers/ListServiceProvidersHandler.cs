using ControlEasyReborn.Modules.ServiceProviders.Application.Abstractions;
using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Handlers;

public sealed class ListServiceProvidersHandler
{
    private readonly IServiceProviderRepository _providers;

    public ListServiceProvidersHandler(IServiceProviderRepository providers)
    {
        _providers = providers;
    }

    public async Task<IReadOnlyList<ServiceProviderResponse>> HandleAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var providers = await _providers.ListAsync(search, skip, take, ct);
        return providers.Select(CreateServiceProviderHandler.ToResponse).ToList();
    }
}