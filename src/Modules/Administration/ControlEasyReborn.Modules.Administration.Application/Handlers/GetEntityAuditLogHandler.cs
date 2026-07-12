using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class GetEntityAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogs;

    public GetEntityAuditLogHandler(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> HandleAsync(Guid entityId, int skip, int take, CancellationToken ct)
    {
        var entries = await _auditLogs.GetByEntityAsync(entityId, skip, take, ct);
        return entries.Select(CreateAuditLogHandler.ToResponse).ToList();
    }
}