using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;
using ControlEasyReborn.Modules.Security.Application.Errors;
using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class GetMyProfileHandler
{
    private readonly IAttendantProfileRepository _profiles;
    private readonly ITenantContext _tenantContext;

    public GetMyProfileHandler(IAttendantProfileRepository profiles, ITenantContext tenantContext)
    {
        _profiles = profiles;
        _tenantContext = tenantContext;
    }

    public async Task<AttendantProfileResponse> HandleAsync(CancellationToken ct)
    {
        var profileId = _tenantContext.ProfileId
            ?? throw new UnauthorizedException("No active profile in current token.");

        var profile = await _profiles.FindAsync(profileId, ct);
        if (profile is null)
        {
            throw new NotFoundException("Attendant profile " + profileId + " was not found.");
        }
        return CreateAttendantProfileHandler.ToResponse(profile);
    }
}