using ControlEasyReborn.SharedKernel.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControlEasyReborn.Infrastructure.Storage;

public static class StorageHostingExtensions
{
    public static IServiceCollection AddControlEasyStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IStorageProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var provider = (options.Provider ?? "Local").Trim();

            return provider.ToUpperInvariant() switch
            {
                "LOCAL" => (IStorageProvider)new LocalFilesystemStorageProvider(Options.Create(options)),
                "S3" => new S3StorageProvider(Options.Create(options), sp.GetRequiredService<ILoggerFactory>()),
                _ => throw new InvalidOperationException($"Unsupported storage provider: {provider}. Expected 'Local' or 'S3'.")
            };
        });

        // Throw-on-missing at startup for the selected provider, mirroring AddControlEasyDbTools.
        ValidateStorageConfigurationOrThrow(configuration);

        return services;
    }

    private static void ValidateStorageConfigurationOrThrow(IConfiguration configuration)
    {
        var provider = (configuration["Storage:Provider"] ?? "Local").Trim();

        switch (provider.ToUpperInvariant())
        {
            case "LOCAL":
                if (string.IsNullOrWhiteSpace(configuration["Storage:Local:Path"]))
                    throw new InvalidOperationException("Storage:Local:Path is not configured.");
                break;
            case "S3":
                if (string.IsNullOrWhiteSpace(configuration["Storage:S3:Endpoint"]))
                    throw new InvalidOperationException("Storage:S3:Endpoint is not configured.");
                if (string.IsNullOrWhiteSpace(configuration["Storage:S3:Bucket"]))
                    throw new InvalidOperationException("Storage:S3:Bucket is not configured.");
                if (string.IsNullOrWhiteSpace(configuration["Storage:S3:AccessKey"]))
                    throw new InvalidOperationException("Storage:S3:AccessKey is not configured.");
                if (string.IsNullOrWhiteSpace(configuration["Storage:S3:SecretKey"]))
                    throw new InvalidOperationException("Storage:S3:SecretKey is not configured.");
                break;
            default:
                throw new InvalidOperationException($"Unsupported storage provider: {provider}. Expected 'Local' or 'S3'.");
        }
    }
}