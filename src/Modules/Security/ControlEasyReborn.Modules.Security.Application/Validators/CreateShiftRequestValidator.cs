using ControlEasyReborn.Modules.Security.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class CreateShiftRequestValidator : AbstractValidator<CreateShiftRequest>
{
    public CreateShiftRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(120);
    }
}