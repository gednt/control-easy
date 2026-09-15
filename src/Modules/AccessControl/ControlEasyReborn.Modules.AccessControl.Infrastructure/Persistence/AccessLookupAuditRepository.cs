using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.Persistence;

public sealed class AccessLookupAuditRepository : IAccessLookupAuditRepository
{
    private const string TableName = "AccessLookupAudits";
    private const string Fields = "Id, TenantId, CriterionType, ResultCountBand, SelectedSubjectType, SelectedSubjectId, PerformedByProfileId, OccurredAtUtc, CorrelationId";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public AccessLookupAuditRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<AccessLookupAudit?> FindAsync(Guid tenantId, Guid lookupAuditId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { lookupAuditId },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task AddAsync(AccessLookupAudit audit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "CriterionType", "ResultCountBand", "SelectedSubjectType", "SelectedSubjectId", "PerformedByProfileId", "OccurredAtUtc", "CorrelationId", "tenant_id" },
            TableName,
            new object?[] { audit.Id, audit.TenantId, (int)audit.CriterionType, (int)audit.ResultCountBand, audit.SelectedSubjectType.HasValue ? (int)audit.SelectedSubjectType.Value : (object)DBNull.Value, (object?)audit.SelectedSubjectId ?? DBNull.Value, audit.PerformedByProfileId, audit.OccurredAtUtc, audit.CorrelationId, audit.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    private static AccessLookupAudit? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static AccessLookupAudit? MapRow(DataRow r)
    {
        var selectedSubjectTypeStr = r["SelectedSubjectType"]?.ToString();
        var selectedSubjectIdStr = r["SelectedSubjectId"]?.ToString();

        return AccessLookupAudit.Hydrate(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            criterionType: (LookupCriterionType)Convert.ToInt32(r["CriterionType"]),
            resultCountBand: (ResultCountBand)Convert.ToInt32(r["ResultCountBand"]),
            selectedSubjectType: string.IsNullOrEmpty(selectedSubjectTypeStr) ? null : (SubjectType)Convert.ToInt32(selectedSubjectTypeStr),
            selectedSubjectId: string.IsNullOrEmpty(selectedSubjectIdStr) ? null : Guid.Parse(selectedSubjectIdStr),
            performedByProfileId: Guid.Parse(r["PerformedByProfileId"].ToString() ?? string.Empty),
            occurredAtUtc: Convert.ToDateTime(r["OccurredAtUtc"]),
            correlationId: Guid.Parse(r["CorrelationId"].ToString() ?? string.Empty));
    }
}
