using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.Errors;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.AccessControl.Entities;

public sealed class AccessCredentialTests
{
    [Fact]
    public void Issue_creates_an_active_QR_credential_that_is_usable_in_its_valid_window()
    {
        var now = DateTime.UtcNow;

        var credential = Issue(now);

        credential.Status.Should().Be(CredentialStatus.Active);
        credential.Method.Should().Be(CredentialMethod.Qr);
        credential.IsUsableAt(now).Should().BeTrue();
        credential.IsUsableAt(now.AddMinutes(-1)).Should().BeFalse();
    }

    [Fact]
    public void Replace_marks_active_credential_replaced_and_records_successor()
    {
        var now = DateTime.UtcNow;
        var credential = Issue(now);
        var successorId = Guid.NewGuid();

        credential.Replace(successorId, now.AddMinutes(1));

        credential.Status.Should().Be(CredentialStatus.Replaced);
        credential.ReplacedByCredentialId.Should().Be(successorId);
        credential.IsUsableAt(now.AddMinutes(1)).Should().BeFalse();
    }

    [Theory]
    [InlineData(CredentialStatus.Inactive)]
    [InlineData(CredentialStatus.Replaced)]
    public void Revoke_rejects_non_active_non_revoked_lifecycle_states(CredentialStatus priorState)
    {
        var now = DateTime.UtcNow;
        var credential = AccessCredential.Hydrate(
            Guid.NewGuid(), Guid.NewGuid(), SubjectType.Resident, Guid.NewGuid(), CredentialMethod.Qr,
            new byte[] { 1 }, 1, priorState, now.AddHours(-1), null, null, Guid.NewGuid(), now.AddHours(-1), null);

        var act = () => credential.Revoke(now);

        act.Should().Throw<CredentialLifecycleConflictException>();
    }

    [Fact]
    public void Revoke_is_idempotent_after_first_revocation()
    {
        var now = DateTime.UtcNow;
        var credential = Issue(now);

        credential.Revoke(now);
        var act = () => credential.Revoke(now.AddMinutes(1));

        act.Should().NotThrow();
        credential.Status.Should().Be(CredentialStatus.Revoked);
    }

    private static AccessCredential Issue(DateTime now) => AccessCredential.Issue(
        Guid.NewGuid(),
        SubjectType.Resident,
        Guid.NewGuid(),
        new byte[] { 1, 2, 3 },
        1,
        now,
        now.AddHours(1),
        Guid.NewGuid(),
        now);
}
