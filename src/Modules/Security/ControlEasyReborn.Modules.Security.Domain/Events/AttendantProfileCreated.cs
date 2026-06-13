namespace ControlEasyReborn.Modules.Security.Domain.Events;

public sealed record AttendantProfileCreated(Guid ProfileId, Guid TenantId, Guid UserId, DateTime OccurredAtUtc)
{
    public Guid ProfileId { get; } = ProfileId;
    public Guid TenantId { get; } = TenantId;
    public Guid UserId { get; } = UserId;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}