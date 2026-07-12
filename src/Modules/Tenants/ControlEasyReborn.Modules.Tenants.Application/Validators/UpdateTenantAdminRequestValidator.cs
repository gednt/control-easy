using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Tenants.Application.Validators;

public sealed class UpdateTenantAdminRequestValidator : AbstractValidator<UpdateTenantAdminRequest>
{
    public UpdateTenantAdminRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(r => r.DisplayName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
