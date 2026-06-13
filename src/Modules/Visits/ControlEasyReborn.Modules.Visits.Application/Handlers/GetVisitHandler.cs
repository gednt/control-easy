using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Application.Contracts;
using ControlEasyReborn.Modules.Visits.Application.Errors;

namespace ControlEasyReborn.Modules.Visits.Application.Handlers;

public sealed class GetVisitHandler
{
    private readonly IVisitRepository _visits;

    public GetVisitHandler(IVisitRepository visits)
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
        return CreateVisitHandler.ToResponse(visit);
    }
}