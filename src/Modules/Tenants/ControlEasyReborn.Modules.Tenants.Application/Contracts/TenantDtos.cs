namespace ControlEasyReborn.Modules.Tenants.Application.Contracts;

public sealed record CreateTenantRequest(string Slug, string DisplayName);

public sealed record TenantResponse(Guid Id, string Slug, string DisplayName, string Status, DateTime CreatedAtUtc);

public sealed record SuspendTenantRequest(string? Reason);

public sealed record ResumeTenantRequest(string? Reason);
