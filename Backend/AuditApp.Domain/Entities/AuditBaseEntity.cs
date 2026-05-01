using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public abstract class AuditBaseEntity : BaseEntity, ISoftDeletable
{
    public string RecordCode { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? ClientLogo { get; set; }
    public string? PaymentStatus { get; set; } // Paid, Unpaid, Partial
    public string Process { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal PartialAmount { get; set; }
    public string? PaymentOption { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Client? Client { get; set; }
    public Branch? Branch { get; set; }
    public User? Creator { get; set; }
}
