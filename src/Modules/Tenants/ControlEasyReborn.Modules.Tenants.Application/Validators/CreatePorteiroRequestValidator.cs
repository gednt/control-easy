using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Tenants.Application.Validators;

public sealed class CreatePorteiroRequestValidator : AbstractValidator<CreatePorteiroRequest>
{
    public CreatePorteiroRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(r => r.DisplayName)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}
