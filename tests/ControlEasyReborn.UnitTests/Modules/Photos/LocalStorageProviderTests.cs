using System.Text;
using ControlEasyReborn.Infrastructure.Storage;
using ControlEasyReborn.SharedKernel.Storage;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ControlEasyReborn.UnitTests.Modules.Photos;

public sealed class LocalStorageProviderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly LocalFilesystemStorageProvider _sut;

    public LocalStorageProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ce-photos-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var options = Options.Create(new StorageOptions
        {
            Provider = "Local",
            Local = new LocalStorageOptions { Path = _tempDir }
        });

        _sut = new LocalFilesystemStorageProvider(options);
    }

    [Fact]
    public async Task SaveAsync_SavesFileContentAndReturnsRelativePath()
    {
        var content = "test content bytes";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var relativePath = await _sut.SaveAsync(stream, "test.jpg", CancellationToken.None);

        relativePath.Should().NotBeNullOrEmpty();
        relativePath.Should().EndWith("test.jpg");
        relativePath.Should().NotContain("\\"); // Must use forward slashes

        var fullPath = Path.Combine(_tempDir, relativePath);
        File.Exists(fullPath).Should().BeTrue();
        var fileContent = await File.ReadAllTextAsync(fullPath);
        fileContent.Should().Be(content);
    }

    [Fact]
    public async Task OpenReadAsync_WhenFileExists_ReturnsReadableStream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample data"));
        var relativePath = await _sut.SaveAsync(stream, "sample.png", CancellationToken.None);

        using var readStream = await _sut.OpenReadAsync(relativePath, CancellationToken.None);

        readStream.Should().NotBeNull();
        using var reader = new StreamReader(readStream!);
        var readContent = await reader.ReadToEndAsync();
        readContent.Should().Be("sample data");
    }

    [Fact]
    public async Task OpenReadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var result = await _sut.OpenReadAsync("2026-09/nonexistent.jpg", CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenFileExists_DeletesFile()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("to delete"));
        var relativePath = await _sut.SaveAsync(stream, "delete.jpg", CancellationToken.None);

        var fullPath = Path.Combine(_tempDir, relativePath);
        File.Exists(fullPath).Should().BeTrue();

        await _sut.DeleteAsync(relativePath, CancellationToken.None);

        File.Exists(fullPath).Should().BeFalse();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
