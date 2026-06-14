using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using ControlEasyReborn.Modules.Apartments.Application.Errors;
using ControlEasyReborn.Modules.Apartments.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Apartments.Application.Handlers;

public sealed class CreateApartmentHandler
{
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<CreateApartmentRequest> _validator;

    public CreateApartmentHandler(IApartmentRepository apartments, IValidator<CreateApartmentRequest> validator)
    {
        _apartments = apartments;
        _validator = validator;
    }

    public async Task<ApartmentResponse> HandleAsync(CreateApartmentRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var existing = await _apartments.FindByBlockUnitAsync(request.Block, request.Unit, ct);
        if (existing is not null)
        {
            throw new ConflictException($"Apartment {request.Block}-{request.Unit} already exists.");
        }

        var apartment = new Apartment(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            block: request.Block,
            unit: request.Unit,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _apartments.AddAsync(apartment, ct);
        return ToResponse(apartment);
    }

    internal static ApartmentResponse ToResponse(Apartment a) =>
        new(a.Id, a.TenantId, a.Block, a.Unit, a.Active, a.CreatedAtUtc, a.UpdatedAtUtc);
}
