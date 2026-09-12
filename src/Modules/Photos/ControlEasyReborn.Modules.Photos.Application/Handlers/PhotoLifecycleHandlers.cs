using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.SharedKernel.Storage;

namespace ControlEasyReborn.Modules.Photos.Application.Handlers;

public sealed record GetPhotoResult(Stream Content, string MimeType, long? SizeBytes);

public sealed class GetPhotoHandler
{
    private readonly IPhotoRepository _photos;
    private readonly IStorageProvider _storage;

    public GetPhotoHandler(IPhotoRepository photos, IStorageProvider storage)
    {
        _photos = photos;
        _storage = storage;
    }

    public async Task<GetPhotoResult> HandleAsync(Guid id, CancellationToken ct)
    {
        var photo = await _photos.FindAsync(id, ct)
            ?? throw new NotFoundException($"Photo {id} was not found.");

        if (photo.IsDeleted)
            throw new NotFoundException($"Photo {id} was not found.");

        var content = await _storage.OpenReadAsync(photo.FilePath, ct)
            ?? throw new NotFoundException($"Photo {id} was not found.");

        return new GetPhotoResult(content, photo.MimeType, photo.SizeBytes);
    }
}

public sealed class SoftDeletePhotoHandler
{
    private readonly IPhotoRepository _photos;

    public SoftDeletePhotoHandler(IPhotoRepository photos)
    {
        _photos = photos;
    }

    public async Task HandleAsync(Guid id, CancellationToken ct)
    {
        var photo = await _photos.FindAsync(id, ct)
            ?? throw new NotFoundException($"Photo {id} was not found.");

        if (photo.IsDeleted)
            throw new NotFoundException($"Photo {id} was not found.");

        photo.SoftDelete();
        await _photos.SoftDeleteAsync(photo, ct);
    }
}