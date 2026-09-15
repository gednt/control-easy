using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IRefusedScanAttemptRepository
{
    Task<RefusedScanAttempt?> FindByScanAttemptAsync(Guid tenantId, Guid scanAttemptId, CancellationToken ct);
    Task<IReadOnlyList<RefusedScanAttempt>> ListAsync(Guid tenantId, DateTime? fromUtc, DateTime? toUtc, CycleDirection? direction, string? failureCode, Guid? attendantProfileId, int skip, int take, CancellationToken ct);
    Task AddAsync(RefusedScanAttempt attempt, CancellationToken ct);
}
