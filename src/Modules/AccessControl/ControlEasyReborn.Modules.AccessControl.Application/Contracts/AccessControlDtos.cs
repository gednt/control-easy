namespace ControlEasyReborn.Modules.AccessControl.Application.Contracts;

public sealed record RecordAccessScanRequest(
    string QrPayload,
    string Direction,
    Guid ScanAttemptId,
    Guid? GatehouseId,
    bool ConfirmDuplicate);

public sealed record ScanResponse(
    string Decision,
    Guid AccessEventId,
    string SubjectType,
    Guid SubjectId,
    Guid? CredentialId,
    Guid? LookupAuditId,
    string AccessMethod,
    string Direction,
    string PolicyOutcome,
    Guid DestinationApartmentId,
    string DestinationBlock,
    string DestinationUnit);

public sealed record AccessCredentialResponse(
    Guid Id,
    Guid TenantId,
    string SubjectType,
    Guid SubjectId,
    string Method,
    string Status,
    DateTime ValidFromUtc,
    DateTime? ExpiresAtUtc,
    Guid? ReplacedByCredentialId,
    Guid IssuedByProfileId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string? OneTimeQrPayload = null);