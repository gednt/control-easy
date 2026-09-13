using System.Text.Json;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.Auditing;

namespace ControlEasyReborn.Modules.Administration.Infrastructure.Services;

public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly IAuditLogRepository _repository;

    public AuditLogWriter(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task WriteAsync(
        Guid tenantId,
        string category,
        string action,
        string entityType,
        Guid? entityId,
        AuditSeverity severity,
        string? details,
        object? metadata = null,
        CancellationToken ct = default)
    {
        string? metadataJson = null;
        if (metadata is not null)
        {
            try
            {
                metadataJson = metadata is string s ? s : JsonSerializer.Serialize(metadata);
            }
            catch
            {
                metadataJson = metadata.ToString();
            }
        }

        var entry = new AuditLogEntry(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            action: action,
            entityType: entityType,
            entityId: entityId ?? Guid.Empty,
            performedByUserId: Guid.Empty,
            performedByName: null,
            details: details,
            createdAtUtc: DateTime.UtcNow,
            category: category,
            severity: severity,
            metadataJson: metadataJson);

        await _repository.AddAsync(entry, ct);
    }
}
