using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Application.Contracts;
using ControlEasyReborn.Modules.Administration.Application.Errors;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using FluentValidation;

namespace ControlEasyReborn.Modules.Administration.Application.Handlers;

public sealed class CreateAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogs;
    private readonly IValidator<CreateAuditLogRequest> _validator;

    public CreateAuditLogHandler(IAuditLogRepository auditLogs, IValidator<CreateAuditLogRequest> validator)
    {
        _auditLogs = auditLogs;
        _validator = validator;
    }

    public async Task<AuditLogResponse> HandleAsync(CreateAuditLogRequest request, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var entry = new AuditLogEntry(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            action: request.Action,
            entityType: request.EntityType,
            entityId: request.EntityId,
            performedByUserId: request.PerformedByUserId,
            performedByName: request.PerformedByName,
            details: request.Details,
            createdAtUtc: DateTime.UtcNow);

        await _auditLogs.AddAsync(entry, ct);
        return ToResponse(entry);
    }

    internal static AuditLogResponse ToResponse(AuditLogEntry e) =>
        new(e.Id, e.TenantId, e.Action, e.EntityType, e.EntityId, e.PerformedByUserId, e.PerformedByName, e.Details, e.CreatedAtUtc);
}