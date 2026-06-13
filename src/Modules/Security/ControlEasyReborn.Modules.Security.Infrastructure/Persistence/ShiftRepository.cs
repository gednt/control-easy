using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Security.Application.Abstractions;
using ControlEasyReborn.Modules.Security.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Security.Infrastructure.Persistence;

public sealed class ShiftRepository : IShiftRepository
{
    private const string TableName = "Shifts";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public ShiftRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Shift?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Name, StartTime, EndTime",
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task<IReadOnlyList<Shift>> ListByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: "Id, TenantId, Name, StartTime, EndTime",
            table: TableName,
            whereClause: "TenantId = @param0",
            parameters: new object[] { tenantId },
            ct: ct);
        return MapList(rows);
    }

    public async Task AddAsync(Shift shift, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "Name", "StartTime", "EndTime", "CrossesMidnight" },
            TableName,
            new object[] { shift.Id, shift.TenantId, shift.Name, shift.StartTime, shift.EndTime, shift.CrossesMidnight },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task UpdateAsync(Shift shift, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "Name", "StartTime", "EndTime", "CrossesMidnight" },
            TableName,
            new[] { shift.Name, shift.StartTime.ToString(), shift.EndTime.ToString(), shift.CrossesMidnight ? "1" : "0" },
            "Id = @param0",
            new object[] { shift.Id },
            ct: ct);
    }

    private static Shift? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static IReadOnlyList<Shift> MapList(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return Array.Empty<Shift>();
        var list = new List<Shift>(rows.Rows.Count);
        foreach (DataRow r in rows.Rows)
        {
            var mapped = MapRow(r);
            if (mapped is not null) list.Add(mapped);
        }
        return list;
    }

    private static Shift? MapRow(DataRow r)
    {
        return new Shift(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            name: r["Name"]?.ToString() ?? string.Empty,
            startTime: (TimeSpan)r["StartTime"],
            endTime: (TimeSpan)r["EndTime"]);
    }
}