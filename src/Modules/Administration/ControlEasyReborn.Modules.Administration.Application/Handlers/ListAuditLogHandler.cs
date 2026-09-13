using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.SharedKernel.Auditing;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class ListAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogs;

    public ListAuditLogHandler(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> HandleAsync(
        Guid tenantId,
        string? category,
        string? severity,
        string? entityType,
        string? action,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? searchTerm,
        int skip,
        int take,
        CancellationToken ct)
    {
        AuditSeverity? parsedSeverity = null;
        if (!string.IsNullOrWhiteSpace(severity) && Enum.TryParse<AuditSeverity>(severity, true, out var s))
        {
            parsedSeverity = s;
        }

        var entries = await _auditLogs.ListAsync(tenantId, category, parsedSeverity, entityType, action, fromUtc, toUtc, searchTerm, skip, take, ct);
        return entries.Select(CreateAuditLogHandler.ToResponse).ToList();
    }
}