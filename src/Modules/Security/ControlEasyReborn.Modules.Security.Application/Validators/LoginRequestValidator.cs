using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty();
    }
}