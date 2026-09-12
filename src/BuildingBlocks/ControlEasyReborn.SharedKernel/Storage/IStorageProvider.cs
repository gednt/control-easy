namespace ControlEasyReborn.SharedKernel.Storage;

public interface IStorageProvider
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string path, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}