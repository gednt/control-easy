namespace ControlEasyReborn.SharedKernel.Auditing;

public interface IAuditLogWriter
{
    Task WriteAsync(
        Guid tenantId,
        string category,
        string action,
        string entityType,
        Guid? entityId,
        AuditSeverity severity,
        string? details,
        object? metadata = null,
        CancellationToken ct = default);
}
