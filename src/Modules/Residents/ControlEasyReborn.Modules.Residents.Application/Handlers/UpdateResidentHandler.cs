using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Residents.Application.Handlers;

public sealed class UpdateResidentHandler
{
    private readonly IResidentRepository _residents;
    private readonly IValidator<UpdateResidentRequest> _validator;

    public UpdateResidentHandler(IResidentRepository residents, IValidator<UpdateResidentRequest> validator)
    {
        _residents = residents;
        _validator = validator;
    }

    public async Task<ResidentResponse> HandleAsync(Guid id, UpdateResidentRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var resident = await _residents.FindAsync(id, ct);
        if (resident is null)
        {
            throw new NotFoundException("Resident " + id + " was not found.");
        }

        resident.UpdateDetails(request.Name, request.Cpf, request.Email, request.Phone, request.ApartmentId);
        if (!request.Active)
        {
            resident.Deactivate();
        }
        await _residents.UpdateAsync(resident, ct);
        return CreateResidentHandler.ToResponse(resident);
    }
}