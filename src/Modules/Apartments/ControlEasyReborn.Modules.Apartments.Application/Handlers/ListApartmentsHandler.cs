using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;

namespace ControlEasyReborn.Modules.Apartments.Application.Handlers;

public sealed class ListApartmentsHandler
{
    private readonly IApartmentRepository _apartments;

    public ListApartmentsHandler(IApartmentRepository apartments)
    {
        _apartments = apartments;
    }

    public async Task<IReadOnlyList<ApartmentResponse>> HandleAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var apartments = await _apartments.ListAsync(search, skip, take, ct);
        return apartments.Select(CreateApartmentHandler.ToResponse).ToList();
    }
}
