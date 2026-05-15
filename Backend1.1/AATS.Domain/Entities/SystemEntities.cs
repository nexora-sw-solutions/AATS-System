using System;

namespace AATS.Domain.Entities
{
    public class NexoraServiceRequest : BaseEntity
    {
        public string RecordCode { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public Guid? ClientId { get; set; }
        public Client? Client { get; set; }
        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string? CompanyName { get; set; }
        public string? ServiceName { get; set; }
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class Payment : BaseEntity
    {
        public string RecordType { get; set; } = string.Empty;
        public Guid RecordId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }

    public class ChequeDetail : BaseEntity
    {
        public Guid PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public string? BankName { get; set; }
        public string? ChequeNumber { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? Status { get; set; }
    }

    public class Document : BaseEntity
    {
        public string RecordType { get; set; } = string.Empty;
        public Guid RecordId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StorageKey { get; set; } = string.Empty;
        public string? Category { get; set; }
        public Guid? UploaderId { get; set; }
        public User? Uploader { get; set; }
    }

    public class ActivityLog
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public Guid? RecordId { get; set; }
        public string? Description { get; set; }
    }
}
