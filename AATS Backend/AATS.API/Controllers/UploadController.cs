using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AATS.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AATS.API.Controllers
{
    public class UploadDocumentsRequest
    {
        public List<IFormFile> Files { get; set; } = new();
        public string RecordType { get; set; } = "General";
        public string RecordId { get; set; } = "";
    }

    public class UploadLogoRequest
    {
        public IFormFile? File { get; set; }
        public string ClientId { get; set; } = "";
    }

    /// <summary>
    /// Handles multipart file uploads to Cloudflare R2.
    /// POST /api/upload/documents  → uploads source documents
    /// POST /api/upload/logo       → uploads a client profile picture
    /// </summary>
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly R2StorageService _r2;

        public UploadController(R2StorageService r2)
        {
            _r2 = r2;
        }

        // ─── Connectivity Test (no auth) ─────────────────────────────────────

        /// <summary>
        /// Uploads a tiny test file to R2. Used to verify R2 credentials and bucket access.
        /// GET /api/upload/test
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestR2Connection()
        {
            try
            {
                var testContent = System.Text.Encoding.UTF8.GetBytes($"R2 connectivity test at {DateTime.UtcNow:O}");
                using var stream = new MemoryStream(testContent);
                var result = await _r2.UploadAsync(stream, "r2-test.txt", "text/plain", "connectivity-tests");
                return Ok(new { success = true, url = result.Url, message = "R2 upload succeeded" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Accepts one or more files in a multipart/form-data body.
        /// Returns a JSON array of { fileName, url, fileSize, description }.
        /// </summary>
        [HttpPost("documents")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)] // 100 MB max total
        public async Task<IActionResult> UploadDocuments(
            [FromForm(Name = "files")] List<IFormFile>? files,
            [FromForm(Name = "recordType")] string? recordType = "General",
            [FromForm(Name = "recordId")] string? recordId = "")
        {
            var inputFiles = files;
            if ((inputFiles == null || inputFiles.Count == 0) && Request.HasFormContentType && Request.Form.Files.Count > 0)
            {
                inputFiles = Request.Form.Files.ToList();
            }

            if (inputFiles == null || inputFiles.Count == 0)
                return BadRequest(new { error = "No files provided." });

            var typeStr = recordType;
            if (string.IsNullOrWhiteSpace(typeStr) && Request.HasFormContentType && Request.Form.ContainsKey("recordType"))
            {
                typeStr = Request.Form["recordType"].ToString();
            }

            // Resolve R2 target folder path based on recordType
            string folder;
            var trimmedType = (typeStr ?? "General").Trim();
            if (string.Equals(trimmedType, "Nexora", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Nexora";
            }
            else if (string.Equals(trimmedType, "Secretarial", StringComparison.OrdinalIgnoreCase) || string.Equals(trimmedType, "Secretarial & Advisory", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory";
            }
            else if (string.Equals(trimmedType, "NIC", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory/NIC";
            }
            else if (string.Equals(trimmedType, "BR", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory/BR";
            }
            else if (string.Equals(trimmedType, "R1", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory/R1";
            }
            else if (string.Equals(trimmedType, "ART", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory/ART";
            }
            else if (string.Equals(trimmedType, "Staff NIC", StringComparison.OrdinalIgnoreCase) || string.Equals(trimmedType, "StaffNIC", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Secretarial & Advisory/Staff NIC";
            }
            else
            {
                folder = $"{_r2.SourceDocsFolder}/{trimmedType.Replace(" ", "-").ToLower()}";
            }

            var results = new List<object>();

            foreach (var file in inputFiles)
            {
                if (file.Length == 0) continue;

                await using var stream = file.OpenReadStream();
                var result = await _r2.UploadAsync(
                    stream,
                    file.FileName,
                    file.ContentType ?? "application/octet-stream",
                    folder);

                results.Add(new
                {
                    fileName    = result.FileName,
                    url         = result.Url,
                    fileSize    = result.FileSize,
                    description = result.Description
                });
            }

            return Ok(results);
        }

        // ─── Client Logo / Profile Picture ──────────────────────────────────

        /// <summary>
        /// Accepts a single image file for the client logo.
        /// Returns { url }.
        /// </summary>
        [HttpPost("logo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)] // 10 MB max
        public async Task<IActionResult> UploadLogo(
            [FromForm(Name = "file")] IFormFile? file,
            [FromForm(Name = "clientId")] string? clientId = "")
        {
            var inputFile = file;
            if (inputFile == null && Request.HasFormContentType && Request.Form.Files.Count > 0)
            {
                inputFile = Request.Form.Files[0];
            }

            if (inputFile == null || inputFile.Length == 0)
                return BadRequest(new { error = "No file provided." });

            var folder = "profile-pictures";
            await using var stream = inputFile.OpenReadStream();
            var result = await _r2.UploadAsync(
                stream,
                inputFile.FileName,
                inputFile.ContentType ?? "image/jpeg",
                folder);

            return Ok(new { url = result.Url });
        }
    }
}
