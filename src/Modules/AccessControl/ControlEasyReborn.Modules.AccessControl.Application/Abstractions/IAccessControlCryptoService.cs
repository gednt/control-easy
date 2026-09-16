using ControlEasyReborn.Modules.AccessControl.Domain.Entities;

namespace ControlEasyReborn.Modules.AccessControl.Application.Abstractions;

public interface IAccessControlCryptoService
{
    Task<AccessCredential?> ResolveByTokenAsync(Guid tenantId, string qrPayload, CancellationToken ct);
}