using ControlEasyReborn.Modules.Administration.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Validators;

public sealed class UpdateConfigurationRequestValidator : AbstractValidator<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequestValidator()
    {
        RuleFor(r => r.Value)
            .NotEmpty()
            .MaximumLength(2000);
    }
}