namespace ControlEasyReborn.SharedKernel.MultiTenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? ProfileId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool IsPlatformAdmin { get; }
    bool IsResolved { get; }
    void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
}
