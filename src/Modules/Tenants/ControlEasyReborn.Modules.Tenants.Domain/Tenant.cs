namespace ControlEasyReborn.Modules.Tenants.Domain;

public enum TenantStatus
{
    Active = 0,
    Suspended = 1
}

// `Tenants` is a `Platform*` root aggregate. Per the C.6 architecture rule
// (review.md positive observation) it intentionally has no `TenantId`
// property: the `tenant_id` global filter does not apply to this table.
// Repositories must call `TenantAwareLinqFactory.Create(..., bypassTenantFilter: true)`.
public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Tenant() { }

    public Tenant(Guid id, string slug, string displayName, TenantStatus status, DateTime createdAtUtc)
    {
        Id = id;
        Slug = slug;
        DisplayName = displayName;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public void Suspend() => Status = TenantStatus.Suspended;
    public void Resume() => Status = TenantStatus.Active;
}
