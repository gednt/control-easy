using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Domain.Entities;

public sealed class CredentialLifecycleAction
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CredentialId { get; private set; }
    public LifecycleAction Action { get; private set; }
    public CredentialStatus PreviousStatus { get; private set; }
    public CredentialStatus ResultingStatus { get; private set; }
    public Guid ActorProfileId { get; private set; }
    public string? ReasonCode { get; private set; }
    public string? ReasonText { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid CorrelationId { get; private set; }

    private CredentialLifecycleAction() { }

    private CredentialLifecycleAction(
        Guid id,
        Guid tenantId,
        Guid credentialId,
        LifecycleAction action,
        CredentialStatus previousStatus,
        CredentialStatus resultingStatus,
        Guid actorProfileId,
        string? reasonCode,
        string? reasonText,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        Id = id;
        TenantId = tenantId;
        CredentialId = credentialId;
        Action = action;
        PreviousStatus = previousStatus;
        ResultingStatus = resultingStatus;
        ActorProfileId = actorProfileId;
        ReasonCode = reasonCode;
        ReasonText = reasonText;
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = correlationId;
    }

    public static CredentialLifecycleAction Record(
        Guid tenantId,
        Guid credentialId,
        LifecycleAction action,
        CredentialStatus previousStatus,
        CredentialStatus resultingStatus,
        Guid actorProfileId,
        string? reasonCode,
        string? reasonText,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        if (reasonText is { Length: > 500 })
            throw new InvalidOperationException("Reason text exceeds 500 characters.");
        if (reasonCode is { Length: > 64 })
            throw new InvalidOperationException("Reason code exceeds 64 characters.");

        return new CredentialLifecycleAction(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            credentialId: credentialId,
            action: action,
            previousStatus: previousStatus,
            resultingStatus: resultingStatus,
            actorProfileId: actorProfileId,
            reasonCode: reasonCode,
            reasonText: reasonText,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId);
    }

    public static CredentialLifecycleAction Hydrate(
        Guid id,
        Guid tenantId,
        Guid credentialId,
        LifecycleAction action,
        CredentialStatus previousStatus,
        CredentialStatus resultingStatus,
        Guid actorProfileId,
        string? reasonCode,
        string? reasonText,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        return new CredentialLifecycleAction(
            id: id,
            tenantId: tenantId,
            credentialId: credentialId,
            action: action,
            previousStatus: previousStatus,
            resultingStatus: resultingStatus,
            actorProfileId: actorProfileId,
            reasonCode: reasonCode,
            reasonText: reasonText,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId);
    }
}
