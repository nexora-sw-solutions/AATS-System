using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace AATS.Infrastructure.Services
{
    public class R2UploadResult
    {
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Description { get; set; } = "Uploaded document";
    }

    public class R2StorageService
    {
        private readonly AmazonS3Client? _s3Client;
        private readonly string _bucketName;
        private readonly string _sourceDocsFolder;
        private readonly string _publicBaseUrl;
        private readonly bool _isConfigured;

        public R2StorageService(IConfiguration configuration)
        {
            var accountId = configuration["CloudflareR2:AccountId"];
            var accessKey = configuration["CloudflareR2:AccessKey"];
            var secretKey = configuration["CloudflareR2:SecretKey"];

            _bucketName = configuration["CloudflareR2:BucketName"] ?? "aats";
            _sourceDocsFolder = configuration["CloudflareR2:SourceDocsFolder"] ?? "Audit & assurance source docs";

            if (!string.IsNullOrWhiteSpace(accountId) &&
                !string.IsNullOrWhiteSpace(accessKey) &&
                !string.IsNullOrWhiteSpace(secretKey))
            {
                _publicBaseUrl = (configuration["CloudflareR2:PublicBaseUrl"] ?? $"https://{accountId}.r2.cloudflarestorage.com/{_bucketName}").TrimEnd('/');

                try
                {
                    var config = new AmazonS3Config
                    {
                        ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                        ForcePathStyle = true,
                        AuthenticationRegion = "auto"
                    };

                    _s3Client = new AmazonS3Client(accessKey, secretKey, config);
                    _isConfigured = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[R2StorageService Warning] Failed to initialize S3 Client: {ex.Message}");
                    _s3Client = null;
                    _isConfigured = false;
                }
            }
            else
            {
                _publicBaseUrl = "";
                _s3Client = null;
                _isConfigured = false;
                Console.WriteLine("[R2StorageService Warning] Cloudflare R2 credentials not configured. R2 storage features will be disabled.");
            }
        }

        public string SourceDocsFolder => _sourceDocsFolder;
        public bool IsConfigured => _isConfigured;

        /// <summary>
        /// Uploads a stream to R2 under the given object key.
        /// Returns the public URL of the uploaded file.
        /// </summary>
        public async Task<R2UploadResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "")
        {
            if (!_isConfigured || _s3Client == null)
            {
                throw new InvalidOperationException("Cloudflare R2 storage is not configured or disabled on this server.");
            }

            // R2 requires the full content in a seekable MemoryStream with chunk encoding disabled.
            // Read into memory first so we can get the exact length and disable chunked signing.
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }
            var fileSize = fileBytes.Length;

            // Build a unique key to avoid collisions
            var sanitizedName = Path.GetFileNameWithoutExtension(fileName)
                                    .Replace(" ", "_")
                                    .Replace("/", "_")
                                    .Replace("\\", "_");
            var ext = Path.GetExtension(fileName);
            var uniqueKey = string.IsNullOrWhiteSpace(folder)
                ? $"{sanitizedName}_{Guid.NewGuid():N}{ext}"
                : $"{folder.TrimEnd('/')}/{sanitizedName}_{Guid.NewGuid():N}{ext}";

            using var uploadStream = new MemoryStream(fileBytes);

            var request = new PutObjectRequest
            {
                BucketName       = _bucketName,
                Key              = uniqueKey,
                InputStream      = uploadStream,
                ContentType      = contentType,
                // R2 does NOT support STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER.
                // UseChunkEncoding=false forces a standard signed PUT instead.
                UseChunkEncoding = false
            };

            await _s3Client.PutObjectAsync(request);

            return new R2UploadResult
            {
                FileName    = fileName,
                Url         = $"{_publicBaseUrl}/{uniqueKey}",
                FileSize    = fileSize,
                Description = "Uploaded document"
            };
        }

        /// <summary>
        /// Deletes an object from R2 by its full public URL.
        /// </summary>
        public async Task DeleteAsync(string publicUrl)
        {
            if (!_isConfigured || _s3Client == null || string.IsNullOrWhiteSpace(publicUrl)) return;

            try
            {
                // Extract the object key from the URL
                var key = publicUrl.Replace(_publicBaseUrl + "/", "");
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key        = key
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[R2StorageService Warning] Failed to delete object {publicUrl}: {ex.Message}");
            }
        }
    }
}
