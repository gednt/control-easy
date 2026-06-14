using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty();

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .NotEqual(r => r.CurrentPassword)
            .WithMessage("New password must differ from the current password.");
    }
}
