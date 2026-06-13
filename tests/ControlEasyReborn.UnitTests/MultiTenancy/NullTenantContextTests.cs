using ControlEasyReborn.SharedKernel.MultiTenancy;
using Xunit;

namespace ControlEasyReborn.UnitTests.MultiTenancy;

public sealed class NullTenantContextTests
{
    [Fact]
    public void Instance_is_singleton()
    {
        Assert.Same(NullTenantContext.Instance, NullTenantContext.Instance);
    }

    [Fact]
    public void Defaults_are_safe()
    {
        var c = NullTenantContext.Instance;
        Assert.Null(c.TenantId);
        Assert.Null(c.ProfileId);
        Assert.False(c.IsPlatformAdmin);
        Assert.False(c.IsResolved);
        Assert.Empty(c.Roles);
        Assert.Empty(c.Permissions);
    }

    [Fact]
    public void Set_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            NullTenantContext.Instance.Set(
                Guid.NewGuid(), null, new[] { "TenantAdmin" }, Array.Empty<string>()));
    }
}
