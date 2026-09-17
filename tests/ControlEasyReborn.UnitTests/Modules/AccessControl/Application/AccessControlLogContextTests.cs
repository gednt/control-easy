using ControlEasyReborn.Modules.AccessControl.Application.Logging;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.AccessControl.Application;

public sealed class AccessControlLogContextTests
{
    [Theory]
    [InlineData("QrPayload", true)]
    [InlineData("qr", true)]
    [InlineData("Token", true)]
    [InlineData("RawPayload", true)]
    [InlineData("Document", true)]
    [InlineData("DocumentNumber", true)]
    [InlineData("DocumentValue", true)]
    [InlineData("Cpf", true)]
    [InlineData("FullName", true)]
    [InlineData("Plate", true)]
    [InlineData("FACIAL_BIOMETRIC", false)]
    [InlineData("TenantId", false)]
    [InlineData("ProfileId", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSensitivePropertyName_blocks_reserved_payload_and_pii_field_names(string? name, bool expected)
    {
        AccessControlLogContext.IsSensitivePropertyName(name).Should().Be(expected);
    }

    [Fact]
    public void BeginScope_returns_disposable_that_does_not_throw_when_no_optionals_passed()
    {
        using var scope = AccessControlLogContext.BeginScope();
        scope.Should().NotBeNull();
    }

    [Fact]
    public void BeginScope_with_all_optionals_returns_composite_disposable_that_disposes_cleanly()
    {
        var scope = AccessControlLogContext.BeginScope(
            tenantId: Guid.NewGuid(),
            profileId: Guid.NewGuid(),
            gatehouseId: Guid.NewGuid(),
            scanAttemptId: Guid.NewGuid(),
            lookupAuditId: Guid.NewGuid(),
            decision: "scan",
            credentialMethod: "qr",
            subjectType: "resident");

        scope.Should().NotBeNull();
        var act = () => scope.Dispose();
        act.Should().NotThrow();
        var secondDispose = () => scope.Dispose();
        secondDispose.Should().NotThrow();
    }

    [Fact]
    public void PushDuration_returns_property_disposable()
    {
        using var prop = AccessControlLogContext.PushDuration(42);
        prop.Should().NotBeNull();
    }
}
