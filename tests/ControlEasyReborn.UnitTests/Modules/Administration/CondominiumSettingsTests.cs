using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using ControlEasyReborn.Modules.Administration.Application.Handlers;
using ControlEasyReborn.Modules.Administration.Application.Validators;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.Auditing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Administration;

public sealed class CondominiumSettingsTests
{
    private readonly CondominiumSettingsValidator _validator = new();

    [Fact]
    public void CreateDefault_should_set_sensible_defaults()
    {
        var tenantId = Guid.NewGuid();
        var settings = CondominiumSettings.CreateDefault(tenantId);

        settings.TenantId.Should().Be(tenantId);
        settings.VisitDurationMinutes.Should().Be(120);
        settings.RequireShiftHandoverNotes.Should().BeTrue();
        settings.DefaultShiftLengthHours.Should().Be(8);
        settings.AllowedVisitorStartHour.Should().Be("06:00");
        settings.AllowedVisitorEndHour.Should().Be("22:00");
        settings.AutoCheckoutAtMidnight.Should().BeTrue();
        settings.MaxActiveVisitorsPerUnit.Should().Be(5);
        settings.PhotoRequiredVisitors.Should().BeTrue();
        settings.PhotoRequiredProviders.Should().BeTrue();
        settings.PhotoRequiredResidents.Should().BeFalse();
        settings.AllowOverrideOnRefusal.Should().BeTrue();
        settings.OverdueVisitAlertMinutes.Should().Be(15);
    }

    [Fact]
    public void Update_should_mutate_properties_and_set_updated_timestamp()
    {
        var tenantId = Guid.NewGuid();
        var settings = CondominiumSettings.CreateDefault(tenantId);

        settings.Update(
            visitDurationMinutes: 180,
            requireShiftHandoverNotes: false,
            defaultShiftLengthHours: 12,
            emergencyContactPhone: "+55 11 98888-7777",
            allowedVisitorStartHour: "07:00",
            allowedVisitorEndHour: "21:00",
            autoCheckoutAtMidnight: false,
            maxActiveVisitorsPerUnit: 8,
            photoRequiredVisitors: false,
            photoRequiredProviders: true,
            photoRequiredResidents: true,
            allowOverrideOnRefusal: false,
            overdueVisitAlertMinutes: 30);

        settings.VisitDurationMinutes.Should().Be(180);
        settings.RequireShiftHandoverNotes.Should().BeFalse();
        settings.DefaultShiftLengthHours.Should().Be(12);
        settings.EmergencyContactPhone.Should().Be("+55 11 98888-7777");
        settings.AllowedVisitorStartHour.Should().Be("07:00");
        settings.AllowedVisitorEndHour.Should().Be("21:00");
        settings.AutoCheckoutAtMidnight.Should().BeFalse();
        settings.MaxActiveVisitorsPerUnit.Should().Be(8);
        settings.PhotoRequiredVisitors.Should().BeFalse();
        settings.PhotoRequiredProviders.Should().BeTrue();
        settings.PhotoRequiredResidents.Should().BeTrue();
        settings.AllowOverrideOnRefusal.Should().BeFalse();
        settings.OverdueVisitAlertMinutes.Should().Be(30);
        settings.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Validator_should_pass_for_valid_settings()
    {
        var request = new UpdateCondominiumSettingsRequest(
            VisitDurationMinutes: 120,
            RequireShiftHandoverNotes: true,
            DefaultShiftLengthHours: 8,
            EmergencyContactPhone: "+55 11 99999-0000",
            AllowedVisitorStartHour: "06:00",
            AllowedVisitorEndHour: "22:00",
            AutoCheckoutAtMidnight: true,
            MaxActiveVisitorsPerUnit: 5,
            PhotoRequiredVisitors: true,
            PhotoRequiredProviders: true,
            PhotoRequiredResidents: false,
            AllowOverrideOnRefusal: true,
            OverdueVisitAlertMinutes: 15);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(10)] // Under 15 min
    [InlineData(1500)] // Over 1440 min
    public void Validator_should_fail_for_out_of_range_visit_duration(int minutes)
    {
        var request = new UpdateCondominiumSettingsRequest(
            VisitDurationMinutes: minutes,
            RequireShiftHandoverNotes: true,
            DefaultShiftLengthHours: 8,
            EmergencyContactPhone: null,
            AllowedVisitorStartHour: "06:00",
            AllowedVisitorEndHour: "22:00",
            AutoCheckoutAtMidnight: true,
            MaxActiveVisitorsPerUnit: 5,
            PhotoRequiredVisitors: true,
            PhotoRequiredProviders: true,
            PhotoRequiredResidents: false,
            AllowOverrideOnRefusal: true,
            OverdueVisitAlertMinutes: 15);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.VisitDurationMinutes));
    }

    [Theory]
    [InlineData("25:00")]
    [InlineData("invalid")]
    [InlineData("6:00")]
    public void Validator_should_fail_for_invalid_time_format(string startHour)
    {
        var request = new UpdateCondominiumSettingsRequest(
            VisitDurationMinutes: 120,
            RequireShiftHandoverNotes: true,
            DefaultShiftLengthHours: 8,
            EmergencyContactPhone: null,
            AllowedVisitorStartHour: startHour,
            AllowedVisitorEndHour: "22:00",
            AutoCheckoutAtMidnight: true,
            MaxActiveVisitorsPerUnit: 5,
            PhotoRequiredVisitors: true,
            PhotoRequiredProviders: true,
            PhotoRequiredResidents: false,
            AllowOverrideOnRefusal: true,
            OverdueVisitAlertMinutes: 15);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.AllowedVisitorStartHour));
    }

    [Fact]
    public async Task GetCondominiumSettingsHandler_should_seed_defaults_when_settings_do_not_exist()
    {
        var repo = Substitute.For<ICondominiumSettingsRepository>();
        var tenantId = Guid.NewGuid();
        repo.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((CondominiumSettings?)null);

        var handler = new GetCondominiumSettingsHandler(repo);

        var result = await handler.HandleAsync(tenantId, CancellationToken.None);

        result.TenantId.Should().Be(tenantId);
        result.VisitDurationMinutes.Should().Be(120);
        await repo.Received(1).SaveAsync(Arg.Is<CondominiumSettings>(s => s.TenantId == tenantId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCondominiumSettingsHandler_should_update_settings_and_write_audit_log()
    {
        var repo = Substitute.For<ICondominiumSettingsRepository>();
        var auditWriter = Substitute.For<IAuditLogWriter>();
        var tenantId = Guid.NewGuid();
        var existing = CondominiumSettings.CreateDefault(tenantId);

        repo.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new UpdateCondominiumSettingsHandler(repo, _validator, auditWriter);

        var request = new UpdateCondominiumSettingsRequest(
            VisitDurationMinutes: 150,
            RequireShiftHandoverNotes: false,
            DefaultShiftLengthHours: 6,
            EmergencyContactPhone: "911",
            AllowedVisitorStartHour: "08:00",
            AllowedVisitorEndHour: "20:00",
            AutoCheckoutAtMidnight: false,
            MaxActiveVisitorsPerUnit: 10,
            PhotoRequiredVisitors: true,
            PhotoRequiredProviders: true,
            PhotoRequiredResidents: true,
            AllowOverrideOnRefusal: false,
            OverdueVisitAlertMinutes: 20);

        var result = await handler.HandleAsync(tenantId, request, Guid.NewGuid(), "Admin User", CancellationToken.None);

        result.VisitDurationMinutes.Should().Be(150);
        result.AllowedVisitorStartHour.Should().Be("08:00");
        await repo.Received(1).SaveAsync(Arg.Is<CondominiumSettings>(s => s.VisitDurationMinutes == 150), Arg.Any<CancellationToken>());
        await auditWriter.Received(1).WriteAsync(
            tenantId: tenantId,
            category: AuditCategory.Settings,
            action: "SettingsUpdated",
            entityType: nameof(CondominiumSettings),
            entityId: Arg.Any<Guid?>(),
            severity: AuditSeverity.Info,
            details: Arg.Is<string>(d => d.Contains("Admin User")),
            metadata: Arg.Any<object?>(),
            ct: Arg.Any<CancellationToken>());
    }
}
