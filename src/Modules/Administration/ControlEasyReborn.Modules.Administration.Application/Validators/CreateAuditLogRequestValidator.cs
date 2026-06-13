using ControlEasyReborn.Modules.Administration.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Validators;

public sealed class CreateAuditLogRequestValidator : AbstractValidator<CreateAuditLogRequest>
{
    public CreateAuditLogRequestValidator()
    {
        RuleFor(r => r.Action)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.EntityType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.PerformedByName)
            .MaximumLength(200)
            .When(r => !string.IsNullOrEmpty(r.PerformedByName));

        RuleFor(r => r.Details)
            .MaximumLength(4000)
            .When(r => !string.IsNullOrEmpty(r.Details));
    }
}