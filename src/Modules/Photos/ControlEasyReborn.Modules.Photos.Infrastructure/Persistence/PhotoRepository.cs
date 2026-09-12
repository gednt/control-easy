using ControlEasyReborn.Infrastructure.MultiTenancy;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.SharedKernel.MultiTenancy;
using DBTools.Abstractions;
using System.Data;

namespace ControlEasyReborn.Modules.Photos.Infrastructure.Persistence;

public sealed class PhotoRepository : IPhotoRepository
{
    private const string TableName = "Photos";

    private const string Fields = "Id, TenantId, FilePath, ThumbnailPath, MimeType, SizeBytes, CapturedAtUtc, CreatedAtUtc, DeletedAtUtc";

    private readonly ITenantContext _ctx;
    private readonly ITenantAwareLinqFactory _factory;

    public PhotoRepository(ITenantContext ctx, ITenantAwareLinqFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<Photo?> FindAsync(Guid id, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        var rows = await db.SelectAsync(
            fields: Fields,
            table: TableName,
            whereClause: "Id = @param0",
            parameters: new object[] { id },
            ct: ct);
        return MapFirstOrDefault(rows);
    }

    public async Task AddAsync(Photo photo, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.InsertAsync(
            new[] { "Id", "TenantId", "FilePath", "ThumbnailPath", "MimeType", "SizeBytes", "CapturedAtUtc", "CreatedAtUtc", "DeletedAtUtc", "tenant_id" },
            TableName,
            new object?[] { photo.Id, photo.TenantId, photo.FilePath, (object?)photo.ThumbnailPath ?? DBNull.Value, photo.MimeType, photo.SizeBytes, (object?)photo.CapturedAtUtc ?? DBNull.Value, photo.CreatedAtUtc, (object?)photo.DeletedAtUtc ?? DBNull.Value, photo.TenantId },
            primaryKeyName: "Id",
            autoIncrement: false,
            ct: ct);
    }

    public async Task SoftDeleteAsync(Photo photo, CancellationToken ct)
    {
        var db = _factory.Create(_ctx);
        await db.UpdateAsync(
            new[] { "DeletedAtUtc" },
            TableName,
            new[] { photo.DeletedAtUtc!.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") },
            "Id = @param1",
            new object[] { photo.Id },
            ct: ct);
    }

    private static Photo? MapFirstOrDefault(DataTable rows)
    {
        if (rows is null || rows.Rows.Count == 0) return null;
        return MapRow(rows.Rows[0]);
    }

    private static Photo? MapRow(DataRow r)
    {
        var thumbnailPathStr = r["ThumbnailPath"]?.ToString();
        var capturedAtStr = r["CapturedAtUtc"]?.ToString();
        var deletedAtStr = r["DeletedAtUtc"]?.ToString();

        return new Photo(
            id: Guid.Parse(r["Id"].ToString() ?? string.Empty),
            tenantId: Guid.Parse(r["TenantId"].ToString() ?? string.Empty),
            filePath: r["FilePath"]?.ToString() ?? string.Empty,
            thumbnailPath: string.IsNullOrEmpty(thumbnailPathStr) ? null : thumbnailPathStr,
            mimeType: r["MimeType"]?.ToString() ?? string.Empty,
            sizeBytes: Convert.ToInt64(r["SizeBytes"]),
            capturedAtUtc: string.IsNullOrEmpty(capturedAtStr) ? null : DateTime.Parse(capturedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind),
            createdAtUtc: Convert.ToDateTime(r["CreatedAtUtc"]),
            deletedAtUtc: string.IsNullOrEmpty(deletedAtStr) ? null : DateTime.Parse(deletedAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}