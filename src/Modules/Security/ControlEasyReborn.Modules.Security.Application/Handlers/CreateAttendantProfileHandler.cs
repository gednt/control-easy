using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class CreateAttendantProfileHandler
{
    private readonly IAttendantProfileRepository _profiles;
    private readonly IUserRepository _users;
    private readonly IValidator<CreateAttendantProfileRequest> _validator;

    public CreateAttendantProfileHandler(
        IAttendantProfileRepository profiles,
        IUserRepository users,
        IValidator<CreateAttendantProfileRequest> validator)
    {
        _profiles = profiles;
        _users = users;
        _validator = validator;
    }

    public async Task<AttendantProfileResponse> HandleAsync(CreateAttendantProfileRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var user = await _users.FindAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User " + request.UserId + " was not found.");
        }

        var profile = new AttendantProfile(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            userId: request.UserId,
            displayName: request.DisplayName,
            shiftId: request.ShiftId,
            gatehouseId: request.GatehouseId,
            permissions: request.Permissions,
            active: true,
            createdAtUtc: DateTime.UtcNow);

        await _profiles.CreateAsync(profile, ct);
        return ToResponse(profile);
    }

    internal static AttendantProfileResponse ToResponse(AttendantProfile p) =>
        new(p.Id, p.TenantId, p.UserId, p.DisplayName, p.ShiftId, p.GatehouseId, p.Permissions, p.Active, p.CreatedAtUtc);
}