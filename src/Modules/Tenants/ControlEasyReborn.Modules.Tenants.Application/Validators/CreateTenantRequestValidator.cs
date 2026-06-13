using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Tenants.Application.Validators;

public sealed class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(r => r.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9](?:[a-z0-9-]{0,30}[a-z0-9])?$")
            .WithMessage("Slug must be lowercase alphanumerics or dashes, 1-32 chars, no leading/trailing dash.");
        RuleFor(r => r.DisplayName)
            .NotEmpty()
            .MaximumLength(120);
    }
}
