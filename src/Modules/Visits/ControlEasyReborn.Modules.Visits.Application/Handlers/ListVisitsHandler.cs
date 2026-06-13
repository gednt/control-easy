using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Application.Contracts;

namespace ControlEasyReborn.Modules.Visits.Application.Handlers;

public sealed class ListVisitsHandler
{
    private readonly IVisitRepository _visits;

    public ListVisitsHandler(IVisitRepository visits)
    {
        _visits = visits;
    }

    public async Task<IReadOnlyList<VisitResponse>> HandleAsync(Guid tenantId, string? status, int skip, int take, CancellationToken ct)
    {
        IReadOnlyList<Domain.Entities.Visit> visits;

        if (string.Equals(status, "open", StringComparison.OrdinalIgnoreCase))
        {
            visits = await _visits.ListOpenAsync(tenantId, ct);
        }
        else
        {
            visits = await _visits.ListAsync(tenantId, status, skip, take, ct);
        }

        return visits.Select(CreateVisitHandler.ToResponse).ToList();
    }
}