using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.Auditing;

namespace ControlEasyReborn.Modules.Administration.Application.Abstractions;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken ct);
    Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AuditLogEntry>> ListAsync(
        Guid tenantId,
        string? category,
        AuditSeverity? severity,
        string? entityType,
        string? action,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? searchTerm,
        int skip,
        int take,
        CancellationToken ct);
    Task<IReadOnlyList<AuditLogEntry>> GetByEntityAsync(Guid entityId, int skip, int take, CancellationToken ct);
}