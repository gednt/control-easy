using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;

namespace ControlEasyReborn.Modules.Residents.Application.Handlers;

public sealed class ListResidentsHandler
{
    private readonly IResidentRepository _residents;

    public ListResidentsHandler(IResidentRepository residents)
    {
        _residents = residents;
    }

    public async Task<IReadOnlyList<ResidentResponse>> HandleAsync(string? search, int skip, int take, CancellationToken ct, Guid? apartmentId = null)
    {
        var residents = await _residents.ListAsync(search, skip, take, ct, apartmentId);
        return residents.Select(CreateResidentHandler.ToResponse).ToList();
    }
}