using FluentValidation;
using ControlEasyReborn.Modules.Security.Application.Contracts;

namespace ControlEasyReborn.Modules.Security.Application.Validators;

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty();
    }
}