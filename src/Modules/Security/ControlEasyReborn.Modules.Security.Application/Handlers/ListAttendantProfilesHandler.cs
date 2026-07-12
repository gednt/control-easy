using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Application.Contracts;

namespace ControlEasyReborn.Modules.Security.Application.Handlers;

public sealed class ListAttendantProfilesHandler
{
    private readonly IAttendantProfileRepository _profiles;

    public ListAttendantProfilesHandler(IAttendantProfileRepository profiles)
    {
        _profiles = profiles;
    }

    public async Task<IReadOnlyList<AttendantProfileResponse>> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        var profiles = await _profiles.ListByTenantAsync(tenantId, ct);
        return profiles.Select(CreateAttendantProfileHandler.ToResponse).ToList();
    }
}