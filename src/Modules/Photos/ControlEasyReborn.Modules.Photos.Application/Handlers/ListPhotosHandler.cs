using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;

namespace ControlEasyReborn.Modules.Photos.Application.Handlers;

public sealed class ListPhotosHandler
{
    private readonly IPhotoRepository _photos;

    public ListPhotosHandler(IPhotoRepository photos) => _photos = photos;

    public async Task<IReadOnlyList<PhotoResponse>> HandleAsync(string entityType, string entityId, CancellationToken ct)
    {
        var photos = await _photos.ListByEntityAsync(entityType, entityId, ct);
        return photos.Select(UploadPhotoHandler.ToResponse).ToArray();
    }
}
