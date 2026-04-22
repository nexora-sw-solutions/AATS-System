using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;

namespace AuditApp.Application.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentResponse>> GetDocumentsByReferenceAsync(Guid referenceId, string referenceType, CancellationToken ct = default);
    Task<DocumentResponse> UploadDocumentAsync(UploadDocumentRequest request, Stream fileStream, string contentType, CancellationToken ct = default);
    Task<string> GetDocumentDownloadUrlAsync(Guid id, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid id, CancellationToken ct = default);
}
