namespace ControlEasyReborn.Modules.Administration.Domain.Events;

public sealed record ConfigurationUpdated(Guid ConfigurationId, Guid TenantId, string Key, DateTime OccurredAtUtc)
{
    public Guid ConfigurationId { get; } = ConfigurationId;
    public Guid TenantId { get; } = TenantId;
    public string Key { get; } = Key;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}