using System;

namespace AATS.Domain.Entities
{
    public class SourceDocument : BaseEntity
    {
        public Guid RecordId { get; set; }
        public string RecordType { get; set; } = "Audit"; // Audit, Client, Secretarial, Tax

        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public string? FileType { get; set; }
        public string? AttachmentCategory { get; set; } // BR, TIN, Form01, ArticleOfAssociation, NIC, Form05, etc.

        public Guid? UploaderId { get; set; }
        public string? UploaderName { get; set; }
    }
}
