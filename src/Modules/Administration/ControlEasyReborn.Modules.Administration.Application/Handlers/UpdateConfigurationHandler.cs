using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class UpdateConfigurationHandler
{
    private readonly IConfigurationRepository _configurations;
    private readonly IValidator<UpdateConfigurationRequest> _validator;

    public UpdateConfigurationHandler(IConfigurationRepository configurations, IValidator<UpdateConfigurationRequest> validator)
    {
        _configurations = configurations;
        _validator = validator;
    }

    public async Task<ConfigurationResponse> HandleAsync(Guid id, UpdateConfigurationRequest request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var entry = await _configurations.FindAsync(id, ct);
        if (entry is null)
        {
            throw new NotFoundException("Configuration " + id + " was not found.");
        }

        entry.UpdateValue(request.Value);
        await _configurations.UpdateAsync(entry, ct);
        return CreateConfigurationHandler.ToResponse(entry);
    }
}