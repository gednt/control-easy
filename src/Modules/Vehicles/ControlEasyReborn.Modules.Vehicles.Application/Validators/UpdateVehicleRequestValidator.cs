using ControlEasyReborn.Modules.Vehicles.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Vehicles.Application.Validators;

public sealed class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleRequestValidator()
    {
        RuleFor(r => r.Plate)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(r => r.Brand)
            .MaximumLength(100)
            .When(r => !string.IsNullOrEmpty(r.Brand));

        RuleFor(r => r.Model)
            .MaximumLength(100)
            .When(r => !string.IsNullOrEmpty(r.Model));

        RuleFor(r => r.Color)
            .MaximumLength(50)
            .When(r => !string.IsNullOrEmpty(r.Color));

        RuleFor(r => r.OwnerName)
            .MaximumLength(200)
            .When(r => !string.IsNullOrEmpty(r.OwnerName));
    }
}