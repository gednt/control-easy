using System.Text;
using ControlEasyReborn.Modules.Photos.Application.Abstractions;
using ControlEasyReborn.Modules.Photos.Application.Contracts;
using ControlEasyReborn.Modules.Photos.Application.Errors;
using ControlEasyReborn.Modules.Photos.Application.Handlers;
using ControlEasyReborn.Modules.Photos.Application.Validators;
using ControlEasyReborn.Modules.Photos.Domain.Entities;
using ControlEasyReborn.SharedKernel.Storage;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class UploadPhotoHandlerTests
{
    private readonly IPhotoRepository _photoRepository = Substitute.For<IPhotoRepository>();
    private readonly IStorageProvider _storageProvider = Substitute.For<IStorageProvider>();
    private readonly UploadPhotoMetadataValidator _validator = new();
    private readonly UploadPhotoHandler _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UploadPhotoHandlerTests()
    {
        _sut = new UploadPhotoHandler(_photoRepository, _storageProvider, _validator);
    }

    [Theory]
    [InlineData("image/jpeg", "photo.jpg")]
    [InlineData("image/png", "photo.png")]
    [InlineData("image/webp", "photo.webp")]
    public async Task HandleAsync_WithValidMimeTypes_StoresPhotoAndReturnsResponse(string mimeType, string fileName)
    {
        var contentBytes = Encoding.UTF8.GetBytes("fake-image-bytes");
        using var stream = new MemoryStream(contentBytes);
        var metadata = new UploadPhotoMetadata(mimeType, DateTime.UtcNow, fileName);

        _storageProvider.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("2026-09/photo.jpg");

        var response = await _sut.HandleAsync(stream, metadata, _tenantId, CancellationToken.None);

        response.Should().NotBeNull();
        response.TenantId.Should().Be(_tenantId);
        response.MimeType.Should().Be(mimeType);
        response.SizeBytes.Should().Be(contentBytes.Length);
        response.FilePath.Should().Be("2026-09/photo.jpg");

        await _photoRepository.Received(1).AddAsync(Arg.Is<Photo>(p =>
            p.TenantId == _tenantId &&
            p.MimeType == mimeType &&
            p.SizeBytes == contentBytes.Length &&
            p.FilePath == "2026-09/photo.jpg"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("text/plain")]
    [InlineData("")]
    public async Task HandleAsync_WithUnsupportedMimeType_ThrowsValidationException(string mimeType)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake"));
        var metadata = new UploadPhotoMetadata(mimeType, DateTime.UtcNow, "test.file");

        var act = () => _sut.HandleAsync(stream, metadata, _tenantId, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.ContainsKey("MimeType"));

        await _storageProvider.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default);
        await _photoRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
