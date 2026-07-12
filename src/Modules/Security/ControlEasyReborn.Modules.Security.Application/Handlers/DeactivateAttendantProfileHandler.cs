using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class DeactivateAttendantProfileHandler
{
    private readonly IAttendantProfileRepository _profiles;

    public DeactivateAttendantProfileHandler(IAttendantProfileRepository profiles)
    {
        _profiles = profiles;
    }

    public async Task<AttendantProfileResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var profile = await _profiles.FindAsync(id, ct);
        if (profile is null)
        {
            throw new NotFoundException("Attendant profile " + id + " was not found.");
        }

        profile.Deactivate();
        await _profiles.DeactivateAsync(profile, ct);
        return CreateAttendantProfileHandler.ToResponse(profile);
    }
}