using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.AccessControl.Application.Abstractions;
using ControlEasyReborn.Modules.AccessControl.Domain.Entities;
using ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.AccessControl.Infrastructure.Persistence;

public sealed class RefusedScanAttemptRepository : IRefusedScanAttemptRepository
{
    private const string TableName = "RefusedScanAttempts";
    private const string Fields = "Id, TenantId, ScanAttemptId, CredentialFingerprint, Direction, FailureCode, PerformedByProfileId, GatehouseId, OccurredAtUtc, CorrelationId, RelatedCredentialId, RelatedSubjectId";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public RefusedScanAttemptRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<RefusedScanAttempt?> FindByScanAttemptAsync(Guid tenantId, Guid scanAttemptId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "ScanAttemptId = @param0",
            parameters: new object[] { scanAttemptId },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<RefusedScanAttempt>> ListAsync(Guid tenantId, DateTime? fromUtc, DateTime? toUtc, CycleDirection? direction, string? failureCode, Guid? attendantProfileId, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereParts = new List<string>();
        var parameters = new List<object>();
        if (fromUtc.HasValue)
        {
            whereParts.Add("OccurredAtUtc >= @param" + parameters.Count);
            parameters.Add(fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            whereParts.Add("OccurredAtUtc <= @param" + parameters.Count);
            parameters.Add(toUtc.Value);
        }
        if (direction.HasValue)
        {
            whereParts.Add("Direction = @param" + parameters.Count);
            parameters.Add((int)direction.Value);
        }
        if (!string.IsNullOrWhiteSpace(failureCode))
        {
            whereParts.Add("FailureCode = @param" + parameters.Count);
            parameters.Add(failureCode);
        }
        if (attendantProfileId.HasValue)
        {
            whereParts.Add("PerformedByProfileId = @param" + parameters.Count);
            parameters.Add(attendantProfileId.Value);
        }
        var whereClause = whereParts.Count == 0 ? "1=1" : string.Join(" AND ", whereParts);

        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: whereClause,
            parameters: parameters.ToArray(),
            ct: ct);

        return MapList(rows).OrderByDescending(a => a.OccurredAtUtc).Skip(skip).Take(take).ToList();
    }

    public async Task AddAsync(RefusedScanAttempt attempt, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "ScanAttemptId", "CredentialFingerprint", "Direction", "FailureCode", "PerformedByProfileId", "GatehouseId", "OccurredAtUtc", "CorrelationId", "RelatedCredentialId", "RelatedSubjectId", "tenant_id" },
            TableName,
            new object?[] { attempt.Id, attempt.TenantId, attempt.ScanAttemptId, (object?)attempt.CredentialFingerprint ?? DBNull.Value, attempt.Direction.HasValue ? (int)attempt.Direction.Value : (object)DBNull.Value, attempt.FailureCode, attempt.PerformedByProfileId, (object?)attempt.GatehouseId ?? DBNull.Value, attempt.OccurredAtUtc, attempt.CorrelationId, (object?)attempt.RelatedCredentialId ?? DBNull.Value, (object?)attempt.RelatedSubjectId ?? DBNull.Value, attempt.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    private static RefusedScanAttempt? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<RefusedScanAttempt> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<RefusedScanAttempt>();
        var list = new List<RefusedScanAttempt>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static RefusedScanAttempt? MapRow(DataRow r)
    {
        var directionStr = r["Direction"]?.ToString();
        var fingerprint = r["CredentialFingerprint"] as byte[];
        var gatehouseIdStr = r["GatehouseId"]?.ToString();
        var relatedCredentialIdStr = r["RelatedCredentialId"]?.ToString();
        var relatedSubjectIdStr = r["RelatedSubjectId"]?.ToString();

        return RefusedScanAttempt.Hydrate(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            scanAttemptId: Guid.Parse(r["ScanAttemptId"].ToString() ?? string.Empty),
            credentialFingerprint: fingerprint,
            direction: string.IsNullOrEmpty(directionStr) ? null : (CycleDirection)Convert.ToInt32(directionStr),
            failureCode: r["FailureCode"]?.ToString() ?? string.Empty,
            performedByProfileId: Guid.Parse(r["PerformedByProfileId"].ToString() ?? string.Empty),
            gatehouseId: string.IsNullOrEmpty(gatehouseIdStr) ? null : Guid.Parse(gatehouseIdStr),
            occurredAtUtc: Convert.ToDateTime(r["OccurredAtUtc"]),
            correlationId: Guid.Parse(r["CorrelationId"].ToString() ?? string.Empty),
            relatedCredentialId: string.IsNullOrEmpty(relatedCredentialIdStr) ? null : Guid.Parse(relatedCredentialIdStr),
            relatedSubjectId: string.IsNullOrEmpty(relatedSubjectIdStr) ? null : Guid.Parse(relatedSubjectIdStr));
    }
}
