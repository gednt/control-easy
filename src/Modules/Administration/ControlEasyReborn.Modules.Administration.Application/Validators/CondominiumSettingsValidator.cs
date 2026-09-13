using ControlEasyReborn.Modules.Administration.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Validators;

public sealed class CondominiumSettingsValidator : AbstractValidator<UpdateCondominiumSettingsRequest>
{
    public CondominiumSettingsValidator()
    {
        RuleFor(r => r.VisitDurationMinutes)
            .InclusiveBetween(15, 1440)
            .WithMessage("Visit duration must be between 15 and 1440 minutes.");

        RuleFor(r => r.DefaultShiftLengthHours)
            .InclusiveBetween(1, 24)
            .WithMessage("Shift length must be between 1 and 24 hours.");

        RuleFor(r => r.MaxActiveVisitorsPerUnit)
            .InclusiveBetween(1, 100)
            .WithMessage("Max active visitors per unit must be between 1 and 100.");

        RuleFor(r => r.OverdueVisitAlertMinutes)
            .InclusiveBetween(0, 180)
            .WithMessage("Overdue alert threshold must be between 0 and 180 minutes.");

        RuleFor(r => r.AllowedVisitorStartHour)
            .NotEmpty()
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("Start hour must be formatted as HH:mm (e.g. 06:00).");

        RuleFor(r => r.AllowedVisitorEndHour)
            .NotEmpty()
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("End hour must be formatted as HH:mm (e.g. 22:00).");

        RuleFor(r => r.EmergencyContactPhone)
            .MaximumLength(50)
            .When(r => !string.IsNullOrEmpty(r.EmergencyContactPhone));
    }
}
