namespace ControlEasyReborn.Modules.Security.Domain.Events;

public sealed record UserCreated(Guid UserId, string Email, string DisplayName, DateTime OccurredAtUtc)
{
    public Guid UserId { get; } = UserId;
    public string Email { get; } = Email;
    public string DisplayName { get; } = DisplayName;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}