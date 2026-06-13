using ControlEasyReborn.Modules.Visits.Domain.Entities;

namespace ControlEasyReborn.Modules.Visits.Application.Abstractions;

public interface IVisitRepository
{
    Task<Visit?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Visit>> ListAsync(Guid tenantId, string? status, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<Visit>> ListOpenAsync(Guid tenantId, CancellationToken ct);
    Task AddAsync(Visit visit, CancellationToken ct);
    Task UpdateAsync(Visit visit, CancellationToken ct);
}