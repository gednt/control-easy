namespace ControlEasyReborn.Modules.Administration.Domain.Entities;

public sealed class CondominiumSettings
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    // Gatehouse Operations
    public int VisitDurationMinutes { get; private set; } = 120;
    public bool RequireShiftHandoverNotes { get; private set; } = true;
    public int DefaultShiftLengthHours { get; private set; } = 8;
    public string? EmergencyContactPhone { get; private set; }

    // Visitor & Access Rules
    public string AllowedVisitorStartHour { get; private set; } = "06:00";
    public string AllowedVisitorEndHour { get; private set; } = "22:00";
    public bool AutoCheckoutAtMidnight { get; private set; } = true;
    public int MaxActiveVisitorsPerUnit { get; private set; } = 5;

    // Photo & Consent Policies
    public bool PhotoRequiredVisitors { get; private set; } = true;
    public bool PhotoRequiredProviders { get; private set; } = true;
    public bool PhotoRequiredResidents { get; private set; } = false;
    public bool AllowOverrideOnRefusal { get; private set; } = true;

    // Alerts & Notifications
    public int OverdueVisitAlertMinutes { get; private set; } = 15;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private CondominiumSettings() { }

    public CondominiumSettings(
        Guid id,
        Guid tenantId,
        int visitDurationMinutes,
        bool requireShiftHandoverNotes,
        int defaultShiftLengthHours,
        string? emergencyContactPhone,
        string allowedVisitorStartHour,
        string allowedVisitorEndHour,
        bool autoCheckoutAtMidnight,
        int maxActiveVisitorsPerUnit,
        bool photoRequiredVisitors,
        bool photoRequiredProviders,
        bool photoRequiredResidents,
        bool allowOverrideOnRefusal,
        int overdueVisitAlertMinutes,
        DateTime createdAtUtc,
        DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        VisitDurationMinutes = visitDurationMinutes;
        RequireShiftHandoverNotes = requireShiftHandoverNotes;
        DefaultShiftLengthHours = defaultShiftLengthHours;
        EmergencyContactPhone = emergencyContactPhone;
        AllowedVisitorStartHour = allowedVisitorStartHour;
        AllowedVisitorEndHour = allowedVisitorEndHour;
        AutoCheckoutAtMidnight = autoCheckoutAtMidnight;
        MaxActiveVisitorsPerUnit = maxActiveVisitorsPerUnit;
        PhotoRequiredVisitors = photoRequiredVisitors;
        PhotoRequiredProviders = photoRequiredProviders;
        PhotoRequiredResidents = photoRequiredResidents;
        AllowOverrideOnRefusal = allowOverrideOnRefusal;
        OverdueVisitAlertMinutes = overdueVisitAlertMinutes;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static CondominiumSettings CreateDefault(Guid tenantId)
    {
        return new CondominiumSettings(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            visitDurationMinutes: 120,
            requireShiftHandoverNotes: true,
            defaultShiftLengthHours: 8,
            emergencyContactPhone: null,
            allowedVisitorStartHour: "06:00",
            allowedVisitorEndHour: "22:00",
            autoCheckoutAtMidnight: true,
            maxActiveVisitorsPerUnit: 5,
            photoRequiredVisitors: true,
            photoRequiredProviders: true,
            photoRequiredResidents: false,
            allowOverrideOnRefusal: true,
            overdueVisitAlertMinutes: 15,
            createdAtUtc: DateTime.UtcNow);
    }

    public void Update(
        int visitDurationMinutes,
        bool requireShiftHandoverNotes,
        int defaultShiftLengthHours,
        string? emergencyContactPhone,
        string allowedVisitorStartHour,
        string allowedVisitorEndHour,
        bool autoCheckoutAtMidnight,
        int maxActiveVisitorsPerUnit,
        bool photoRequiredVisitors,
        bool photoRequiredProviders,
        bool photoRequiredResidents,
        bool allowOverrideOnRefusal,
        int overdueVisitAlertMinutes)
    {
        VisitDurationMinutes = visitDurationMinutes;
        RequireShiftHandoverNotes = requireShiftHandoverNotes;
        DefaultShiftLengthHours = defaultShiftLengthHours;
        EmergencyContactPhone = emergencyContactPhone;
        AllowedVisitorStartHour = allowedVisitorStartHour;
        AllowedVisitorEndHour = allowedVisitorEndHour;
        AutoCheckoutAtMidnight = autoCheckoutAtMidnight;
        MaxActiveVisitorsPerUnit = maxActiveVisitorsPerUnit;
        PhotoRequiredVisitors = photoRequiredVisitors;
        PhotoRequiredProviders = photoRequiredProviders;
        PhotoRequiredResidents = photoRequiredResidents;
        AllowOverrideOnRefusal = allowOverrideOnRefusal;
        OverdueVisitAlertMinutes = overdueVisitAlertMinutes;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
