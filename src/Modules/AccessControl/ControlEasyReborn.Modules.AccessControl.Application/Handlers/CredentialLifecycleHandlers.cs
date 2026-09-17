using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Application.Errors;
using ControlEasyReborn.Modules.AccessControl.Application.Logging;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.Errors;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ControlEasyReborn.Modules.AccessControl.Application.Handlers;

public sealed record IssueCredentialResult(Guid CredentialId, string QrPayload);

public sealed record ReplaceCredentialResult(Guid NewCredentialId, string NewQrPayload);

public sealed record RevokeCredentialResult(Guid CredentialId, LifecycleAction Action, CredentialStatus ResultingStatus);

public sealed class IssueCredentialHandler
{
    private readonly IAccessCredentialRepository _credentials;
    private readonly ICredentialLifecycleActionRepository _lifecycle;
    private readonly IOpaqueTokenIssuer _tokens;
    private readonly AccessControlHmacKeyProvider _hmacKeyProvider;
    private readonly IAccessControlClock _clock;
    private readonly ILogger<IssueCredentialHandler> _logger;

    public IssueCredentialHandler(
        IAccessCredentialRepository credentials,
        ICredentialLifecycleActionRepository lifecycle,
        IOpaqueTokenIssuer tokens,
        AccessControlHmacKeyProvider hmacKeyProvider,
        IAccessControlClock clock,
        ILogger<IssueCredentialHandler>? logger = null)
    {
        _credentials = credentials;
        _lifecycle = lifecycle;
        _tokens = tokens;
        _hmacKeyProvider = hmacKeyProvider;
        _clock = clock;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<IssueCredentialHandler>.Instance;
    }

    public async Task<IssueCredentialResult> HandleAsync(IssueCredentialCommand command, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = AccessControlLogContext.BeginScope(
            tenantId: command.TenantId,
            profileId: command.IssuedByProfileId,
            decision: "credential_issue",
            subjectType: SubjectTypeCodes.ToWire(command.SubjectType));

        var nowUtc = _clock.UtcNow;

        var existingActive = await _credentials.FindActiveAsync(command.TenantId, command.SubjectType, command.SubjectId, ct);
        if (existingActive is not null)
        {
            throw new CredentialAlreadyActiveException(
                $"An active credential already exists for subject '{command.SubjectType}:{command.SubjectId}'.");
        }

        var issued = _tokens.Issue(_hmacKeyProvider.Key);

        var credential = AccessCredential.Issue(
            tenantId: command.TenantId,
            subjectType: command.SubjectType,
            subjectId: command.SubjectId,
            secretVerifier: issued.Verifier,
            keyVersion: issued.KeyVersion,
            validFromUtc: command.ValidFromUtc <= nowUtc ? nowUtc : command.ValidFromUtc,
            expiresAtUtc: command.ExpiresAtUtc,
            issuedByProfileId: command.IssuedByProfileId,
            nowUtc: nowUtc);

        await _credentials.AddAsync(credential, ct);

        var lifecycle = CredentialLifecycleAction.Record(
            tenantId: command.TenantId,
            credentialId: credential.Id,
            action: LifecycleAction.Issued,
            previousStatus: CredentialStatus.Active,
            resultingStatus: CredentialStatus.Active,
            actorProfileId: command.IssuedByProfileId,
            reasonCode: null,
            reasonText: null,
            occurredAtUtc: nowUtc,
            correlationId: Guid.NewGuid());
        await _lifecycle.AddAsync(lifecycle, ct);

        stopwatch.Stop();
        using (AccessControlLogContext.PushDuration(stopwatch.ElapsedMilliseconds))
        {
            _logger.LogInformation(
                "AccessControl credential issued credentialId={CredentialId} subjectId={SubjectId} elapsedMs={ElapsedMs}",
                credential.Id,
                command.SubjectId,
                stopwatch.ElapsedMilliseconds);
        }

        return new IssueCredentialResult(credential.Id, issued.Token);
    }
}

public sealed class ReplaceCredentialHandler
{
    private readonly IAccessCredentialRepository _credentials;
    private readonly ICredentialLifecycleActionRepository _lifecycle;
    private readonly IOpaqueTokenIssuer _tokens;
    private readonly AccessControlHmacKeyProvider _hmacKeyProvider;
    private readonly IAccessControlClock _clock;
    private readonly ILogger<ReplaceCredentialHandler> _logger;

    public ReplaceCredentialHandler(
        IAccessCredentialRepository credentials,
        ICredentialLifecycleActionRepository lifecycle,
        IOpaqueTokenIssuer tokens,
        AccessControlHmacKeyProvider hmacKeyProvider,
        IAccessControlClock clock,
        ILogger<ReplaceCredentialHandler>? logger = null)
    {
        _credentials = credentials;
        _lifecycle = lifecycle;
        _tokens = tokens;
        _hmacKeyProvider = hmacKeyProvider;
        _clock = clock;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ReplaceCredentialHandler>.Instance;
    }

    public async Task<ReplaceCredentialResult> HandleAsync(ReplaceCredentialCommand command, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = AccessControlLogContext.BeginScope(
            tenantId: command.TenantId,
            profileId: command.IssuedByProfileId,
            decision: "credential_replace");

        var nowUtc = _clock.UtcNow;

        var predecessor = await _credentials.FindByIdAsync(command.TenantId, command.CredentialId, ct);
        if (predecessor is null)
        {
            throw new NotFoundException($"Credential '{command.CredentialId}' was not found.");
        }
        if (predecessor.Status != CredentialStatus.Active)
        {
            throw new Domain.Errors.CredentialLifecycleConflictException("Only an active credential can be replaced.");
        }

        var issued = _tokens.Issue(_hmacKeyProvider.Key);

        var successor = AccessCredential.Issue(
            tenantId: command.TenantId,
            subjectType: predecessor.SubjectType,
            subjectId: predecessor.SubjectId,
            secretVerifier: issued.Verifier,
            keyVersion: issued.KeyVersion,
            validFromUtc: command.ValidFromUtc <= nowUtc ? nowUtc : command.ValidFromUtc,
            expiresAtUtc: command.ExpiresAtUtc,
            issuedByProfileId: command.IssuedByProfileId,
            nowUtc: nowUtc);

        var previousStatus = predecessor.Status;
        predecessor.Replace(successor.Id, nowUtc);

        var lifecycle = CredentialLifecycleAction.Record(
            tenantId: command.TenantId,
            credentialId: predecessor.Id,
            action: LifecycleAction.Replaced,
            previousStatus: previousStatus,
            resultingStatus: predecessor.Status,
            actorProfileId: command.IssuedByProfileId,
            reasonCode: null,
            reasonText: null,
            occurredAtUtc: nowUtc,
            correlationId: Guid.NewGuid());

        await _credentials.ReplaceAsync(predecessor, successor, lifecycle, ct);

        stopwatch.Stop();
        using (AccessControlLogContext.PushDuration(stopwatch.ElapsedMilliseconds))
        {
            _logger.LogInformation(
                "AccessControl credential replaced predecessorId={PredecessorId} successorId={SuccessorId} subjectId={SubjectId} elapsedMs={ElapsedMs}",
                predecessor.Id,
                successor.Id,
                predecessor.SubjectId,
                stopwatch.ElapsedMilliseconds);
        }

        return new ReplaceCredentialResult(successor.Id, issued.Token);
    }
}

public sealed class RevokeCredentialHandler
{
    private readonly IAccessCredentialRepository _credentials;
    private readonly ICredentialLifecycleActionRepository _lifecycle;
    private readonly IAccessControlClock _clock;
    private readonly ILogger<RevokeCredentialHandler> _logger;

    public RevokeCredentialHandler(
        IAccessCredentialRepository credentials,
        ICredentialLifecycleActionRepository lifecycle,
        IAccessControlClock clock,
        ILogger<RevokeCredentialHandler>? logger = null)
    {
        _credentials = credentials;
        _lifecycle = lifecycle;
        _clock = clock;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RevokeCredentialHandler>.Instance;
    }

    public async Task<RevokeCredentialResult> HandleAsync(RevokeCredentialCommand command, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = AccessControlLogContext.BeginScope(
            tenantId: command.TenantId,
            profileId: command.ActorProfileId,
            decision: "credential_revoke");

        var nowUtc = _clock.UtcNow;

        var credential = await _credentials.FindByIdAsync(command.TenantId, command.CredentialId, ct);
        if (credential is null)
        {
            throw new NotFoundException($"Credential '{command.CredentialId}' was not found.");
        }
        if (credential.Status == CredentialStatus.Revoked)
        {
            stopwatch.Stop();
            using (AccessControlLogContext.PushDuration(stopwatch.ElapsedMilliseconds))
            {
                _logger.LogInformation(
                    "AccessControl credential revoke idempotent credentialId={CredentialId} elapsedMs={ElapsedMs}",
                    credential.Id,
                    stopwatch.ElapsedMilliseconds);
            }
            return new RevokeCredentialResult(credential.Id, LifecycleAction.Revoked, CredentialStatus.Revoked);
        }
        if (credential.Status != CredentialStatus.Active)
        {
            throw new Domain.Errors.CredentialLifecycleConflictException("Only an active credential can be revoked.");
        }

        var previousStatus = credential.Status;
        credential.Revoke(nowUtc);
        await _credentials.UpdateStatusAsync(credential, ct);

        var lifecycle = CredentialLifecycleAction.Record(
            tenantId: command.TenantId,
            credentialId: credential.Id,
            action: LifecycleAction.Revoked,
            previousStatus: previousStatus,
            resultingStatus: credential.Status,
            actorProfileId: command.ActorProfileId,
            reasonCode: command.ReasonCode,
            reasonText: command.ReasonText,
            occurredAtUtc: nowUtc,
            correlationId: Guid.NewGuid());
        await _lifecycle.AddAsync(lifecycle, ct);

        stopwatch.Stop();
        using (AccessControlLogContext.PushDuration(stopwatch.ElapsedMilliseconds))
        {
            _logger.LogInformation(
                "AccessControl credential revoked credentialId={CredentialId} subjectId={SubjectId} elapsedMs={ElapsedMs}",
                credential.Id,
                credential.SubjectId,
                stopwatch.ElapsedMilliseconds);
        }

        return new RevokeCredentialResult(credential.Id, LifecycleAction.Revoked, credential.Status);
    }
}