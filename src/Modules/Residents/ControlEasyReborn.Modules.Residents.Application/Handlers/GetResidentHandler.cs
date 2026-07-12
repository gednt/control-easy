using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Errors;

namespace ControlEasyReborn.Modules.Residents.Application.Handlers;

public sealed class GetResidentHandler
{
    private readonly IResidentRepository _residents;

    public GetResidentHandler(IResidentRepository residents)
    {
        _residents = residents;
    }

    public async Task<ResidentResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var resident = await _residents.FindAsync(id, ct);
        if (resident is null)
        {
            throw new NotFoundException("Resident " + id + " was not found.");
        }
        return CreateResidentHandler.ToResponse(resident);
    }
}