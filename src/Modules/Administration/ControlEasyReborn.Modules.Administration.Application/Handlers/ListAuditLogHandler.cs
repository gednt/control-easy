using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class ListAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogs;

    public ListAuditLogHandler(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> HandleAsync(Guid tenantId, string? entityType, string? action, int skip, int take, CancellationToken ct)
    {
        var entries = await _auditLogs.ListAsync(tenantId, entityType, action, skip, take, ct);
        return entries.Select(CreateAuditLogHandler.ToResponse).ToList();
    }
}