using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Errors;

namespace ControlEasyReborn.Modules.Apartments.Application.Handlers;

public sealed class GetApartmentHandler
{
    private readonly IApartmentRepository _apartments;

    public GetApartmentHandler(IApartmentRepository apartments)
    {
        _apartments = apartments;
    }

    public async Task<ApartmentResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var apartment = await _apartments.FindAsync(id, ct);
        if (apartment is null)
        {
            throw new NotFoundException("Apartment " + id + " was not found.");
        }

        return CreateApartmentHandler.ToResponse(apartment);
    }
}
