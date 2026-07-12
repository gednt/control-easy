using ControlEasyReborn.Modules.Administration.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Validators;

public sealed class CreateConfigurationRequestValidator : AbstractValidator<CreateConfigurationRequest>
{
    public CreateConfigurationRequestValidator()
    {
        RuleFor(r => r.Key)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Value)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(r => r.Description)
            .MaximumLength(500)
            .When(r => !string.IsNullOrEmpty(r.Description));
    }
}