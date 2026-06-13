namespace ControlEasyReborn.Modules.ServiceProviders.Application.Contracts;

public sealed record CreateServiceProviderRequest(string Name, string Document, string? Phone, string? Email, string? ServiceType, string? Company);

public sealed record UpdateServiceProviderRequest(string Name, string Document, string? Phone, string? Email, string? ServiceType, string? Company);

public sealed record ServiceProviderResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Document,
    string? Phone,
    string? Email,
    string? ServiceType,
    string? Company,
    bool Active,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);