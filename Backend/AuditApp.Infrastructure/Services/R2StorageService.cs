using Amazon.S3;
using Amazon.S3.Model;
using AuditApp.Application.Common;
using Microsoft.Extensions.Configuration;

namespace AuditApp.Infrastructure.Services;

public class R2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public R2StorageService(IConfiguration configuration)
    {
        var r2Config = configuration.GetSection("CloudflareR2");
        _bucketName = r2Config["BucketName"] ?? "aats";

        var config = new AmazonS3Config
        {
            ServiceURL = r2Config["ServiceURL"],
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(
            r2Config["AccessKey"],
            r2Config["SecretKey"],
            config
        );
    }

    public async Task<string> UploadAsync(string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    public string GetPreSignedUrl(string key, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Verb = HttpVerb.GET
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
