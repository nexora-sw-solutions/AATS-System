using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class EpfEtfRecord : BaseEntity, ISoftDeletable
{
    public string RecordCode { get; set; } = string.Empty;
    public DateOnly RecordDate { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int NumberOfStaff { get; set; }
    public string? Process { get; set; }
    public string? Phone { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal PartialAmount { get; set; }
    public string? PaymentOption { get; set; }
    public string? PaymentStatus { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Client? Client { get; set; }
    public Branch? Branch { get; set; }
    public List<EpfEtfStaff> StaffMembers { get; set; } = new();
}
