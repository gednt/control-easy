using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Abstractions;

public interface ITenantAdminRepository
{
    Task<TenantAdminResponse> CreateAdminAsync(Guid tenantId, string email, string displayName, string passwordHash, CancellationToken ct);
    Task<IReadOnlyList<TenantAdminResponse>> ListAdminsAsync(Guid tenantId, CancellationToken ct);
    Task<TenantAdminResponse> UpdateAdminAsync(Guid tenantId, Guid userId, string email, string displayName, CancellationToken ct);
    Task RevokeAdminAsync(Guid userId, CancellationToken ct);
    Task SetAdminActiveAsync(Guid tenantId, Guid userId, bool active, CancellationToken ct);
    Task DeleteAdminAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<PorteiroResponse> CreatePorteiroAsync(Guid tenantId, string email, string displayName, string passwordHash, CancellationToken ct);
    Task<IReadOnlyList<PorteiroResponse>> ListPorteirosAsync(Guid tenantId, CancellationToken ct);
    Task UpdatePorteiroAsync(Guid tenantId, Guid userId, string email, string displayName, CancellationToken ct);
    Task SetPorteiroActiveAsync(Guid tenantId, Guid userId, bool active, CancellationToken ct);
    Task DeletePorteiroAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, Guid? exceptUserId, CancellationToken ct);
    Task EnsureAttendantProfileAsync(Guid userId, Guid tenantId, string displayName, string roles, CancellationToken ct);
}