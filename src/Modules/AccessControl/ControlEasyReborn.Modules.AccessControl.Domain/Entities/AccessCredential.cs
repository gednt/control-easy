using ControlEasyReborn.Modules.AccessControl.Domain.Errors;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Domain.Entities;

public sealed class AccessCredential
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public SubjectType SubjectType { get; private set; }
    public Guid SubjectId { get; private set; }
    public CredentialMethod Method { get; private set; }
    public byte[] SecretVerifier { get; private set; } = Array.Empty<byte>();
    public int KeyVersion { get; private set; }
    public CredentialStatus Status { get; private set; }
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public Guid? ReplacedByCredentialId { get; private set; }
    public Guid IssuedByProfileId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private AccessCredential() { }

    private AccessCredential(
        Guid id,
        Guid tenantId,
        SubjectType subjectType,
        Guid subjectId,
        CredentialMethod method,
        byte[] secretVerifier,
        int keyVersion,
        CredentialStatus status,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        Guid? replacedByCredentialId,
        Guid issuedByProfileId,
        DateTime createdAtUtc,
        DateTime? updatedAtUtc)
    {
        if (!CredentialMethodCodes.IsCreatable(method))
            throw new InvalidOperationException("Only QR credentials are creatable in this release.");
        if (secretVerifier is null || secretVerifier.Length == 0)
            throw new InvalidOperationException("Secret verifier is required.");

        Id = id;
        TenantId = tenantId;
        SubjectType = subjectType;
        SubjectId = subjectId;
        Method = method;
        SecretVerifier = secretVerifier;
        KeyVersion = keyVersion;
        Status = status;
        ValidFromUtc = validFromUtc;
        ExpiresAtUtc = expiresAtUtc;
        ReplacedByCredentialId = replacedByCredentialId;
        IssuedByProfileId = issuedByProfileId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static AccessCredential Issue(
        Guid tenantId,
        SubjectType subjectType,
        Guid subjectId,
        byte[] secretVerifier,
        int keyVersion,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        Guid issuedByProfileId,
        DateTime nowUtc)
    {
        return new AccessCredential(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            subjectType: subjectType,
            subjectId: subjectId,
            method: CredentialMethod.Qr,
            secretVerifier: secretVerifier,
            keyVersion: keyVersion,
            status: CredentialStatus.Active,
            validFromUtc: validFromUtc,
            expiresAtUtc: expiresAtUtc,
            replacedByCredentialId: null,
            issuedByProfileId: issuedByProfileId,
            createdAtUtc: nowUtc,
            updatedAtUtc: nowUtc);
    }

    public static AccessCredential Hydrate(
        Guid id,
        Guid tenantId,
        SubjectType subjectType,
        Guid subjectId,
        CredentialMethod method,
        byte[] secretVerifier,
        int keyVersion,
        CredentialStatus status,
        DateTime validFromUtc,
        DateTime? expiresAtUtc,
        Guid? replacedByCredentialId,
        Guid issuedByProfileId,
        DateTime createdAtUtc,
        DateTime? updatedAtUtc)
    {
        return new AccessCredential(
            id: id,
            tenantId: tenantId,
            subjectType: subjectType,
            subjectId: subjectId,
            method: method,
            secretVerifier: secretVerifier,
            keyVersion: keyVersion,
            status: status,
            validFromUtc: validFromUtc,
            expiresAtUtc: expiresAtUtc,
            replacedByCredentialId: replacedByCredentialId,
            issuedByProfileId: issuedByProfileId,
            createdAtUtc: createdAtUtc,
            updatedAtUtc: updatedAtUtc);
    }

    public bool IsUsableAt(DateTime nowUtc)
    {
        if (Status != CredentialStatus.Active) return false;
        if (nowUtc < ValidFromUtc) return false;
        if (ExpiresAtUtc.HasValue && nowUtc >= ExpiresAtUtc.Value) return false;
        return true;
    }

    public void Replace(Guid successorId, DateTime nowUtc)
    {
        if (Status != CredentialStatus.Active)
            throw new CredentialLifecycleConflictException("Only an active credential can be replaced.");
        Status = CredentialStatus.Replaced;
        ReplacedByCredentialId = successorId;
        UpdatedAtUtc = nowUtc;
    }

    public void Revoke(DateTime nowUtc)
    {
        if (Status == CredentialStatus.Revoked) return;
        if (Status != CredentialStatus.Active)
            throw new CredentialLifecycleConflictException("Only an active credential can be revoked.");
        Status = CredentialStatus.Revoked;
        UpdatedAtUtc = nowUtc;
    }

    public void Deactivate(DateTime nowUtc)
    {
        if (Status == CredentialStatus.Inactive) return;
        if (Status != CredentialStatus.Active)
            throw new CredentialLifecycleConflictException("Only an active credential can be deactivated.");
        Status = CredentialStatus.Inactive;
        UpdatedAtUtc = nowUtc;
    }

    public void Expire(DateTime nowUtc)
    {
        if (Status == CredentialStatus.Expired) return;
        Status = CredentialStatus.Expired;
        UpdatedAtUtc = nowUtc;
    }
}
