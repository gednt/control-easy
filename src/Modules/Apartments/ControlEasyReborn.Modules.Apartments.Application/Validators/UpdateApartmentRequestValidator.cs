using ControlEasyReborn.Modules.Apartments.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Apartments.Application.Validators;

public sealed class UpdateApartmentRequestValidator : AbstractValidator<UpdateApartmentRequest>
{
    public UpdateApartmentRequestValidator()
    {
        RuleFor(r => r.Block)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(r => r.Unit)
            .NotEmpty()
            .MaximumLength(50);
    }
}
