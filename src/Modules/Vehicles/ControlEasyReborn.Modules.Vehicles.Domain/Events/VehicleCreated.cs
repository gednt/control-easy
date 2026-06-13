namespace ControlEasyReborn.Modules.Vehicles.Domain.Events;

public sealed record VehicleCreated(Guid VehicleId, Guid TenantId, string Plate, DateTime OccurredAtUtc)
{
    public Guid VehicleId { get; } = VehicleId;
    public Guid TenantId { get; } = TenantId;
    public string Plate { get; } = Plate;
    public DateTime OccurredAtUtc { get; } = OccurredAtUtc;
}