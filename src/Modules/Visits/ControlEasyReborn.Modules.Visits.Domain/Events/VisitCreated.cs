namespace ControlEasyReborn.Modules.Visits.Domain.Events;

public sealed record VisitCreated(Guid VisitId, Guid TenantId, string VisitorName, DateTime OccurredAtUtc)
{
    public Guid VisitId { get; } = VisitId;
    public Guid TenantId { get; } = TenantId;
    public string VisitorName { get; } = VisitorName;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}