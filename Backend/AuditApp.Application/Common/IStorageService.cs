namespace AuditApp.Application.Common;

public interface IStorageService
{
    Task<string> UploadAsync(string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    string GetPreSignedUrl(string key, int expirationMinutes = 60);
}
