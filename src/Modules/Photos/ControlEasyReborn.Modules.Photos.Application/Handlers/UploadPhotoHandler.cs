using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.SharedKernel.Storage;
using FluentValidation;

namespace ControlEasyReborn.Modules.Photos.Application.Handlers;

public sealed class UploadPhotoHandler
{
    private static readonly string[] ExtensionByMime = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    }.Values.ToArray();

    private readonly IPhotoRepository _photos;
    private readonly IStorageProvider _storage;
    private readonly IValidator<UploadPhotoMetadata> _validator;

    public UploadPhotoHandler(
        IPhotoRepository photos,
        IStorageProvider storage,
        IValidator<UploadPhotoMetadata> validator)
    {
        _photos = photos;
        _storage = storage;
        _validator = validator;
    }

    public async Task<PhotoResponse> HandleAsync(Stream content, UploadPhotoMetadata metadata, Guid tenantId, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(metadata, ct);
        if (!result.IsValid)
        {
            throw new Errors.ValidationException(result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var fileName = string.IsNullOrWhiteSpace(metadata.FileName)
            ? "photo" + ExtensionFor(metadata.MimeType)
            : metadata.FileName;

        var filePath = await _storage.SaveAsync(content, fileName, ct);

        var photo = new Photo(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            filePath: filePath,
            thumbnailPath: null,
            mimeType: metadata.MimeType,
            sizeBytes: content.CanSeek ? content.Length : 0,
            capturedAtUtc: metadata.CapturedAtUtc,
            createdAtUtc: DateTime.UtcNow);

        await _photos.AddAsync(photo, ct);
        return ToResponse(photo);
    }

    internal static PhotoResponse ToResponse(Photo p) =>
        new(p.Id, p.TenantId, p.FilePath, p.ThumbnailPath, p.MimeType, p.SizeBytes, p.CapturedAtUtc, p.CreatedAtUtc, p.DeletedAtUtc);

    private static string ExtensionFor(string mimeType) => mimeType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".bin"
    };
}