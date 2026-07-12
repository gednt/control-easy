using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Apartments.Application.Handlers;

public sealed class UpdateApartmentHandler
{
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<UpdateApartmentRequest> _validator;

    public UpdateApartmentHandler(IApartmentRepository apartments, IValidator<UpdateApartmentRequest> validator)
    {
        _apartments = apartments;
        _validator = validator;
    }

    public async Task<ApartmentResponse> HandleAsync(Guid id, UpdateApartmentRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var apartment = await _apartments.FindAsync(id, ct);
        if (apartment is null)
        {
            throw new NotFoundException("Apartment " + id + " was not found.");
        }

        var duplicate = await _apartments.FindByBlockUnitAsync(request.Block, request.Unit, ct);
        if (duplicate is not null && duplicate.Id != id)
        {
            throw new ConflictException($"Apartment {request.Block}-{request.Unit} already exists.");
        }

        apartment.UpdateDetails(request.Block, request.Unit, request.Active);
        await _apartments.UpdateAsync(apartment, ct);
        return CreateApartmentHandler.ToResponse(apartment);
    }
}
