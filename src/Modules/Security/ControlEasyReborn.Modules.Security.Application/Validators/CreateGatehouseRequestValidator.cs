using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class CreateGatehouseRequestValidator : AbstractValidator<CreateGatehouseRequest>
{
    public CreateGatehouseRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(r => r.Location)
            .MaximumLength(500)
            .When(r => !string.IsNullOrEmpty(r.Location));
    }
}