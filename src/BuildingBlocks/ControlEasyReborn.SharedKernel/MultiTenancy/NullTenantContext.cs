namespace ControlEasyReborn.SharedKernel.MultiTenancy;

public sealed class NullTenantContext : ITenantContext
{
    public static readonly NullTenantContext Instance = new();

    public Guid? TenantId => null;
    public Guid? ProfileId => null;
    public IReadOnlyCollection<string> Roles => Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
    public bool IsPlatformAdmin => false;
    public bool IsResolved => false;

    public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        throw new InvalidOperationException("NullTenantContext.Set must not be called; it is for unit tests only.");
    }
}
