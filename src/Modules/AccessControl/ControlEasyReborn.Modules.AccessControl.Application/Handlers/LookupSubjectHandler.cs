using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Application.Commands;
using ControlEasyReborn.Modules.AccessControl.Application.Logging;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.Modules.Apartments.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Application.Abstractions;
using ControlEasyReborn.Modules.Residents.Domain.Entities;
using ControlEasyReborn.Modules.Vehicles.Application.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ControlEasyReborn.Modules.AccessControl.Application.Handlers;

public sealed record LookupSubjectResultItem(
    string SubjectType,
    Guid SubjectId,
    Guid? ApartmentId,
    string? ApartmentBlock,
    string? ApartmentUnit,
    string? DisplayName,
    string? DocumentMasked,
    string? Plate);

public sealed record LookupSubjectResult(
    Guid LookupAuditId,
    LookupCriterionType CriterionType,
    ResultCountBand ResultCountBand,
    IReadOnlyList<LookupSubjectResultItem> Items);

public sealed class LookupSubjectHandler
{
    private const int DefaultResultCap = 25;
    private const int MinNameLength = 3;
    private const int MinBlockLength = 1;
    private const int MinApartmentLength = 1;

    private readonly IAccessLookupAuditRepository _audits;
    private readonly IAccessControlClock _clock;
    private readonly IResidentDirectory _residents;
    private readonly IVehicleDirectory _vehicles;
    private readonly IApartmentDirectory _apartments;
    private readonly ILogger<LookupSubjectHandler> _logger;

    public LookupSubjectHandler(
        IAccessLookupAuditRepository audits,
        IAccessControlClock clock,
        IResidentDirectory residents,
        IVehicleDirectory vehicles,
        IApartmentDirectory apartments,
        ILogger<LookupSubjectHandler>? logger = null)
    {
        _audits = audits;
        _clock = clock;
        _residents = residents;
        _vehicles = vehicles;
        _apartments = apartments;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LookupSubjectHandler>.Instance;
    }

    public async Task<LookupSubjectResult> HandleAsync(LookupSubjectCommand command, Guid performedByProfileId, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        using var scope = AccessControlLogContext.BeginScope(
            tenantId: command.TenantId,
            profileId: performedByProfileId,
            decision: "lookup_subject");

        var value = (command.Value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["Value"] = new[] { "Search value is required." }
            });
        }

        var items = command.Criterion switch
        {
            LookupCriterionType.Cpf => await LookupByCpfAsync(command.TenantId, value, ct),
            LookupCriterionType.IdentityDocument => await LookupByDocumentAsync(command.TenantId, value, ct),
            LookupCriterionType.Name => await LookupByNameAsync(command.TenantId, value, ct),
            LookupCriterionType.Apartment => await LookupByApartmentAsync(command.TenantId, value, command.Unit, ct),
            LookupCriterionType.Block => await LookupByBlockAsync(command.TenantId, value, ct),
            _ => throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["Criterion"] = new[] { "Unknown criterion." }
            })
        };

        var band = ResultCountBandRules.FromCount(items.Count);
        var audit = AccessLookupAudit.Record(
            tenantId: command.TenantId,
            criterionType: command.Criterion,
            resultCountBand: band,
            selectedSubjectType: null,
            selectedSubjectId: null,
            performedByProfileId: performedByProfileId,
            occurredAtUtc: _clock.UtcNow,
            correlationId: Guid.NewGuid());

        await _audits.AddAsync(audit, ct);

        stopwatch.Stop();
        using (AccessControlLogContext.PushDuration(stopwatch.ElapsedMilliseconds))
        {
            _logger.LogInformation(
                "AccessControl lookup subject criterion={Criterion} resultBand={ResultBand} auditId={LookupAuditId} elapsedMs={ElapsedMs}",
                LookupCriterionTypeCodes.ToWire(command.Criterion),
                ResultCountBandRules.ToWire(band),
                audit.Id,
                stopwatch.ElapsedMilliseconds);
        }

        return new LookupSubjectResult(
            LookupAuditId: audit.Id,
            CriterionType: command.Criterion,
            ResultCountBand: band,
            Items: items);
    }

    private async Task<IReadOnlyList<LookupSubjectResultItem>> LookupByCpfAsync(Guid tenantId, string value, CancellationToken ct)
    {
        var resident = await _residents.FindActiveByCpfAsync(tenantId, value, ct);
        if (resident is null)
        {
            return Array.Empty<LookupSubjectResultItem>();
        }
        return new[] { await MapResidentAsync(tenantId, resident, ct) };
    }

    private async Task<IReadOnlyList<LookupSubjectResultItem>> LookupByDocumentAsync(Guid tenantId, string value, CancellationToken ct)
    {
        var residents = await _residents.SearchByDocumentAsync(tenantId, "national_id", value, ct);
        if (residents.Count == 0)
        {
            return Array.Empty<LookupSubjectResultItem>();
        }
        var list = new List<LookupSubjectResultItem>(residents.Count);
        foreach (var r in residents)
        {
            list.Add(await MapResidentAsync(tenantId, r, ct));
        }
        return list;
    }

    private async Task<IReadOnlyList<LookupSubjectResultItem>> LookupByNameAsync(Guid tenantId, string value, CancellationToken ct)
    {
        if (value.Length < MinNameLength)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["Value"] = new[] { "Search too broad. Provide at least " + MinNameLength + " characters." }
            });
        }
        var residents = await _residents.SearchByNameAsync(tenantId, value, 0, DefaultResultCap, ct);
        var list = new List<LookupSubjectResultItem>(residents.Count);
        foreach (var r in residents)
        {
            list.Add(await MapResidentAsync(tenantId, r, ct));
        }
        return list;
    }

    private async Task<IReadOnlyList<LookupSubjectResultItem>> LookupByApartmentAsync(Guid tenantId, string value, string? unit, CancellationToken ct)
    {
        if (value.Length < MinApartmentLength)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["Value"] = new[] { "Search too broad. Provide an apartment number." }
            });
        }
        if (!Guid.TryParse(value, out var apartmentId))
        {
            return Array.Empty<LookupSubjectResultItem>();
        }
        var residents = await _residents.SearchByApartmentAsync(tenantId, apartmentId, 0, DefaultResultCap, ct);
        var list = new List<LookupSubjectResultItem>(residents.Count);
        foreach (var r in residents)
        {
            list.Add(await MapResidentAsync(tenantId, r, ct));
        }
        return list;
    }

    private async Task<IReadOnlyList<LookupSubjectResultItem>> LookupByBlockAsync(Guid tenantId, string value, CancellationToken ct)
    {
        if (value.Length < MinBlockLength)
        {
            throw new Application.Errors.ValidationException(new Dictionary<string, string[]>
            {
                ["Value"] = new[] { "Search too broad. Provide a block identifier." }
            });
        }
        var apartments = await _apartments.SearchByBlockAsync(tenantId, value, 0, DefaultResultCap, ct);
        var list = new List<LookupSubjectResultItem>();
        foreach (var apartment in apartments)
        {
            var residents = await _residents.SearchByApartmentAsync(tenantId, apartment.Id, 0, DefaultResultCap, ct);
            foreach (var r in residents)
            {
                list.Add(await MapResidentAsync(tenantId, r, ct));
            }
        }
        return list;
    }

    private async Task<LookupSubjectResultItem> MapResidentAsync(Guid tenantId, Resident resident, CancellationToken ct)
    {
        string? block = null;
        string? unit = null;
        if (resident.ApartmentId.HasValue && resident.ApartmentId.Value != Guid.Empty)
        {
            var apartment = await _apartments.FindActiveAsync(tenantId, resident.ApartmentId.Value, ct);
            if (apartment is not null)
            {
                block = apartment.Block;
                unit = apartment.Unit;
            }
        }
        return new LookupSubjectResultItem(
            SubjectType: "resident",
            SubjectId: resident.Id,
            ApartmentId: resident.ApartmentId,
            ApartmentBlock: block,
            ApartmentUnit: unit,
            DisplayName: resident.Name,
            DocumentMasked: MaskCpf(resident.Cpf),
            Plate: null);
    }

    private static string? MaskCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return null;
        if (cpf.Length < 4) return "***";
        return "***" + cpf[^4..];
    }
}