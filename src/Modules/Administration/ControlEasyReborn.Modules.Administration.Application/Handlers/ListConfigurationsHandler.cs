using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class ListConfigurationsHandler
{
    private readonly IConfigurationRepository _configurations;

    public ListConfigurationsHandler(IConfigurationRepository configurations)
    {
        _configurations = configurations;
    }

    public async Task<IReadOnlyList<ConfigurationResponse>> HandleAsync(Guid tenantId, int skip, int take, CancellationToken ct)
    {
        var entries = await _configurations.ListByTenantAsync(tenantId, skip, take, ct);
        return entries.Select(CreateConfigurationHandler.ToResponse).ToList();
    }
}