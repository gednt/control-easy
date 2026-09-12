using System.Text;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.SharedKernel.Storage;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class PhotoLifecycleHandlersTests
{
    private readonly IPhotoRepository _photoRepository = Substitute.For<IPhotoRepository>();
    private readonly IStorageProvider _storageProvider = Substitute.For<IStorageProvider>();
    private readonly GetPhotoHandler _getHandler;
    private readonly SoftDeletePhotoHandler _deleteHandler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public PhotoLifecycleHandlersTests()
    {
        _getHandler = new GetPhotoHandler(_photoRepository, _storageProvider);
        _deleteHandler = new SoftDeletePhotoHandler(_photoRepository);
    }

    [Fact]
    public async Task GetPhoto_WhenPhotoExistsAndNotDeleted_ReturnsContentStream()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 1024, DateTime.UtcNow, DateTime.UtcNow);
        var fakeStream = new MemoryStream(Encoding.UTF8.GetBytes("image-data"));

        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);
        _storageProvider.OpenReadAsync("2026-09/photo.jpg", Arg.Any<CancellationToken>()).Returns(fakeStream);

        var result = await _getHandler.HandleAsync(photoId, CancellationToken.None);

        result.Should().NotBeNull();
        result.MimeType.Should().Be("image/jpeg");
        result.SizeBytes.Should().Be(1024);
        result.Content.Should().BeSameAs(fakeStream);
    }

    [Fact]
    public async Task GetPhoto_WhenPhotoNotFoundInRepository_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns((Photo?)null);

        var act = () => _getHandler.HandleAsync(photoId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPhoto_WhenPhotoIsSoftDeleted_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 1024, DateTime.UtcNow, DateTime.UtcNow);
        photo.SoftDelete();

        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        var act = () => _getHandler.HandleAsync(photoId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _storageProvider.DidNotReceiveWithAnyArgs().OpenReadAsync(default!, default);
    }

    [Fact]
    public async Task GetPhoto_WhenStorageReturnsNull_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 1024, DateTime.UtcNow, DateTime.UtcNow);

        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);
        _storageProvider.OpenReadAsync("2026-09/photo.jpg", Arg.Any<CancellationToken>()).Returns((Stream?)null);

        var act = () => _getHandler.HandleAsync(photoId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDelete_WhenPhotoExistsAndActive_MarksDeletedAndPersists()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 1024, DateTime.UtcNow, DateTime.UtcNow);

        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        await _deleteHandler.HandleAsync(photoId, CancellationToken.None);

        photo.IsDeleted.Should().BeTrue();
        photo.DeletedAtUtc.Should().NotBeNull();

        await _photoRepository.Received(1).SoftDeleteAsync(photo, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SoftDelete_WhenPhotoNotFound_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns((Photo?)null);

        var act = () => _deleteHandler.HandleAsync(photoId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDelete_WhenPhotoAlreadyDeleted_ThrowsNotFoundException()
    {
        var photoId = Guid.NewGuid();
        var photo = new Photo(photoId, _tenantId, "2026-09/photo.jpg", null, "image/jpeg", 1024, DateTime.UtcNow, DateTime.UtcNow);
        photo.SoftDelete();

        _photoRepository.FindAsync(photoId, Arg.Any<CancellationToken>()).Returns(photo);

        var act = () => _deleteHandler.HandleAsync(photoId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _photoRepository.DidNotReceive().SoftDeleteAsync(Arg.Any<Photo>(), Arg.Any<CancellationToken>());
    }
}
