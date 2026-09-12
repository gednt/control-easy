namespace ControlEasyReborn.Modules.Photos.Domain.Entities;

public sealed class Photo
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string? ThumbnailPath { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DateTime? CapturedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public Photo(
        Guid id,
        Guid tenantId,
        string filePath,
        string? thumbnailPath,
        string mimeType,
        long sizeBytes,
        DateTime? capturedAtUtc,
        DateTime createdAtUtc,
        DateTime? deletedAtUtc = null)
    {
        Id = id;
        TenantId = tenantId;
        FilePath = filePath;
        ThumbnailPath = thumbnailPath;
        MimeType = mimeType;
        SizeBytes = sizeBytes;
        CapturedAtUtc = capturedAtUtc;
        CreatedAtUtc = createdAtUtc;
        DeletedAtUtc = deletedAtUtc;
    }

    public bool IsDeleted => DeletedAtUtc.HasValue;

    public void SoftDelete()
    {
        if (DeletedAtUtc.HasValue)
            throw new InvalidOperationException("Photo is already deleted.");
        DeletedAtUtc = DateTime.UtcNow;
    }
}