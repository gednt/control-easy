using ControlEasyReborn.Modules.Residents.Domain.Entities;

namespace ControlEasyReborn.Modules.Residents.Application.Abstractions;

public interface IResidentDirectory
{
    Task<Resident?> FindActiveByCpfAsync(Guid tenantId, string normalizedCpf, CancellationToken ct);
    Task<Resident?> FindByIdAsync(Guid tenantId, Guid residentId, CancellationToken ct);
    Task<IReadOnlyList<Resident>> SearchByNameAsync(Guid tenantId, string nameTerm, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<Resident>> SearchByApartmentAsync(Guid tenantId, Guid apartmentId, int skip, int take, CancellationToken ct);
    Task<IReadOnlyList<Resident>> SearchByDocumentAsync(Guid tenantId, string documentType, string normalizedValue, CancellationToken ct);
    Task<IReadOnlyList<ResidentIdentityDocument>> GetActiveIdentityDocumentsAsync(Guid tenantId, Guid residentId, CancellationToken ct);
}
