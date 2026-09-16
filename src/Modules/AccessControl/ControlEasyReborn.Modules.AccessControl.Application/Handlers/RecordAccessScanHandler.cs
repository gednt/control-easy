using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;

namespace ControlEasyReborn.Modules.AccessControl.Application.Handlers;

public sealed record ScanDecisionResult(
    ScanDecisionKind Decision,
    Guid? AccessEventId,
    Guid? RefusalId,
    SubjectType SubjectType,
    Guid SubjectId,
    Guid? CredentialId,
    AccessMethod AccessMethod,
    CycleDirection Direction,
    PolicyOutcome PolicyOutcome,
    Guid? DestinationApartmentId,
    string DestinationBlock,
    string DestinationUnit,
    string? FailureCode);

public sealed class RecordAccessScanHandler
{
    private readonly IAccessCredentialRepository _credentials;
    private readonly IAccessEventRepository _events;
    private readonly IRefusedScanAttemptRepository _refusals;
    private readonly IResidentDirectory _residents;
    private readonly IVehicleDirectory _vehicles;
    private readonly IApartmentDirectory _apartments;
    private readonly IConsentPolicyEvaluator _policy;
    private readonly IAccessControlCryptoService _crypto;
    private readonly IAccessControlClock _clock;
    private readonly AccessEventDestinationResolver _resolver;

    public RecordAccessScanHandler(
        IAccessCredentialRepository credentials,
        IAccessEventRepository events,
        IRefusedScanAttemptRepository refusals,
        IResidentDirectory residents,
        IVehicleDirectory vehicles,
        IApartmentDirectory apartments,
        IConsentPolicyEvaluator policy,
        IAccessControlCryptoService crypto,
        IAccessControlClock clock,
        AccessEventDestinationResolver? resolver = null)
    {
        _credentials = credentials;
        _events = events;
        _refusals = refusals;
        _residents = residents;
        _vehicles = vehicles;
        _apartments = apartments;
        _policy = policy;
        _crypto = crypto;
        _clock = clock;
        _resolver = resolver ?? new AccessEventDestinationResolver(residents, vehicles, apartments);
    }

    public async Task<ScanDecisionResult> HandleAsync(RecordAccessScanCommand command, CancellationToken ct)
    {
        var nowUtc = _clock.UtcNow;

        var existing = await _events.FindByScanAttemptAsync(command.TenantId, command.ScanAttemptId, ct);
        if (existing is not null)
        {
            return new ScanDecisionResult(
                Decision: ScanDecisionKind.Recorded,
                AccessEventId: existing.Id,
                RefusalId: null,
                SubjectType: existing.SubjectType,
                SubjectId: existing.SubjectId,
                CredentialId: existing.CredentialId,
                AccessMethod: existing.AccessMethod,
                Direction: existing.Direction,
                PolicyOutcome: existing.PolicyOutcome,
                DestinationApartmentId: existing.DestinationApartmentId,
                DestinationBlock: existing.DestinationBlock,
                DestinationUnit: existing.DestinationUnit,
                FailureCode: null);
        }

        var credential = await _crypto.ResolveByTokenAsync(command.TenantId, command.QrPayload, ct);
        if (credential is null)
        {
            return await RecordRefusalAsync(command, ct,
                relatedCredentialId: null,
                relatedSubjectId: null,
                failureCode: RefusalCodes.InvalidCredential,
                decisionKind: ScanDecisionKind.Refused);
        }

        if (credential.Status != CredentialStatus.Active || !credential.IsUsableAt(nowUtc))
        {
            return await RecordRefusalAsync(command, ct,
                relatedCredentialId: credential.Id,
                relatedSubjectId: credential.SubjectId,
                failureCode: RefusalCodes.CredentialInactive,
                decisionKind: ScanDecisionKind.Refused);
        }

        var destination = await _resolver.ResolveAsync(command.TenantId, credential.SubjectType, credential.SubjectId, ct);
        if (!destination.Resolved)
        {
            return await RecordRefusalAsync(command, ct,
                relatedCredentialId: credential.Id,
                relatedSubjectId: credential.SubjectId,
                failureCode: destination.FailureCode ?? RefusalCodes.NotAuthorized,
                decisionKind: ScanDecisionKind.Refused);
        }

        var policyOutcome = await _policy.EvaluateAsync(SubjectTypeCodes.ToWire(credential.SubjectType), credential.SubjectId, "scan", ct);
        var policyFinal = policyOutcome switch
        {
            ConsentOutcome.Permitted => PolicyOutcome.Permit,
            ConsentOutcome.RequiresAction => PolicyOutcome.RequiresAction,
            ConsentOutcome.Refused => PolicyOutcome.Refused,
            _ => PolicyOutcome.Refused
        };

        if (policyFinal == PolicyOutcome.RequiresAction)
        {
            return await RecordRefusalAsync(command, ct,
                relatedCredentialId: credential.Id,
                relatedSubjectId: credential.SubjectId,
                failureCode: RefusalCodes.PolicyActionRequired,
                decisionKind: ScanDecisionKind.PolicyActionRequired,
                policy: policyFinal);
        }

        if (policyFinal == PolicyOutcome.Refused)
        {
            return await RecordRefusalAsync(command, ct,
                relatedCredentialId: credential.Id,
                relatedSubjectId: credential.SubjectId,
                failureCode: RefusalCodes.NotAuthorized,
                decisionKind: ScanDecisionKind.Refused,
                policy: policyFinal);
        }

        var accessEvent = AccessEvent.Record(
            tenantId: command.TenantId,
            subjectType: credential.SubjectType,
            subjectId: credential.SubjectId,
            direction: command.Direction,
            accessMethod: AccessMethod.Qr,
            credentialId: credential.Id,
            lookupAuditId: null,
            scanAttemptId: command.ScanAttemptId,
            performedByProfileId: command.PerformedByProfileId,
            gatehouseId: command.GatehouseId,
            occurredAtUtc: nowUtc,
            correlationId: Guid.NewGuid(),
            duplicateOfAccessEventId: null,
            duplicateConfirmed: false,
            policyOutcome: policyFinal,
            destinationApartmentId: destination.ApartmentId!.Value,
            destinationBlock: destination.Block,
            destinationUnit: destination.Unit);

        await _events.AddAsync(accessEvent, ct);

        return new ScanDecisionResult(
            Decision: ScanDecisionKind.Recorded,
            AccessEventId: accessEvent.Id,
            RefusalId: null,
            SubjectType: accessEvent.SubjectType,
            SubjectId: accessEvent.SubjectId,
            CredentialId: accessEvent.CredentialId,
            AccessMethod: accessEvent.AccessMethod,
            Direction: accessEvent.Direction,
            PolicyOutcome: accessEvent.PolicyOutcome,
            DestinationApartmentId: accessEvent.DestinationApartmentId,
            DestinationBlock: accessEvent.DestinationBlock,
            DestinationUnit: accessEvent.DestinationUnit,
            FailureCode: null);
    }

    private async Task<ScanDecisionResult> RecordRefusalAsync(
        RecordAccessScanCommand command,
        CancellationToken ct,
        Guid? relatedCredentialId,
        Guid? relatedSubjectId,
        string failureCode,
        ScanDecisionKind decisionKind,
        PolicyOutcome policy = PolicyOutcome.Refused)
    {
        var nowUtc = _clock.UtcNow;
        var attempt = RefusedScanAttempt.Record(
            tenantId: command.TenantId,
            scanAttemptId: command.ScanAttemptId,
            credentialFingerprint: null,
            direction: command.Direction,
            failureCode: failureCode,
            performedByProfileId: command.PerformedByProfileId,
            gatehouseId: command.GatehouseId,
            occurredAtUtc: nowUtc,
            correlationId: Guid.NewGuid(),
            relatedCredentialId: relatedCredentialId,
            relatedSubjectId: relatedSubjectId);

        await _refusals.AddAsync(attempt, ct);

        return new ScanDecisionResult(
            Decision: decisionKind,
            AccessEventId: null,
            RefusalId: attempt.Id,
            SubjectType: default,
            SubjectId: Guid.Empty,
            CredentialId: relatedCredentialId,
            AccessMethod: AccessMethod.Qr,
            Direction: command.Direction,
            PolicyOutcome: policy,
            DestinationApartmentId: null,
            DestinationBlock: string.Empty,
            DestinationUnit: string.Empty,
            FailureCode: failureCode);
    }
}