using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Errors;

namespace ControlEasyReborn.Modules.Visits.Application.Handlers;

public sealed class CheckInVisitHandler
{
    private readonly IVisitRepository _visits;

    public CheckInVisitHandler(IVisitRepository visits)
    {
        _visits = visits;
    }

    public async Task<VisitResponse> HandleAsync(Guid id, Guid attendantProfileId, Guid? gatehouseId, CancellationToken ct)
    {
        var visit = await _visits.FindAsync(id, ct);
        if (visit is null)
        {
            throw new NotFoundException("Visit " + id + " was not found.");
        }

        visit.CheckIn(attendantProfileId, gatehouseId);
        await _visits.UpdateAsync(visit, ct);
        return CreateVisitHandler.ToResponse(visit);
    }
}