using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Domain.Entities;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class GetCondominiumSettingsHandler
{
    private readonly ICondominiumSettingsRepository _repository;

    public GetCondominiumSettingsHandler(ICondominiumSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<CondominiumSettingsResponse> HandleAsync(Guid tenantId, CancellationToken ct)
    {
        var settings = await _repository.GetByTenantIdAsync(tenantId, ct);
        if (settings is null)
        {
            settings = CondominiumSettings.CreateDefault(tenantId);
            await _repository.SaveAsync(settings, ct);
        }

        return ToResponse(settings);
    }

    private static CondominiumSettingsResponse ToResponse(CondominiumSettings s)
    {
        return new CondominiumSettingsResponse(
            s.Id,
            s.TenantId,
            s.VisitDurationMinutes,
            s.RequireShiftHandoverNotes,
            s.DefaultShiftLengthHours,
            s.EmergencyContactPhone,
            s.AllowedVisitorStartHour,
            s.AllowedVisitorEndHour,
            s.AutoCheckoutAtMidnight,
            s.MaxActiveVisitorsPerUnit,
            s.PhotoRequiredVisitors,
            s.PhotoRequiredProviders,
            s.PhotoRequiredResidents,
            s.AllowOverrideOnRefusal,
            s.OverdueVisitAlertMinutes,
            s.CreatedAtUtc,
            s.UpdatedAtUtc);
    }
}
