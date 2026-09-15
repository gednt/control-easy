using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IAccessLookupAuditRepository
{
    Task<AccessLookupAudit?> FindAsync(Guid tenantId, Guid lookupAuditId, CancellationToken ct);
    Task AddAsync(AccessLookupAudit audit, CancellationToken ct);
}
