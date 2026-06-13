using ControlEasyReborn.SharedKernel.MultiTenancy;

namespace ControlEasyReborn.Infrastructure.MultiTenancy;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly object _gate = new();
    private Guid? _tenantId;
    private Guid? _profileId;
    private IReadOnlyCollection<string> _roles = Array.Empty<string>();
    private IReadOnlyCollection<string> _permissions = Array.Empty<string>();
    private bool _resolved;

    public Guid? TenantId { get { lock (_gate) return _tenantId; } }
    public Guid? ProfileId { get { lock (_gate) return _profileId; } }
    public IReadOnlyCollection<string> Roles { get { lock (_gate) return _roles; } }
    public IReadOnlyCollection<string> Permissions { get { lock (_gate) return _permissions; } }
    public bool IsPlatformAdmin => Roles.Contains("PlatformAdmin");
    public bool IsResolved { get { lock (_gate) return _resolved; } }

    public void Set(Guid? tenantId, Guid? profileId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        lock (_gate)
        {
            _tenantId = tenantId;
            _profileId = profileId;
            _roles = roles;
            _permissions = permissions;
            _resolved = true;
        }
    }

    internal void Reset()
    {
        lock (_gate)
        {
            _tenantId = null;
            _profileId = null;
            _roles = Array.Empty<string>();
            _permissions = Array.Empty<string>();
            _resolved = false;
        }
    }
}
