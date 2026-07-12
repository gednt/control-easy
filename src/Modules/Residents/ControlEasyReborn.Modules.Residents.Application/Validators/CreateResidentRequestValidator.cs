using ControlEasyReborn.Modules.Residents.Application.Contracts;
using ControlEasyReborn.Modules.Residents.Domain.ValueObjects;
using FluentValidation;

namespace ControlEasyReborn.Modules.Residents.Application.Validators;

public sealed class CreateResidentRequestValidator : AbstractValidator<CreateResidentRequest>
{
    public CreateResidentRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Cpf)
            .NotEmpty()
            .Must(Cpf.IsValid)
            .WithMessage("CPF is invalid.");

        RuleFor(r => r.Email)
            .EmailAddress()
            .When(r => !string.IsNullOrEmpty(r.Email));

        RuleFor(r => r.Phone)
            .MaximumLength(20)
            .When(r => !string.IsNullOrEmpty(r.Phone));
    }
}