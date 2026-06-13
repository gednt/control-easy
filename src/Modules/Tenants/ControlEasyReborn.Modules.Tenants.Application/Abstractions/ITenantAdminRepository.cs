using ControlEasyReborn.Modules.Tenants.Application.Contracts;

namespace ControlEasyReborn.Modules.Tenants.Application.Abstractions;

public interface ITenantAdminRepository
{
    Task<TenantAdminResponse> CreateAdminAsync(Guid tenantId, string email, string displayName, string passwordHash, CancellationToken ct);
    Task<IReadOnlyList<TenantAdminResponse>> ListAdminsAsync(Guid tenantId, CancellationToken ct);
    Task RevokeAdminAsync(Guid userId, CancellationToken ct);
}