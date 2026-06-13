using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class ListGatehousesHandler
{
    private readonly IGatehouseRepository _gatehouses;

    public ListGatehousesHandler(IGatehouseRepository gatehouses)
    {
        _gatehouses = gatehouses;
    }

    public async Task<IReadOnlyList<GatehouseResponse>> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        var gatehouses = await _gatehouses.ListByTenantAsync(tenantId, ct);
        return gatehouses.Select(CreateGatehouseHandler.ToResponse).ToList();
    }
}