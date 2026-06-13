namespace ControlEasyReborn.Modules.Security.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntry?> FindByTokenAsync(string token, CancellationToken ct);
    Task AddAsync(RefreshTokenEntry entry, CancellationToken ct);
    Task RevokeAsync(RefreshTokenEntry entry, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}

public sealed class RefreshTokenEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;
}