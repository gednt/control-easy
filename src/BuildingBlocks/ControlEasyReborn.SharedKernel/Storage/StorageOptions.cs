namespace ControlEasyReborn.SharedKernel.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>"Local" (default) or "S3" (MinIO-compatible).</summary>
    public string Provider { get; init; } = "Local";

    public LocalStorageOptions Local { get; init; } = new();
    public S3StorageOptions S3 { get; init; } = new();
}

public sealed class LocalStorageOptions
{
    public string Path { get; init; } = string.Empty;
}

public sealed class S3StorageOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}