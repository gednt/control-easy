namespace ControlEasyReborn.Modules.Administration.Domain.Events;

public sealed record AuditLogCreated(Guid AuditLogId, Guid TenantId, string Action, DateTime OccurredAtUtc)
{
    public Guid AuditLogId { get; } = AuditLogId;
    public Guid TenantId { get; } = TenantId;
    public string Action { get; } = Action;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}