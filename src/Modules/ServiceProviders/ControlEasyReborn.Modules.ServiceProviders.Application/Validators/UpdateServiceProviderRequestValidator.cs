using ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;
using FluentValidation;

namespace ControlEasyReborn.Modules.ServiceProviders.Application.Validators;

public sealed class UpdateServiceProviderRequestValidator : AbstractValidator<UpdateServiceProviderRequest>
{
    public UpdateServiceProviderRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Document)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(r => r.Phone)
            .MaximumLength(20)
            .When(r => !string.IsNullOrEmpty(r.Phone));

        RuleFor(r => r.Email)
            .EmailAddress()
            .When(r => !string.IsNullOrEmpty(r.Email));

        RuleFor(r => r.ServiceType)
            .MaximumLength(100)
            .When(r => !string.IsNullOrEmpty(r.ServiceType));

        RuleFor(r => r.Company)
            .MaximumLength(200)
            .When(r => !string.IsNullOrEmpty(r.Company));
    }
}