namespace ControlEasyReborn.Modules.Administration.Application.Contracts;

public sealed record CondominiumSettingsResponse(
    Guid Id,
    Guid TenantId,
    int VisitDurationMinutes,
    bool RequireShiftHandoverNotes,
    int DefaultShiftLengthHours,
    string? EmergencyContactPhone,
    string AllowedVisitorStartHour,
    string AllowedVisitorEndHour,
    bool AutoCheckoutAtMidnight,
    int MaxActiveVisitorsPerUnit,
    bool PhotoRequiredVisitors,
    bool PhotoRequiredProviders,
    bool PhotoRequiredResidents,
    bool AllowOverrideOnRefusal,
    int OverdueVisitAlertMinutes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record UpdateCondominiumSettingsRequest(
    int VisitDurationMinutes,
    bool RequireShiftHandoverNotes,
    int DefaultShiftLengthHours,
    string? EmergencyContactPhone,
    string AllowedVisitorStartHour,
    string AllowedVisitorEndHour,
    bool AutoCheckoutAtMidnight,
    int MaxActiveVisitorsPerUnit,
    bool PhotoRequiredVisitors,
    bool PhotoRequiredProviders,
    bool PhotoRequiredResidents,
    bool AllowOverrideOnRefusal,
    int OverdueVisitAlertMinutes);
