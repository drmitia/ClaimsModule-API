using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ClaimsModule.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IOptions<StorageOptions> options,
        ILogger<AzureBlobStorageService> logger)
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
        var blobPath = $"claim-documents/{organisationId}/{claimId}/{fileName}";

        var containerClient = new BlobContainerClient(
            _options.AzureConnectionString,
            _options.AzureContainerName);

        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.UploadAsync(
            fileStream,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        _logger.LogInformation("File uploaded to Azure Blob at {Path}", blobPath);

        return blobPath;
    }

    public Task<string> GetDownloadUrlAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var containerClient = new BlobContainerClient(
            _options.AzureConnectionString,
            _options.AzureContainerName);

        var blobClient = containerClient.GetBlobClient(blobPath);

        var sasUri = blobClient.GenerateSasUri(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddHours(1));

        return Task.FromResult(sasUri.ToString());
    }

    public async Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var containerClient = new BlobContainerClient(
            _options.AzureConnectionString,
            _options.AzureContainerName);

        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("File deleted from Azure Blob at {Path}", blobPath);
    }
}