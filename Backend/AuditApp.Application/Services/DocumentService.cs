using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DocumentService(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<DocumentResponse>> GetDocumentsByReferenceAsync(Guid referenceId, string referenceType, CancellationToken ct = default)
    {
        var documents = await _db.Documents
            .Where(d => d.RecordId == referenceId && d.RecordType == referenceType)
            .AsNoTracking()
            .ToListAsync(ct);

        return documents.Select(d => MapToResponse(d)).ToList();
    }

    public async Task<DocumentResponse> UploadDocumentAsync(UploadDocumentRequest request, Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var key = $"documents/{Guid.NewGuid()}-{request.FileName}";
        await _storage.UploadAsync(key, fileStream, contentType, ct);

        var document = new Document
        {
            FileName = request.FileName,
            StorageKey = key,
            MimeType = contentType,
            FileSize = fileStream.Length.ToString(),
            RecordId = request.ReferenceId,
            RecordType = request.ReferenceType,
            UploadedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(document);
    }

    public async Task<string> GetDocumentDownloadUrlAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _db.Documents.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Document not found.");

        return _storage.GetPreSignedUrl(document.StorageKey);
    }

    public async Task DeleteDocumentAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _db.Documents.FindAsync([id], ct);
        if (document != null)
        {
            await _storage.DeleteAsync(document.StorageKey, ct);
            _db.Documents.Remove(document);
            await _db.SaveChangesAsync(ct);
        }
    }

    private DocumentResponse MapToResponse(Document d) => new()
    {
        Id = d.Id,
        FileName = d.FileName,
        FilePath = d.StorageKey,
        FileType = d.MimeType ?? string.Empty,
        FileSize = long.TryParse(d.FileSize, out var size) ? size : 0,
        ReferenceId = d.RecordId,
        ReferenceType = d.RecordType,
        DownloadUrl = _storage.GetPreSignedUrl(d.StorageKey),
        CreatedAt = d.CreatedAt
    };
}
