using ControlEasyReborn.SharedKernel.Storage;
using Microsoft.Extensions.Logging;

namespace ControlEasyReborn.Infrastructure.Storage;

/// <summary>S3-backed implementation using Amazon.S3 (MinIO-compatible, path-style addressing).</summary>
public sealed class AmazonS3StorageProvider : IStorageProvider
{
    private readonly Amazon.S3.IAmazonS3 _client;
    private readonly string _bucket;
    private readonly ILogger<AmazonS3StorageProvider> _logger;

    public AmazonS3StorageProvider(Amazon.S3.IAmazonS3 client, string bucket, ILogger<AmazonS3StorageProvider> logger)
    {
        _client = client;
        _bucket = bucket;
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)
    {
        var safeName = SanitizeFileName(fileName);
        var key = $"{DateTime.UtcNow:yyyy-MM}/{Guid.NewGuid():N}-{safeName}";

        await _client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            AutoCloseStream = false
        }, ct);

        return key;
    }

    public async Task<Stream?> OpenReadAsync(string path, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetObjectAsync(_bucket, path, ct);
            return response.ResponseStream;
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (AmazonS3NotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        try
        {
            await _client.DeleteObjectAsync(_bucket, path, ct);
        }
        catch (AmazonS3NotFoundException)
        {
            _logger.LogWarning("S3 object {Key} was already absent during delete.", path);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(name))
            name = "file";
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}

/// <summary>Thrown by the AWS SDK when a key does not exist (SDK version-dependent exception type).</summary>
public sealed class AmazonS3NotFoundException : Exception
{
    public AmazonS3NotFoundException(string message) : base(message) { }
}