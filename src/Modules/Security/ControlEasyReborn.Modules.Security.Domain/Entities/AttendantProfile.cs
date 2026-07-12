namespace ControlEasyReborn.Modules.Security.Domain.Entities;

public sealed class AttendantProfile
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public Guid? ShiftId { get; private set; }
    public Guid? GatehouseId { get; private set; }
    public string Permissions { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private AttendantProfile() { }

    public AttendantProfile(Guid id, Guid tenantId, Guid userId, string? displayName, Guid? shiftId, Guid? gatehouseId, string permissions, bool active, DateTime createdAtUtc, DateTime? updatedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        DisplayName = displayName;
        ShiftId = shiftId;
        GatehouseId = gatehouseId;
        Permissions = permissions;
        Active = active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string? displayName, Guid? shiftId, Guid? gatehouseId, string permissions)
    {
        DisplayName = displayName;
        ShiftId = shiftId;
        GatehouseId = gatehouseId;
        Permissions = permissions;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}