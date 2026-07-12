using ControlEasyReborn.Modules.Visits.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Visits.Application.Validators;

public sealed class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        RuleFor(r => r.VisitorName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.VisitorDocument)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(r => r.VisitorPhone)
            .MaximumLength(20)
            .When(r => !string.IsNullOrEmpty(r.VisitorPhone));

        RuleFor(r => r.Purpose)
            .MaximumLength(500)
            .When(r => !string.IsNullOrEmpty(r.Purpose));
    }
}