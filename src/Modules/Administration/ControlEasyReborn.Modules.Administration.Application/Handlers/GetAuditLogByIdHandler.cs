using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class GetAuditLogByIdHandler
{
    private readonly IAuditLogRepository _auditLogs;

    public GetAuditLogByIdHandler(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<AuditLogResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var entry = await _auditLogs.GetByIdAsync(id, ct);
        if (entry is null)
        {
            throw new NotFoundException($"Audit log entry '{id}' was not found.");
        }

        return CreateAuditLogHandler.ToResponse(entry);
    }
}
