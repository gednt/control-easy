using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class GetConfigurationByKeyHandler
{
    private readonly IConfigurationRepository _configurations;

    public GetConfigurationByKeyHandler(IConfigurationRepository configurations)
    {
        _configurations = configurations;
    }

    public async Task<ConfigurationResponse> HandleAsync(Guid tenantId, string key, CancellationToken ct)
    {
        var entry = await _configurations.GetByKeyAsync(tenantId, key, ct);
        if (entry is null)
        {
            throw new NotFoundException("Configuration with key '" + key + "' was not found.");
        }
        return CreateConfigurationHandler.ToResponse(entry);
    }
}