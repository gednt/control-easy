namespace ControlEasyReborn.Modules.Security.Domain.Entities;

public sealed class Shift
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool CrossesMidnight => StartTime > EndTime;

    private Shift() { }

    public Shift(Guid id, Guid tenantId, string name, TimeSpan startTime, TimeSpan endTime)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
    }
}