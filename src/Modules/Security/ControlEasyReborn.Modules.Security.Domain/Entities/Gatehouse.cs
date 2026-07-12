namespace ControlEasyReborn.Modules.Security.Domain.Entities;

public sealed class Gatehouse
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Location { get; private set; }

    private Gatehouse() { }

    public Gatehouse(Guid id, Guid tenantId, string name, string? location)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Location = location;
    }
}