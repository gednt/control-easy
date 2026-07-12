using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class UpdateAttendantProfileRequestValidator : AbstractValidator<UpdateAttendantProfileRequest>
{
    public UpdateAttendantProfileRequestValidator()
    {
        RuleFor(r => r.Permissions)
            .NotEmpty();

        RuleFor(r => r.DisplayName)
            .MaximumLength(200)
            .When(r => !string.IsNullOrEmpty(r.DisplayName));
    }
}