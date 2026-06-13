using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class UpdateAttendantProfileHandler
{
    private readonly IAttendantProfileRepository _profiles;
    private readonly IValidator<UpdateAttendantProfileRequest> _validator;

    public UpdateAttendantProfileHandler(IAttendantProfileRepository profiles, IValidator<UpdateAttendantProfileRequest> validator)
    {
        _profiles = profiles;
        _validator = validator;
    }

    public async Task<AttendantProfileResponse> HandleAsync(Guid id, UpdateAttendantProfileRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var profile = await _profiles.FindAsync(id, ct);
        if (profile is null)
        {
            throw new NotFoundException("Attendant profile " + id + " was not found.");
        }

        profile.UpdateDetails(request.DisplayName, request.ShiftId, request.GatehouseId, request.Permissions);
        await _profiles.UpdateAsync(profile, ct);
        return CreateAttendantProfileHandler.ToResponse(profile);
    }
}