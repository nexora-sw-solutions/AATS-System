namespace AuditApp.Application.DTOs.Shared;

public class DocumentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}
