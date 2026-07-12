using ControlEasyReborn.Modules.Administration.Domain.Entities;

namespace ControlEasyReborn.Modules.Administration.Application.Abstractions;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken ct);
    Task<IReadOnlyList<AuditLogEntry>> ListAsync(Guid tenantId, string? entityType, string? action, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<AuditLogEntry>> GetByEntityAsync(Guid entityId, int skip, int take, CancellationToken ct);
}