using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.Auditing;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class UpdateCondominiumSettingsHandler
{
    private readonly ICondominiumSettingsRepository _repository;
    private readonly IValidator<UpdateCondominiumSettingsRequest> _validator;
    private readonly IAuditLogWriter _auditWriter;

    public UpdateCondominiumSettingsHandler(
        ICondominiumSettingsRepository repository,
        IValidator<UpdateCondominiumSettingsRequest> validator,
        IAuditLogWriter auditWriter)
    {
        _repository = repository;
        _validator = validator;
        _auditWriter = auditWriter;
    }

    public async Task<CondominiumSettingsResponse> HandleAsync(
        Guid tenantId,
        UpdateCondominiumSettingsRequest request,
        Guid? performedByUserId = null,
        string? performedByName = null,
        CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ControlEasyReborn.Modules.Administration.Application.Errors.ValidationException(errors);
        }

        var settings = await _repository.GetByTenantIdAsync(tenantId, ct);
        if (settings is null)
        {
            settings = CondominiumSettings.CreateDefault(tenantId);
        }

        settings.Update(
            visitDurationMinutes: request.VisitDurationMinutes,
            requireShiftHandoverNotes: request.RequireShiftHandoverNotes,
            defaultShiftLengthHours: request.DefaultShiftLengthHours,
            emergencyContactPhone: request.EmergencyContactPhone,
            allowedVisitorStartHour: request.AllowedVisitorStartHour,
            allowedVisitorEndHour: request.AllowedVisitorEndHour,
            autoCheckoutAtMidnight: request.AutoCheckoutAtMidnight,
            maxActiveVisitorsPerUnit: request.MaxActiveVisitorsPerUnit,
            photoRequiredVisitors: request.PhotoRequiredVisitors,
            photoRequiredProviders: request.PhotoRequiredProviders,
            photoRequiredResidents: request.PhotoRequiredResidents,
            allowOverrideOnRefusal: request.AllowOverrideOnRefusal,
            overdueVisitAlertMinutes: request.OverdueVisitAlertMinutes);

        await _repository.SaveAsync(settings, ct);

        await _auditWriter.WriteAsync(
            tenantId: tenantId,
            category: AuditCategory.Settings,
            action: "SettingsUpdated",
            entityType: nameof(CondominiumSettings),
            entityId: settings.Id,
            severity: AuditSeverity.Info,
            details: $"Condominium settings updated by {performedByName ?? "Administrator"}.",
            metadata: new
            {
                request.VisitDurationMinutes,
                request.DefaultShiftLengthHours,
                request.AllowedVisitorStartHour,
                request.AllowedVisitorEndHour,
                request.AutoCheckoutAtMidnight,
                request.MaxActiveVisitorsPerUnit,
                request.PhotoRequiredVisitors,
                request.PhotoRequiredProviders,
                request.PhotoRequiredResidents,
                request.AllowOverrideOnRefusal,
                request.OverdueVisitAlertMinutes
            },
            ct: ct);

        return new CondominiumSettingsResponse(
            settings.Id,
            settings.TenantId,
            settings.VisitDurationMinutes,
            settings.RequireShiftHandoverNotes,
            settings.DefaultShiftLengthHours,
            settings.EmergencyContactPhone,
            settings.AllowedVisitorStartHour,
            settings.AllowedVisitorEndHour,
            settings.AutoCheckoutAtMidnight,
            settings.MaxActiveVisitorsPerUnit,
            settings.PhotoRequiredVisitors,
            settings.PhotoRequiredProviders,
            settings.PhotoRequiredResidents,
            settings.AllowOverrideOnRefusal,
            settings.OverdueVisitAlertMinutes,
            settings.CreatedAtUtc,
            settings.UpdatedAtUtc);
    }
}
