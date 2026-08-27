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
        private readonly string _localStoragePath;

        public R2StorageService(IConfiguration configuration)
        {
            var accountId = configuration["CloudflareR2:AccountId"];
            var accessKey = configuration["CloudflareR2:AccessKey"];
            var secretKey = configuration["CloudflareR2:SecretKey"];

            _bucketName = configuration["CloudflareR2:BucketName"] ?? "aats";
            _sourceDocsFolder = configuration["CloudflareR2:SourceDocsFolder"] ?? "Audit & assurance source docs";

            _localStoragePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");

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
                _publicBaseUrl = "/uploads";
                _s3Client = null;
                _isConfigured = false;
                Console.WriteLine("[R2StorageService Warning] Cloudflare R2 credentials not configured. Falling back to local disk storage.");
            }
        }

        public string SourceDocsFolder => _sourceDocsFolder;
        public bool IsConfigured => _isConfigured;

        /// <summary>
        /// Uploads a stream to R2 or local disk storage under the given object key.
        /// Returns the public URL of the uploaded file.
        /// </summary>
        public async Task<R2UploadResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "")
        {
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
            var relativeKey = string.IsNullOrWhiteSpace(folder)
                ? $"{sanitizedName}_{Guid.NewGuid():N}{ext}"
                : $"{folder.TrimEnd('/')}/{sanitizedName}_{Guid.NewGuid():N}{ext}";

            if (_isConfigured && _s3Client != null)
            {
                using var uploadStream = new MemoryStream(fileBytes);
                var request = new PutObjectRequest
                {
                    BucketName       = _bucketName,
                    Key              = relativeKey,
                    InputStream      = uploadStream,
                    ContentType      = contentType,
                    UseChunkEncoding = false
                };

                await _s3Client.PutObjectAsync(request);

                return new R2UploadResult
                {
                    FileName    = fileName,
                    Url         = $"{_publicBaseUrl}/{relativeKey}",
                    FileSize    = fileSize,
                    Description = "Uploaded document"
                };
            }
            else
            {
                // Local disk storage fallback
                var targetFilePath = Path.Combine(_localStoragePath, relativeKey.Replace('/', Path.DirectorySeparatorChar));
                var targetDirectory = Path.GetDirectoryName(targetFilePath);

                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                await File.WriteAllBytesAsync(targetFilePath, fileBytes);

                return new R2UploadResult
                {
                    FileName    = fileName,
                    Url         = $"/uploads/{relativeKey}",
                    FileSize    = fileSize,
                    Description = "Uploaded document (local storage)"
                };
            }
        }

        /// <summary>
        /// Deletes an object from R2 or local disk storage by its full public URL.
        /// </summary>
        public async Task DeleteAsync(string publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl)) return;

            if (_isConfigured && _s3Client != null)
            {
                try
                {
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
            else
            {
                try
                {
                    var relativePath = publicUrl.StartsWith("/uploads/")
                        ? publicUrl.Substring("/uploads/".Length)
                        : publicUrl;
                    var localPath = Path.Combine(_localStoragePath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(localPath))
                    {
                        File.Delete(localPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[R2StorageService Warning] Failed to delete local file {publicUrl}: {ex.Message}");
                }
            }
        }
    }
}
