using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;

namespace ControlEasyReborn.Modules.AccessControl.Application.Handlers;

public sealed record ManualAccessResult(
    Guid AccessEventId,
    Guid LookupAuditId,
    SubjectType SubjectType,
    Guid SubjectId,
    AccessMethod AccessMethod,
    CycleDirection Direction,
    PolicyOutcome PolicyOutcome,
    Guid DestinationApartmentId,
    string DestinationBlock,
    string DestinationUnit);

public sealed class RecordManualAccessHandler
{
    private readonly IAccessLookupAuditRepository _audits;
    private readonly IAccessEventRepository _events;
    private readonly IAccessControlClock _clock;
    private readonly IConsentPolicyEvaluator _policy;
    private readonly AccessEventDestinationResolver _resolver;

    public RecordManualAccessHandler(
        IAccessLookupAuditRepository audits,
        IAccessEventRepository events,
        IAccessControlClock clock,
        IConsentPolicyEvaluator policy,
        AccessEventDestinationResolver resolver)
    {
        _audits = audits;
        _events = events;
        _clock = clock;
        _policy = policy;
        _resolver = resolver;
    }

    public async Task<ManualAccessResult> HandleAsync(RecordManualAccessCommand command, CancellationToken ct)
    {
        var nowUtc = _clock.UtcNow;

        var audit = await _audits.FindAsync(command.TenantId, command.LookupAuditId, ct);
        if (audit is null || audit.PerformedByProfileId != command.PerformedByProfileId)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["LookupAuditId"] = new[] { RefusalCodes.ManualEventOrphanLookupId }
            });
        }

        var destination = await _resolver.ResolveAsync(command.TenantId, command.SubjectType, command.SubjectId, ct);
        if (!destination.Resolved)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["SubjectId"] = new[] { destination.FailureCode ?? RefusalCodes.NotAuthorized }
            });
        }

        var policyOutcome = await _policy.EvaluateAsync(SubjectTypeCodes.ToWire(command.SubjectType), command.SubjectId, "manual", ct);
        var policyFinal = policyOutcome switch
        {
            ConsentOutcome.Permitted => PolicyOutcome.Permit,
            ConsentOutcome.RequiresAction => PolicyOutcome.RequiresAction,
            ConsentOutcome.Refused => PolicyOutcome.Refused,
            _ => PolicyOutcome.Refused
        };

        if (policyFinal != PolicyOutcome.Permit)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["SubjectId"] = new[] { RefusalCodes.NotAuthorized }
            });
        }

        var accessEvent = AccessEvent.Record(
            tenantId: command.TenantId,
            subjectType: command.SubjectType,
            subjectId: command.SubjectId,
            direction: command.Direction,
            accessMethod: AccessMethod.ManualLookup,
            credentialId: null,
            lookupAuditId: command.LookupAuditId,
            scanAttemptId: Guid.NewGuid(),
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

        return new ManualAccessResult(
            AccessEventId: accessEvent.Id,
            LookupAuditId: command.LookupAuditId,
            SubjectType: command.SubjectType,
            SubjectId: command.SubjectId,
            AccessMethod: AccessMethod.ManualLookup,
            Direction: command.Direction,
            PolicyOutcome: policyFinal,
            DestinationApartmentId: accessEvent.DestinationApartmentId,
            DestinationBlock: accessEvent.DestinationBlock,
            DestinationUnit: accessEvent.DestinationUnit);
    }
}