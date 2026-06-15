namespace ClaimsModule.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid organisationId,
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<string> GetDownloadUrlAsync(
        string blobPath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default);
}