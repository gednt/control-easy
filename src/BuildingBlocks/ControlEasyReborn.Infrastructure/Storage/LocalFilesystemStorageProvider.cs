using ControlEasyReborn.SharedKernel.Storage;
using Microsoft.Extensions.Options;

namespace ControlEasyReborn.Infrastructure.Storage;

public sealed class LocalFilesystemStorageProvider : IStorageProvider
{
    private readonly string _basePath;

    public LocalFilesystemStorageProvider(IOptions<StorageOptions> options)
    {
        var configured = options.Value.Local.Path;
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("Storage:Local:Path is not configured.");

        _basePath = Path.GetFullPath(configured);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct)
    {
        var safeName = SanitizeFileName(fileName);
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy-MM"), $"{Guid.NewGuid():N}-{safeName}");
        var fullPath = Path.Combine(_basePath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, ct);
        return relativePath.Replace('\\', '/');
    }

    public Task<Stream?> OpenReadAsync(string path, CancellationToken ct)
    {
        var fullPath = ResolveFullPath(path);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteAsync(string path, CancellationToken ct)
    {
        var fullPath = ResolveFullPath(path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private string ResolveFullPath(string path)
    {
        var safeRelative = SanitizeRelativePath(path);
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, safeRelative));
        if (!fullPath.StartsWith(_basePath, StringComparison.Ordinal))
            throw new InvalidOperationException("Storage path escapes the configured root.");
        return fullPath;
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

    private static string SanitizeRelativePath(string path)
    {
        var segments = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var safe = new List<string>();
        foreach (var segment in segments)
        {
            if (segment == "." || segment == "..")
                continue;
            var cleaned = SanitizeFileName(segment);
            safe.Add(cleaned);
        }
        return safe.Count == 0 ? string.Empty : string.Join('/', safe);
    }
}