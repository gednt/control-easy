using ControlEasyReborn.SharedKernel.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControlEasyReborn.Infrastructure.Storage;

/// <summary>
/// S3-compatible storage provider (works with AWS S3 and MinIO).
/// Uses the AWS SDK's S3 client configured against a custom endpoint.
/// Correctness is verified via unit tests against the abstraction; live bring-up
/// is validated by compose (Phase 11 design note).
/// </summary>
public sealed class S3StorageProvider : IStorageProvider
{
    private readonly IStorageProvider _inner;

    public S3StorageProvider(IOptions<StorageOptions> options, ILoggerFactory loggerFactory)
    {
        var s3 = options.Value.S3;
        if (string.IsNullOrWhiteSpace(s3.Endpoint))
            throw new InvalidOperationException("Storage:S3:Endpoint is not configured.");
        if (string.IsNullOrWhiteSpace(s3.Bucket))
            throw new InvalidOperationException("Storage:S3:Bucket is not configured.");
        if (string.IsNullOrWhiteSpace(s3.AccessKey))
            throw new InvalidOperationException("Storage:S3:AccessKey is not configured.");
        if (string.IsNullOrWhiteSpace(s3.SecretKey))
            throw new InvalidOperationException("Storage:S3:SecretKey is not configured.");

        _inner = CreateAmazonS3BackedProvider(s3, loggerFactory);
    }

    public Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct) =>
        _inner.SaveAsync(content, fileName, ct);

    public Task<Stream?> OpenReadAsync(string path, CancellationToken ct) =>
        _inner.OpenReadAsync(path, ct);

    public Task DeleteAsync(string path, CancellationToken ct) =>
        _inner.DeleteAsync(path, ct);

    private static IStorageProvider CreateAmazonS3BackedProvider(S3StorageOptions s3, ILoggerFactory loggerFactory)
    {
        var amazonS3Config = new Amazon.S3.AmazonS3Config
        {
            ServiceURL = s3.Endpoint,
            ForcePathStyle = true
        };
        var amazonS3Client = new Amazon.S3.AmazonS3Client(s3.AccessKey, s3.SecretKey, amazonS3Config);
        return new AmazonS3StorageProvider(amazonS3Client, s3.Bucket, loggerFactory.CreateLogger<AmazonS3StorageProvider>());
    }
}