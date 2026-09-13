using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Administration.Application.Abstractions;
using ControlEasyReborn.Modules.Administration.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Administration.Infrastructure.Persistence;

public sealed class CondominiumSettingsRepository : ICondominiumSettingsRepository
{
    private const string TableName = "CondominiumSettings";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public CondominiumSettingsRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<CondominiumSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "*",
            table: TableName,
            whereClause: "TenantId = @param0",
            parameters: new object[] { tenantId },
            ct: ct);

        return MapFirstOrDefault(rows);
    }

    public async Task SaveAsync(CondominiumSettings settings, CancellationToken ct)
    {
        var existing = await GetByTenantIdAsync(settings.TenantId, ct);
        var db = _factory.Create(_ctx);

        if (existing is null)
        {
            await db.InsertAsync(
                new[]
                {
                    "Id",
                    "TenantId",
                    "VisitDurationMinutes",
                    "RequireShiftHandoverNotes",
                    "DefaultShiftLengthHours",
                    "EmergencyContactPhone",
                    "AllowedVisitorStartHour",
                    "AllowedVisitorEndHour",
                    "AutoCheckoutAtMidnight",
                    "MaxActiveVisitorsPerUnit",
                    "PhotoRequiredVisitors",
                    "PhotoRequiredProviders",
                    "PhotoRequiredResidents",
                    "AllowOverrideOnRefusal",
                    "OverdueVisitAlertMinutes",
                    "CreatedAtUtc",
                    "UpdatedAtUtc"
                },
                TableName,
                new object?[]
                {
                    settings.Id,
                    settings.TenantId,
                    settings.VisitDurationMinutes,
                    settings.RequireShiftHandoverNotes,
                    settings.DefaultShiftLengthHours,
                    (object?)settings.EmergencyContactPhone ?? DBNull.Value,
                    settings.AllowedVisitorStartHour,
                    settings.AllowedVisitorEndHour,
                    settings.AutoCheckoutAtMidnight,
                    settings.MaxActiveVisitorsPerUnit,
                    settings.PhotoRequiredVisitors,
                    settings.PhotoRequiredProviders,
                    settings.PhotoRequiredResidents,
                    settings.AllowOverrideOnRefusal,
                    settings.OverdueVisitAlertMinutes,
                    settings.CreatedAtUtc,
                    (object?)settings.UpdatedAtUtc ?? DBNull.Value
                },
                primaryKeyName: "Id",
                autoIncrement: false,
                ct: ct);
        }
        else
        {
            await db.UpdateAsync(
                new[]
                {
                    "VisitDurationMinutes",
                    "RequireShiftHandoverNotes",
                    "DefaultShiftLengthHours",
                    "EmergencyContactPhone",
                    "AllowedVisitorStartHour",
                    "AllowedVisitorEndHour",
                    "AutoCheckoutAtMidnight",
                    "MaxActiveVisitorsPerUnit",
                    "PhotoRequiredVisitors",
                    "PhotoRequiredProviders",
                    "PhotoRequiredResidents",
                    "AllowOverrideOnRefusal",
                    "OverdueVisitAlertMinutes",
                    "UpdatedAtUtc"
                },
                TableName,
                new[]
                {
                    settings.VisitDurationMinutes.ToString(),
                    settings.RequireShiftHandoverNotes ? "1" : "0",
                    settings.DefaultShiftLengthHours.ToString(),
                    settings.EmergencyContactPhone ?? string.Empty,
                    settings.AllowedVisitorStartHour,
                    settings.AllowedVisitorEndHour,
                    settings.AutoCheckoutAtMidnight ? "1" : "0",
                    settings.MaxActiveVisitorsPerUnit.ToString(),
                    settings.PhotoRequiredVisitors ? "1" : "0",
                    settings.PhotoRequiredProviders ? "1" : "0",
                    settings.PhotoRequiredResidents ? "1" : "0",
                    settings.AllowOverrideOnRefusal ? "1" : "0",
                    settings.OverdueVisitAlertMinutes.ToString(),
                    settings.UpdatedAtUtc.HasValue ? settings.UpdatedAtUtc.Value.ToString("o") : DateTime.UtcNow.ToString("o")
                },
                "TenantId = @param0",
                new object[] { settings.TenantId },
                ct: ct);
        }
    }

    private static CondominiumSettings? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static CondominiumSettings? MapRow(DataRow r)
    {
        var updatedAtStr = r["UpdatedAtUtc"]?.ToString();

        return new CondominiumSettings(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            visitDurationMinutes: Convert.ToInt32(r["VisitDurationMinutes"]),
            requireShiftHandoverNotes: Convert.ToBoolean(r["RequireShiftHandoverNotes"]),
            defaultShiftLengthHours: Convert.ToInt32(r["DefaultShiftLengthHours"]),
            emergencyContactPhone: string.IsNullOrEmpty(r["EmergencyContactPhone"]?.ToString()) ? null : r["EmergencyContactPhone"].ToString(),
            allowedVisitorStartHour: r["AllowedVisitorStartHour"]?.ToString() ?? "06:00",
            allowedVisitorEndHour: r["AllowedVisitorEndHour"]?.ToString() ?? "22:00",
            autoCheckoutAtMidnight: Convert.ToBoolean(r["AutoCheckoutAtMidnight"]),
            maxActiveVisitorsPerUnit: Convert.ToInt32(r["MaxActiveVisitorsPerUnit"]),
            photoRequiredVisitors: Convert.ToBoolean(r["PhotoRequiredVisitors"]),
            photoRequiredProviders: Convert.ToBoolean(r["PhotoRequiredProviders"]),
            photoRequiredResidents: Convert.ToBoolean(r["PhotoRequiredResidents"]),
            allowOverrideOnRefusal: Convert.ToBoolean(r["AllowOverrideOnRefusal"]),
            overdueVisitAlertMinutes: Convert.ToInt32(r["OverdueVisitAlertMinutes"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: string.IsNullOrEmpty(updatedAtStr) ? null : DateTime.Parse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}
