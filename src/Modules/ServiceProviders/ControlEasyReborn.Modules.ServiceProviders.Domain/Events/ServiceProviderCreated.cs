namespace ControlEasyReborn.Modules.ServiceProviders.Domain.Events;

public sealed record ServiceProviderCreated(Guid ServiceProviderId, Guid TenantId, string Name, DateTime OccurredAtUtc)
{
    public Guid ServiceProviderId { get; } = ServiceProviderId;
    public Guid TenantId { get; } = TenantId;
    public string Name { get; } = Name;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}