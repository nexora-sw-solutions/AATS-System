using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class Document : BaseEntity, ISoftDeletable
{
    public string RecordType { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string? DocumentCategory { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public User? Uploader { get; set; }
}
