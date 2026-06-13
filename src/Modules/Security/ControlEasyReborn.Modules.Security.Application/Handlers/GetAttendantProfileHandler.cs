using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class GetAttendantProfileHandler
{
    private readonly IAttendantProfileRepository _profiles;

    public GetAttendantProfileHandler(IAttendantProfileRepository profiles)
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
        return CreateAttendantProfileHandler.ToResponse(profile);
    }
}