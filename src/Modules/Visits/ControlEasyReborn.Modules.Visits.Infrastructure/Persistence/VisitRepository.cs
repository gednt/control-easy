using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Visits.Application.Abstractions;
using ControlEasyReborn.Modules.Visits.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Visits.Infrastructure.Persistence;

public sealed class VisitRepository : IVisitRepository
{
    private const string TableName = "Visits";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public VisitRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Visit?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Visit>> ListAsync(Guid tenantId, string? status, int skip, int take, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var whereClause = string.IsNullOrWhiteSpace(status)
            ? "1=1"
            : "Status = @param0";
        var parameters = string.IsNullOrWhiteSpace(status)
            ? Array.Empty<object>()
            : new object[] { (int)Enum.Parse(typeof(VisitStatus), status, true) };

        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: whereClause,
            parameters: parameters,
            ct: ct);

        var visits = MapList(rows).ToList();
        var existingIds = new HashSet<Guid>(visits.Select(v => v.Id));

        try
        {
            var auditRows = await db.SelectAsync(
                fields: "Id, TenantId, EntryState, OverrideReason, PhotoId, SubjectType, SubjectName, SubjectDocument, PerformedByProfileId, RecordedAt",
                table: "ConsentAuditLog",
                whereClause: "1=1",
                parameters: Array.Empty<object>(),
                ct: ct);

            if (auditRows is not null && auditRows.Rows.Count > 0)
            {
                foreach (DataRow ar in auditRows.Rows)
                {
                    var id = Guid.Parse(ar["Id"].ToString() ?? string.Empty);
                    if (existingIds.Contains(id)) continue;

                    var entryState = ar["EntryState"]?.ToString() ?? string.Empty;
                    var subjectType = ar["SubjectType"]?.ToString() ?? "visitor";
                    var subjectName = ar["SubjectName"]?.ToString();
                    var overrideReason = ar["OverrideReason"]?.ToString();
                    var recordedAt = Convert.ToDateTime(ar["RecordedAt"]);
                    var attendantIdStr = ar["PerformedByProfileId"]?.ToString();
                    Guid? attendantId = string.IsNullOrEmpty(attendantIdStr) ? null : Guid.Parse(attendantIdStr);

                    var visitStatus = entryState switch
                    {
                        "entered_with_consent" or "entered_override" => VisitStatus.CheckedIn,
                        "gatehouse_only" => VisitStatus.CheckedOut,
                        "entered_without_consent" => VisitStatus.Cancelled,
                        _ => VisitStatus.Pending
                    };

                    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VisitStatus>(status, true, out var filterStatus) && visitStatus != filterStatus)
                    {
                        continue;
                    }

                    var defaultName = subjectType switch
                    {
                        "service_provider" => "Service provider",
                        "dweller" => "Resident",
                        "vehicle" => "Vehicle",
                        _ => "Visitor"
                    };

                    var purpose = entryState switch
                    {
                        "gatehouse_only" => "Package drop / delivery",
                        "entered_without_consent" => "Consent refused — entry denied",
                        "entered_override" => string.IsNullOrWhiteSpace(overrideReason) ? "Override entry" : $"Override ({overrideReason})",
                        _ => subjectType == "service_provider" ? "Service provider entry" : subjectType == "dweller" ? "Resident entry" : "Visitor entry"
                    };

                    visits.Add(new Visit(
                        id: id,
                        tenantId: Guid.Parse(ar["TenantId"].ToString() ?? string.Empty),
                        visitorName: !string.IsNullOrWhiteSpace(subjectName) ? subjectName : defaultName,
                        visitorDocument: ar["SubjectDocument"]?.ToString() ?? "N/A",
                        visitorPhone: null,
                        apartmentId: null,
                        purpose: purpose,
                        status: visitStatus,
                        attendantProfileId: attendantId,
                        gatehouseId: null,
                        checkedInAtUtc: (visitStatus == VisitStatus.CheckedIn || visitStatus == VisitStatus.CheckedOut) ? recordedAt : null,
                        checkedOutAtUtc: visitStatus == VisitStatus.CheckedOut ? recordedAt : null,
                        createdAtUtc: recordedAt,
                        updatedAtUtc: null));
                }
            }
        }
        catch
        {
            // Fallback gracefully
        }

        return visits.OrderByDescending(v => v.CreatedAtUtc).Skip(skip).Take(take).ToList();
    }

    public async Task<IReadOnlyList<Visit>> ListOpenAsync(Guid tenantId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, VisitorName, VisitorDocument, VisitorPhone, ApartmentId, Purpose, Status, AttendantProfileId, GatehouseId, CheckedInAtUtc, CheckedOutAtUtc, CreatedAtUtc, UpdatedAtUtc",
            table: TableName,
            whereClause: "Status IN (0, 1)",
            parameters: Array.Empty<object>(),
            ct: ct);

        var visits = MapList(rows).ToList();
        var existingIds = new HashSet<Guid>(visits.Select(v => v.Id));

        try
        {
            var auditRows = await db.SelectAsync(
                fields: "Id, TenantId, EntryState, OverrideReason, PhotoId, SubjectType, SubjectName, SubjectDocument, PerformedByProfileId, RecordedAt",
                table: "ConsentAuditLog",
                whereClause: "EntryState IN ('entered_with_consent', 'entered_override')",
                parameters: Array.Empty<object>(),
                ct: ct);

            if (auditRows is not null && auditRows.Rows.Count > 0)
            {
                foreach (DataRow ar in auditRows.Rows)
                {
                    var id = Guid.Parse(ar["Id"].ToString() ?? string.Empty);
                    if (existingIds.Contains(id)) continue;

                    var subjectType = ar["SubjectType"]?.ToString() ?? "visitor";
                    var subjectName = ar["SubjectName"]?.ToString();
                    var overrideReason = ar["OverrideReason"]?.ToString();
                    var recordedAt = Convert.ToDateTime(ar["RecordedAt"]);
                    var attendantIdStr = ar["PerformedByProfileId"]?.ToString();
                    Guid? attendantId = string.IsNullOrEmpty(attendantIdStr) ? null : Guid.Parse(attendantIdStr);

                    var defaultName = subjectType switch
                    {
                        "service_provider" => "Service provider",
                        "dweller" => "Resident",
                        "vehicle" => "Vehicle",
                        _ => "Visitor"
                    };

                    visits.Add(new Visit(
                        id: id,
                        tenantId: Guid.Parse(ar["TenantId"].ToString() ?? string.Empty),
                        visitorName: !string.IsNullOrWhiteSpace(subjectName) ? subjectName : defaultName,
                        visitorDocument: ar["SubjectDocument"]?.ToString() ?? "N/A",
                        visitorPhone: null,
                        apartmentId: null,
                        purpose: $"Arrival: {subjectType}",
                        status: VisitStatus.CheckedIn,
                        attendantProfileId: attendantId,
                        gatehouseId: null,
                        checkedInAtUtc: recordedAt,
                        checkedOutAtUtc: null,
                        createdAtUtc: recordedAt,
                        updatedAtUtc: null));
                }
            }
        }
        catch
        {
            // Fallback gracefully
        }

        return visits.OrderByDescending(v => v.CreatedAtUtc).ToList();
    }

    public async Task AddAsync(Visit visit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "VisitorName", "VisitorDocument", "VisitorPhone", "ApartmentId", "Purpose", "Status", "AttendantProfileId", "GatehouseId", "CheckedInAtUtc", "CheckedOutAtUtc", "CreatedAtUtc", "UpdatedAtUtc", "tenant_id" },
            TableName,
            new object?[] { visit.Id, visit.TenantId, visit.VisitorName, visit.VisitorDocument, (object?)visit.VisitorPhone ?? DBNull.Value, (object?)visit.ApartmentId ?? DBNull.Value, (object?)visit.Purpose ?? DBNull.Value, (int)visit.Status, (object?)visit.AttendantProfileId ?? DBNull.Value, (object?)visit.GatehouseId ?? DBNull.Value, (object?)visit.CheckedInAtUtc ?? DBNull.Value, (object?)visit.CheckedOutAtUtc ?? DBNull.Value, visit.CreatedAtUtc, (object?)visit.UpdatedAtUtc ?? DBNull.Value, visit.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Visit visit, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "VisitorName", "VisitorDocument", "VisitorPhone", "ApartmentId", "Purpose", "Status", "AttendantProfileId", "GatehouseId", "CheckedInAtUtc", "CheckedOutAtUtc", "UpdatedAtUtc" },
            TableName,
            new[]
            {
                visit.VisitorName,
                visit.VisitorDocument,
                visit.VisitorPhone,
                visit.ApartmentId?.ToString(),
                visit.Purpose,
                ((int)visit.Status).ToString(),
                visit.AttendantProfileId?.ToString(),
                visit.GatehouseId?.ToString(),
                FormatDateTime(visit.CheckedInAtUtc),
                FormatDateTime(visit.CheckedOutAtUtc),
                FormatDateTime(visit.UpdatedAtUtc),
            },
            $"Id = '{visit.Id}'",
            Array.Empty<object>(),
            ct: ct);
    }

    private static string? FormatDateTime(DateTime? value) =>
        value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff") : null;

    private static Visit? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Visit> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Visit>();
        var list = new List<Visit>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Visit? MapRow(DataRow r)
    {
        var apartmentIdStr = r["ApartmentId"]?.ToString();
        var attendantProfileIdStr = r["AttendantProfileId"]?.ToString();
        var gatehouseIdStr = r["GatehouseId"]?.ToString();
        var checkedInAtStr = r["CheckedInAtUtc"]?.ToString();
        var checkedOutAtStr = r["CheckedOutAtUtc"]?.ToString();
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new Visit(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            visitorName: r["VisitorName"]?.ToString() ?? string.Empty,
            visitorDocument: r["VisitorDocument"]?.ToString() ?? string.Empty,
            visitorPhone: string.IsNullOrEmpty(r["VisitorPhone"]?.ToString()) ? null : r["VisitorPhone"].ToString(),
            apartmentId: string.IsNullOrEmpty(apartmentIdStr) ? null : Guid.Parse(apartmentIdStr),
            purpose: string.IsNullOrEmpty(r["Purpose"]?.ToString()) ? null : r["Purpose"].ToString(),
            status: (VisitStatus)Convert.ToInt32(r["Status"]),
            attendantProfileId: string.IsNullOrEmpty(attendantProfileIdStr) ? null : Guid.Parse(attendantProfileIdStr),
            gatehouseId: string.IsNullOrEmpty(gatehouseIdStr) ? null : Guid.Parse(gatehouseIdStr),
            checkedInAtUtc: string.IsNullOrEmpty(checkedInAtStr) ? null : DateTime.Parse(checkedInAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            checkedOutAtUtc: string.IsNullOrEmpty(checkedOutAtStr) ? null : DateTime.Parse(checkedOutAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}