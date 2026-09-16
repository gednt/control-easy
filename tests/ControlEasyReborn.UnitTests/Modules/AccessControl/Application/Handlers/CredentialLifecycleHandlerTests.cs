using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Application.Handlers;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.Errors;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.Modules.AccessControl.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.AccessControl.Application.Handlers;

public sealed class CredentialLifecycleHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _actorProfileId = Guid.NewGuid();
    private readonly IAccessCredentialRepository _credentials = Substitute.For<IAccessCredentialRepository>();
    private readonly ICredentialLifecycleActionRepository _lifecycle = Substitute.For<ICredentialLifecycleActionRepository>();
    private readonly IOpaqueTokenIssuer _tokens = Substitute.For<IOpaqueTokenIssuer>();
    private readonly AccessControlHmacKeyProvider _hmacKeyProvider = new();

    private sealed class StubClock : IAccessControlClock
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }

    private IssueCredentialHandler BuildIssuer(StubClock? clock = null) =>
        new(_credentials, _lifecycle, _tokens, _hmacKeyProvider, clock ?? new StubClock());

    private ReplaceCredentialHandler BuildReplacer(StubClock? clock = null) =>
        new(_credentials, _lifecycle, _tokens, _hmacKeyProvider, clock ?? new StubClock());

    private RevokeCredentialHandler BuildRevoker(StubClock? clock = null) =>
        new(_credentials, _lifecycle, clock ?? new StubClock());

    [Fact]
    public async Task Issue_throws_when_active_credential_already_exists_for_subject()
    {
        var residentId = Guid.NewGuid();
        _credentials.FindActiveAsync(_tenantId, SubjectType.Resident, residentId, Arg.Any<CancellationToken>())
            .Returns(BuildCredential(SubjectType.Resident, residentId, CredentialStatus.Active));

        var handler = BuildIssuer();
        var cmd = new IssueCredentialCommand(_tenantId, SubjectType.Resident, residentId, DateTime.UtcNow, null, _actorProfileId);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<CredentialAlreadyActiveException>();
    }

    [Fact]
    public async Task Issue_persists_credential_and_writes_lifecycle_action_when_no_active_exists()
    {
        var residentId = Guid.NewGuid();
        _credentials.FindActiveAsync(_tenantId, SubjectType.Resident, residentId, Arg.Any<CancellationToken>())
            .Returns((AccessCredential?)null);
        var token = "token-abc";
        var verifier = new byte[] { 9, 9, 9 };
        _tokens.Issue(Arg.Any<byte[]>())
            .Returns(new IssuedToken(token, verifier, 1, new byte[] { 1, 1, 1 }));

        var handler = BuildIssuer();
        var cmd = new IssueCredentialCommand(_tenantId, SubjectType.Resident, residentId, DateTime.UtcNow, null, _actorProfileId);
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.QrPayload.Should().Be(token);
        result.CredentialId.Should().NotBe(Guid.Empty);
        await _credentials.Received(1).AddAsync(Arg.Is<AccessCredential>(c => c.SubjectId == residentId && c.Status == CredentialStatus.Active), Arg.Any<CancellationToken>());
        await _lifecycle.Received(1).AddAsync(Arg.Is<CredentialLifecycleAction>(a => a.Action == LifecycleAction.Issued), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Replace_replaces_predecessor_and_persists_lifecycle_action()
    {
        var residentId = Guid.NewGuid();
        var predecessor = BuildCredential(SubjectType.Resident, residentId, CredentialStatus.Active);
        _credentials.FindByIdAsync(_tenantId, predecessor.Id, Arg.Any<CancellationToken>()).Returns(predecessor);
        _tokens.Issue(Arg.Any<byte[]>()).Returns(new IssuedToken("new-token", new byte[] { 1, 1, 1 }, 1, new byte[] { 2, 2, 2 }));

        var handler = BuildReplacer();
        var cmd = new ReplaceCredentialCommand(_tenantId, predecessor.Id, DateTime.UtcNow, null, _actorProfileId);
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.NewCredentialId.Should().NotBe(Guid.Empty);
        result.NewQrPayload.Should().Be("new-token");
        predecessor.Status.Should().Be(CredentialStatus.Replaced);
        await _credentials.Received(1).ReplaceAsync(predecessor, Arg.Is<AccessCredential>(c => c.Id == result.NewCredentialId), Arg.Any<CredentialLifecycleAction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Replace_throws_when_predecessor_not_active()
    {
        var residentId = Guid.NewGuid();
        var predecessor = BuildCredential(SubjectType.Resident, residentId, CredentialStatus.Revoked);
        _credentials.FindByIdAsync(_tenantId, predecessor.Id, Arg.Any<CancellationToken>()).Returns(predecessor);

        var handler = BuildReplacer();
        var cmd = new ReplaceCredentialCommand(_tenantId, predecessor.Id, DateTime.UtcNow, null, _actorProfileId);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<CredentialLifecycleConflictException>();
    }

    [Fact]
    public async Task Revoke_marks_credential_revoked_and_records_lifecycle_action_with_reason()
    {
        var residentId = Guid.NewGuid();
        var credential = BuildCredential(SubjectType.Resident, residentId, CredentialStatus.Active);
        _credentials.FindByIdAsync(_tenantId, credential.Id, Arg.Any<CancellationToken>()).Returns(credential);

        var handler = BuildRevoker();
        var cmd = new RevokeCredentialCommand(_tenantId, credential.Id, "lost", "Lost device", _actorProfileId);
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.ResultingStatus.Should().Be(CredentialStatus.Revoked);
        credential.Status.Should().Be(CredentialStatus.Revoked);
        await _credentials.Received(1).UpdateStatusAsync(credential, Arg.Any<CancellationToken>());
        await _lifecycle.Received(1).AddAsync(Arg.Is<CredentialLifecycleAction>(a => a.Action == LifecycleAction.Revoked && a.ReasonCode == "lost"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_is_idempotent_when_already_revoked()
    {
        var residentId = Guid.NewGuid();
        var credential = BuildCredential(SubjectType.Resident, residentId, CredentialStatus.Revoked);
        _credentials.FindByIdAsync(_tenantId, credential.Id, Arg.Any<CancellationToken>()).Returns(credential);

        var handler = BuildRevoker();
        var cmd = new RevokeCredentialCommand(_tenantId, credential.Id, null, null, _actorProfileId);
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.ResultingStatus.Should().Be(CredentialStatus.Revoked);
        await _credentials.DidNotReceive().UpdateStatusAsync(Arg.Any<AccessCredential>(), Arg.Any<CancellationToken>());
        await _lifecycle.DidNotReceive().AddAsync(Arg.Any<CredentialLifecycleAction>(), Arg.Any<CancellationToken>());
    }

    private static AccessCredential BuildCredential(SubjectType subjectType, Guid subjectId, CredentialStatus status)
    {
        return AccessCredential.Hydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            subjectType,
            subjectId,
            CredentialMethod.Qr,
            new byte[] { 1, 2, 3 },
            keyVersion: 1,
            status: status,
            validFromUtc: DateTime.UtcNow.AddMinutes(-5),
            expiresAtUtc: null,
            replacedByCredentialId: null,
            issuedByProfileId: Guid.NewGuid(),
            createdAtUtc: DateTime.UtcNow.AddMinutes(-5),
            updatedAtUtc: null);
    }
}