using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class ListShiftsHandler
{
    private readonly IShiftRepository _shifts;

    public ListShiftsHandler(IShiftRepository shifts)
    {
        _shifts = shifts;
    }

    public async Task<IReadOnlyList<ShiftResponse>> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        var shifts = await _shifts.ListByTenantAsync(tenantId, ct);
        return shifts.Select(CreateShiftHandler.ToResponse).ToList();
    }
}