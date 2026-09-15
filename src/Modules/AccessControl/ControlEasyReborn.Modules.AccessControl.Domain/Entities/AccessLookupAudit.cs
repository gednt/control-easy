using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

namespace ControlEasyReborn.Modules.AccessControl.Domain.Entities;

public sealed class AccessLookupAudit
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public LookupCriterionType CriterionType { get; private set; }
    public ResultCountBand ResultCountBand { get; private set; }
    public SubjectType? SelectedSubjectType { get; private set; }
    public Guid? SelectedSubjectId { get; private set; }
    public Guid PerformedByProfileId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Guid CorrelationId { get; private set; }

    private AccessLookupAudit() { }

    private AccessLookupAudit(
        Guid id,
        Guid tenantId,
        LookupCriterionType criterionType,
        ResultCountBand resultCountBand,
        SubjectType? selectedSubjectType,
        Guid? selectedSubjectId,
        Guid performedByProfileId,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        Id = id;
        TenantId = tenantId;
        CriterionType = criterionType;
        ResultCountBand = resultCountBand;
        SelectedSubjectType = selectedSubjectType;
        SelectedSubjectId = selectedSubjectId;
        PerformedByProfileId = performedByProfileId;
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = correlationId;
    }

    public static AccessLookupAudit Record(
        Guid tenantId,
        LookupCriterionType criterionType,
        ResultCountBand resultCountBand,
        SubjectType? selectedSubjectType,
        Guid? selectedSubjectId,
        Guid performedByProfileId,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        return new AccessLookupAudit(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            criterionType: criterionType,
            resultCountBand: resultCountBand,
            selectedSubjectType: selectedSubjectType,
            selectedSubjectId: selectedSubjectId,
            performedByProfileId: performedByProfileId,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId);
    }

    public static AccessLookupAudit Hydrate(
        Guid id,
        Guid tenantId,
        LookupCriterionType criterionType,
        ResultCountBand resultCountBand,
        SubjectType? selectedSubjectType,
        Guid? selectedSubjectId,
        Guid performedByProfileId,
        DateTime occurredAtUtc,
        Guid correlationId)
    {
        return new AccessLookupAudit(
            id: id,
            tenantId: tenantId,
            criterionType: criterionType,
            resultCountBand: resultCountBand,
            selectedSubjectType: selectedSubjectType,
            selectedSubjectId: selectedSubjectId,
            performedByProfileId: performedByProfileId,
            occurredAtUtc: occurredAtUtc,
            correlationId: correlationId);
    }
}
