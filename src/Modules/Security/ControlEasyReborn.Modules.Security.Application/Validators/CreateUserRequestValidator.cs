using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(r => r.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Roles)
            .NotEmpty();

        RuleFor(r => r.TenantId)
            .NotEmpty();
    }
}