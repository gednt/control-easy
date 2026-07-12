using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class CreateConfigurationHandler
{
    private readonly IConfigurationRepository _configurations;
    private readonly IValidator<CreateConfigurationRequest> _validator;

    public CreateConfigurationHandler(IConfigurationRepository configurations, IValidator<CreateConfigurationRequest> validator)
    {
        _configurations = configurations;
        _validator = validator;
    }

    public async Task<ConfigurationResponse> HandleAsync(CreateConfigurationRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var entry = new ConfigurationEntry(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            key: request.Key,
            value: request.Value,
            description: request.Description,
            createdAtUtc: DateTime.UtcNow);

        await _configurations.AddAsync(entry, ct);
        return ToResponse(entry);
    }

    internal static ConfigurationResponse ToResponse(ConfigurationEntry c) =>
        new(c.Id, c.TenantId, c.Key, c.Value, c.Description, c.CreatedAtUtc, c.UpdatedAtUtc);
}