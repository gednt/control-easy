using ControlEasyReborn.SharedKernel.Auditing;

namespace ControlEasyReborn.Modules.Administration.Domain.Entities;

public sealed class AuditLogEntry
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Category { get; private set; } = AuditCategory.System;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public AuditSeverity Severity { get; private set; } = AuditSeverity.Info;
    public Guid PerformedByUserId { get; private set; }
    public string? PerformedByName { get; private set; }
    public string? Details { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuditLogEntry() { }

    public AuditLogEntry(
        Guid id,
        Guid tenantId,
        string action,
        string entityType,
        Guid entityId,
        Guid performedByUserId,
        string? performedByName,
        string? details,
        DateTime createdAtUtc,
        string category = AuditCategory.System,
        AuditSeverity severity = AuditSeverity.Info,
        string? metadataJson = null)
    {
        Id = id;
        TenantId = tenantId;
        Category = string.IsNullOrWhiteSpace(category) ? AuditCategory.System : category;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Severity = severity;
        PerformedByUserId = performedByUserId;
        PerformedByName = performedByName;
        Details = details;
        MetadataJson = metadataJson;
        CreatedAtUtc = createdAtUtc;
    }
}