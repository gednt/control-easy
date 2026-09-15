using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.Persistence;

public sealed class CredentialLifecycleActionRepository : ICredentialLifecycleActionRepository
{
    private const string TableName = "CredentialLifecycleActions";
    private const string Fields = "Id, TenantId, CredentialId, Action, PreviousStatus, ResultingStatus, ActorProfileId, ReasonCode, ReasonText, OccurredAtUtc, CorrelationId";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public CredentialLifecycleActionRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task AddAsync(CredentialLifecycleAction action, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "CredentialId", "Action", "PreviousStatus", "ResultingStatus", "ActorProfileId", "ReasonCode", "ReasonText", "OccurredAtUtc", "CorrelationId", "tenant_id" },
            TableName,
            new object?[] { action.Id, action.TenantId, action.CredentialId, (int)action.Action, (int)action.PreviousStatus, (int)action.ResultingStatus, action.ActorProfileId, (object?)action.ReasonCode ?? DBNull.Value, (object?)action.ReasonText ?? DBNull.Value, action.OccurredAtUtc, action.CorrelationId, action.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task<IReadOnlyList<CredentialLifecycleAction>> ListAsync(Guid tenantId, Guid credentialId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "CredentialId = @param0",
            parameters: new object[] { credentialId },
            ct: ct);
        return MapList(rows).OrderBy(a => a.OccurredAtUtc).ToList();
    }

    private static IReadOnlyList<CredentialLifecycleAction> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<CredentialLifecycleAction>();
        var list = new List<CredentialLifecycleAction>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static CredentialLifecycleAction? MapRow(DataRow r)
    {
        var reasonCodeStr = r["ReasonCode"]?.ToString();
        var reasonTextStr = r["ReasonText"]?.ToString();

        return CredentialLifecycleAction.Hydrate(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            credentialId: Guid.Parse(r["CredentialId"].ToString() ?? string.Empty),
            action: (LifecycleAction)Convert.ToInt32(r["Action"]),
            previousStatus: (CredentialStatus)Convert.ToInt32(r["PreviousStatus"]),
            resultingStatus: (CredentialStatus)Convert.ToInt32(r["ResultingStatus"]),
            actorProfileId: Guid.Parse(r["ActorProfileId"].ToString() ?? string.Empty),
            reasonCode: string.IsNullOrEmpty(reasonCodeStr) ? null : reasonCodeStr,
            reasonText: string.IsNullOrEmpty(reasonTextStr) ? null : reasonTextStr,
            occurredAtUtc: Convert.ToDateTime(r["OccurredAtUtc"]),
            correlationId: Guid.Parse(r["CorrelationId"].ToString() ?? string.Empty));
    }
}
