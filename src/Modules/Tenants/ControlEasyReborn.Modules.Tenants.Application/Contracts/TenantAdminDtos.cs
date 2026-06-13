namespace ControlEasyReborn.Modules.Tenants.Application.Contracts;

public sealed record CreateTenantAdminRequest(string Email, string DisplayName, string Password);

public sealed record TenantAdminResponse(Guid UserId, string Email, string DisplayName, Guid TenantId, bool Active, DateTime CreatedAtUtc);

public sealed record BackupResult(Guid TenantId, string Slug, string FilePath, long SizeBytes, DateTime CreatedAtUtc);