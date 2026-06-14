using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Errors;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Residents.Application.Handlers;

public sealed class CreateResidentHandler
{
    private readonly IResidentRepository _residents;
    private readonly IApartmentRepository _apartments;
    private readonly IValidator<CreateResidentRequest> _validator;

    public CreateResidentHandler(
        IResidentRepository residents,
        IApartmentRepository apartments,
        IValidator<CreateResidentRequest> validator)
    {
        _residents = residents;
        _apartments = apartments;
        _validator = validator;
    }

    public async Task<ResidentResponse> HandleAsync(CreateResidentRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        if (request.ApartmentId.HasValue && !await _apartments.ExistsAsync(request.ApartmentId.Value, ct))
        {
            throw new NotFoundException("Apartment " + request.ApartmentId + " was not found.");
        }

        var resident = new Resident(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            name: request.Name,
            cpf: request.Cpf,
            email: request.Email,
            phone: request.Phone,
            apartmentId: request.ApartmentId,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _residents.AddAsync(resident, ct);
        return ToResponse(resident);
    }

    internal static ResidentResponse ToResponse(Resident r) =>
        new(r.Id, r.TenantId, r.Name, r.Cpf, r.Email, r.Phone, r.ApartmentId, r.Active, r.CreatedAtUtc);
}