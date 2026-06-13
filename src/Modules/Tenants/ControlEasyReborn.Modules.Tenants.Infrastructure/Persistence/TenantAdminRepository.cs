using System.Data;
using ControlEasyReborn.Modules.Tenants.Application.Abstractions;
using ControlEasyReborn.Modules.Tenants.Application.Contracts;
using DBTools.Abstractions;

namespace ControlEasyReborn.Modules.Tenants.Infrastructure.Persistence;

public sealed class TenantAdminRepository : ITenantAdminRepository
{
    private const string TableName = "Users";
    private const string SelectFields = "Id, TenantId, Email, DisplayName, Active, Roles, CreatedAtUtc";

    private readonly IAsyncSqlClient _db;

    public TenantAdminRepository(IAsyncSqlClient db)
    {
        _db = db;
    }

    public async Task<TenantAdminResponse> CreateAdminAsync(Guid tenantId, string email, string displayName, string passwordHash, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await _db.InsertAsync(
            new[] { "Id", "TenantId", "Email", "PasswordHash", "DisplayName", "Active", "MustChangePassword", "Roles", "CreatedAtUtc", "UpdatedAtUtc" },
            TableName,
            new object?[] { userId, tenantId, email, passwordHash, displayName, true, true, "TenantAdmin", now, DBNull.Value },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);

        return new TenantAdminResponse(userId, email, displayName, tenantId, true, now);
    }

    public async Task<IReadOnlyList<TenantAdminResponse>> ListAdminsAsync(Guid tenantId, CancellationToken ct)
    {
        var rows = await _db.SelectAsync(
            fields: SelectFields,
            table: TableName,
            whereClause: "TenantId = @param0 AND Roles LIKE '%TenantAdmin%'",
            parameters: new object[] { tenantId },
            ct: ct);

        var result = new List<TenantAdminResponse>();
        if (rows is null || rows.Rows.Count == 0) return result;

        foreach (DataRow r in rows.Rows)
        {
            result.Add(new TenantAdminResponse(
                UserId: Guid.Parse(r["Id"].ToString() ?? string.Empty),
                Email: r["Email"]?.ToString() ?? string.Empty,
                DisplayName: r["DisplayName"]?.ToString() ?? string.Empty,
                TenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
                Active: Convert.ToBoolean(r["Active"]),
                CreatedAtUtc: Convert.ToDateTime(r["CreatedAtUtc"])));
        }

        return result;
    }

    public async Task RevokeAdminAsync(Guid userId, CancellationToken ct)
    {
        await _db.UpdateAsync(
            new[] { "Roles", "UpdatedAtUtc" },
            TableName,
            new[] { "", DateTime.UtcNow.ToString("o") },
            whereClause: "Id = @param0",
            whereParameters: new object[] { userId },
            ct: ct);
    }
}