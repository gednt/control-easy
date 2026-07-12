using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class GetConfigurationHandler
{
    private readonly IConfigurationRepository _configurations;

    public GetConfigurationHandler(IConfigurationRepository configurations)
    {
        _configurations = configurations;
    }

    public async Task<ConfigurationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var entry = await _configurations.FindAsync(id, ct);
        if (entry is null)
        {
            throw new NotFoundException("Configuration " + id + " was not found.");
        }
        return CreateConfigurationHandler.ToResponse(entry);
    }
}