using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Errors;

namespace ControlEasyReborn.Modules.Visits.Application.Handlers;

public sealed class CheckOutVisitHandler
{
    private readonly IVisitRepository _visits;

    public CheckOutVisitHandler(IVisitRepository visits)
    {
        _visits = visits;
    }

    public async Task<VisitResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var visit = await _visits.FindAsync(id, ct);
        if (visit is null)
        {
            throw new NotFoundException("Visit " + id + " was not found.");
        }

        try
        {
            visit.CheckOut();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _visits.UpdateAsync(visit, ct);
        return CreateVisitHandler.ToResponse(visit);
    }
}