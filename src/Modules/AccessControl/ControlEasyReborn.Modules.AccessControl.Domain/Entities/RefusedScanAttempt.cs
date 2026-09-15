using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Domain.Entities;

public sealed class RefusedScanAttempt
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ScanAttemptId { get; private set; }
    public byte[]? CredentialFingerprint { get; private set; }
    public CycleDirection? Direction { get; private set; }
    public string FailureCode { get; private set; } = string.Empty;
    public Guid PerformedByProfileId { get; private set; }
    public Guid? GatehouseId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid? RelatedCredentialId { get; private set; }
    public Guid? RelatedSubjectId { get; private set; }

    private RefusedScanAttempt() { }

    private RefusedScanAttempt(
        Guid id,
        Guid tenantId,
        Guid scanAttemptId,
        byte[]? credentialFingerprint,
        CycleDirection? direction,
        string failureCode,
        Guid performedByProfileId,
        Guid? gatehouseId,
        DateTime occurredAtUtc,
        Guid correlationId,
        Guid? relatedCredentialId,
        Guid? relatedSubjectId)
    {
        if (string.IsNullOrWhiteSpace(failureCode)) throw new InvalidOperationException("Failure code is required.");
        if (!RefusalCodes.All.Contains(failureCode)) throw new InvalidOperationException("Unknown failure code: " + failureCode);

        Id = id;
        TenantId = tenantId;
        ScanAttemptId = scanAttemptId;
        CredentialFingerprint = credentialFingerprint;
        Direction = direction;
        FailureCode = failureCode;
        PerformedByProfileId = performedByProfileId;
        GatehouseId = gatehouseId;
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = correlationId;
        RelatedCredentialId = relatedCredentialId;
        RelatedSubjectId = relatedSubjectId;
    }

    public static RefusedScanAttempt Record(
        Guid tenantId,
        Guid scanAttemptId,
        byte[]? credentialFingerprint,
        CycleDirection? direction,
        string failureCode,
        Guid performedByProfileId,
        Guid? gatehouseId,
        DateTime occurredAtUtc,
        Guid correlationId,
        Guid? relatedCredentialId,
        Guid? relatedSubjectId)
    {
        return new RefusedScanAttempt(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            scanAttemptId: scanAttemptId,
            credentialFingerprint: credentialFingerprint,
            direction: direction,
            failureCode: failureCode,
            performedByProfileId: performedByProfileId,
            gatehouseId: gatehouseId,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId,
            relatedCredentialId: relatedCredentialId,
            relatedSubjectId: relatedSubjectId);
    }

    public static RefusedScanAttempt Hydrate(
        Guid id,
        Guid tenantId,
        Guid scanAttemptId,
        byte[]? credentialFingerprint,
        CycleDirection? direction,
        string failureCode,
        Guid performedByProfileId,
        Guid? gatehouseId,
        DateTime occurredAtUtc,
        Guid correlationId,
        Guid? relatedCredentialId,
        Guid? relatedSubjectId)
    {
        return new RefusedScanAttempt(
            id: id,
            tenantId: tenantId,
            scanAttemptId: scanAttemptId,
            credentialFingerprint: credentialFingerprint,
            direction: direction,
            failureCode: failureCode,
            performedByProfileId: performedByProfileId,
            gatehouseId: gatehouseId,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId,
            relatedCredentialId: relatedCredentialId,
            relatedSubjectId: relatedSubjectId);
    }
}
