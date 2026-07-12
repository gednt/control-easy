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

        try
        {
            visit.CheckIn(attendantProfileId, gatehouseId);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _visits.UpdateAsync(visit, ct);
        return CreateVisitHandler.ToResponse(visit);
    }
}