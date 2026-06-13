using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class CreateAttendantProfileRequestValidator : AbstractValidator<CreateAttendantProfileRequest>
{
    public CreateAttendantProfileRequestValidator()
    {
        RuleFor(r => r.UserId)
            .NotEmpty();

        RuleFor(r => r.Permissions)
            .NotEmpty();

        RuleFor(r => r.DisplayName)
            .MaximumLength(200)
            .When(r => !string.IsNullOrEmpty(r.DisplayName));
    }
}