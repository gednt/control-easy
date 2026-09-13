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
            whereClause: $"TenantId = '{tenantId}'",
            parameters: Array.Empty<object>(),
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
                    "tenant_id",
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
                    settings.TenantId,
                    settings.VisitDurationMinutes,
                    settings.RequireShiftHandoverNotes ? 1 : 0,
                    settings.DefaultShiftLengthHours,
                    (object?)settings.EmergencyContactPhone ?? DBNull.Value,
                    settings.AllowedVisitorStartHour,
                    settings.AllowedVisitorEndHour,
                    settings.AutoCheckoutAtMidnight ? 1 : 0,
                    settings.MaxActiveVisitorsPerUnit,
                    settings.PhotoRequiredVisitors ? 1 : 0,
                    settings.PhotoRequiredProviders ? 1 : 0,
                    settings.PhotoRequiredResidents ? 1 : 0,
                    settings.AllowOverrideOnRefusal ? 1 : 0,
                    settings.OverdueVisitAlertMinutes,
                    settings.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                    settings.UpdatedAtUtc.HasValue ? settings.UpdatedAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value
                },
                primaryKeyName: "Id",
                autoIncrement: false,
                ct: ct);
        }
        else
        {
            var updated = await db.UpdateAsync(
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
                    settings.UpdatedAtUtc.HasValue ? settings.UpdatedAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                },
                $"TenantId = '{settings.TenantId}'",
                Array.Empty<object>(),
                ct: ct);

            if (!updated)
            {
                throw new InvalidOperationException($"Failed to update CondominiumSettings for tenant {settings.TenantId}. {db.Error}");
            }
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
        DateTime? updatedAt = null;
        if (!string.IsNullOrEmpty(updatedAtStr))
        {
            if (DateTime.TryParse(updatedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d))
                updatedAt = d;
            else if (DateTime.TryParse(updatedAtStr, out var d2))
                updatedAt = d2;
        }

        return new CondominiumSettings(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            visitDurationMinutes: Convert.ToInt32(r["VisitDurationMinutes"]),
            requireShiftHandoverNotes: ToBool(r["RequireShiftHandoverNotes"], true),
            defaultShiftLengthHours: Convert.ToInt32(r["DefaultShiftLengthHours"]),
            emergencyContactPhone: string.IsNullOrEmpty(r["EmergencyContactPhone"]?.ToString()) ? null : r["EmergencyContactPhone"].ToString(),
            allowedVisitorStartHour: r["AllowedVisitorStartHour"]?.ToString() ?? "06:00",
            allowedVisitorEndHour: r["AllowedVisitorEndHour"]?.ToString() ?? "22:00",
            autoCheckoutAtMidnight: ToBool(r["AutoCheckoutAtMidnight"], true),
            maxActiveVisitorsPerUnit: Convert.ToInt32(r["MaxActiveVisitorsPerUnit"]),
            photoRequiredVisitors: ToBool(r["PhotoRequiredVisitors"], true),
            photoRequiredProviders: ToBool(r["PhotoRequiredProviders"], true),
            photoRequiredResidents: ToBool(r["PhotoRequiredResidents"], false),
            allowOverrideOnRefusal: ToBool(r["AllowOverrideOnRefusal"], true),
            overdueVisitAlertMinutes: Convert.ToInt32(r["OverdueVisitAlertMinutes"]),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            updatedAtUtc: updatedAt);
    }

    private static bool ToBool(object? val, bool defaultValue = false)
    {
        if (val is null or DBNull) return defaultValue;
        if (val is bool b) return b;
        var s = val.ToString()?.Trim();
        if (string.IsNullOrEmpty(s)) return defaultValue;
        if (s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) return true;
        if (s == "0" || string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)) return false;
        return defaultValue;
    }
}
