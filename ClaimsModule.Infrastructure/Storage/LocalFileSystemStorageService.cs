using ClaimsModule.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage;

public class LocalFileSystemStorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly ILogger<LocalFileSystemStorageService> _logger;

    public LocalFileSystemStorageService(
        IOptions<StorageOptions> options,
        ILogger<LocalFileSystemStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid organisationId,
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var relativePath = Path.Combine(
            organisationId.ToString(),
            claimId.ToString(),
            fileName);

        var fullPath = Path.Combine(
            _options.GetResolvedLocalPath(), // ← use resolved path
            relativePath);

        var directory = Path.GetDirectoryName(fullPath)!;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await using var fileOutput = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(fileOutput, cancellationToken);

        return relativePath;
    }

    public Task<string> GetDownloadUrlAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        // For local storage return the relative path as the URL
        var url = $"/files/{blobPath.Replace("\\", "/")}";
        return Task.FromResult(url);
    }

    public Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_options.LocalBasePath, blobPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted at {Path}", fullPath);
        }

        return Task.CompletedTask;
    }
}