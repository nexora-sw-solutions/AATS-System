namespace AATS.Domain.Entities
{
    public class SourceDocumentDto
    {
        public Guid Id { get; set; }
        public Guid RecordId { get; set; }
        public string? FileName { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public long? FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UploaderId { get; set; }
        public string? UploaderName { get; set; }
        public string? FileType { get; set; }
    }
}
