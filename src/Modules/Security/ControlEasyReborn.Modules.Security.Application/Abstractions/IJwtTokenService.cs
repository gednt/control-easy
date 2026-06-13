namespace ControlEasyReborn.Modules.Security.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, Guid tenantId, Guid profileId, string roles, string permissions);
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken ct);
}