namespace ControlEasyReborn.Modules.Residents.Domain.Events;

public sealed record ResidentCreated(Guid ResidentId, Guid TenantId, string Name, DateTime OccurredAtUtc)
{
    public Guid ResidentId { get; } = ResidentId;
    public Guid TenantId { get; } = TenantId;
    public string Name { get; } = Name;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}