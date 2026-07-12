namespace ControlEasyReborn.Modules.Apartments.Domain.Entities;

public sealed class Apartment
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Block { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Apartment() { }

    public Apartment(Guid id, Guid tenantId, string block, string unit, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        Block = block;
        Unit = unit;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string block, string unit, bool active)
    {
        Block = block;
        Unit = unit;
        Active = active;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
