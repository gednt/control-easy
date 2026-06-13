namespace ControlEasyReborn.Modules.Administration.Domain.Entities;

public sealed class AuditLogEntry
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public string? PerformedByName { get; private set; }
    public string? Details { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuditLogEntry() { }

    public AuditLogEntry(Guid id, Guid tenantId, string action, string entityType, Guid entityId, Guid performedByUserId, string? performedByName, string? details, DateTime createdAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        PerformedByUserId = performedByUserId;
        PerformedByName = performedByName;
        Details = details;
        CreatedAtUtc = createdAtUtc;
    }
}