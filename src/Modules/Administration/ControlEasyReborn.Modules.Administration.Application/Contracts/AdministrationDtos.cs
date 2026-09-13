namespace ControlEasyReborn.Modules.Administration.Application.Contracts;

public sealed record CreateAuditLogRequest(
    string Action,
    string EntityType,
    Guid EntityId,
    Guid PerformedByUserId,
    string? PerformedByName,
    string? Details,
    string? Category = null,
    string? Severity = null,
    string? MetadataJson = null);

public sealed record AuditLogResponse(
    Guid Id,
    Guid TenantId,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid PerformedByUserId,
    string? PerformedByName,
    string? Details,
    DateTime CreatedAtUtc,
    string Category = "System",
    string Severity = "Info",
    string? MetadataJson = null);

public sealed record CreateConfigurationRequest(
    string Key,
    string Value,
    string? Description);

public sealed record UpdateConfigurationRequest(
    string Value);

public sealed record ConfigurationResponse(
    Guid Id,
    Guid TenantId,
    string Key,
    string Value,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);