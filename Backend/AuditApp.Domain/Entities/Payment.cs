using AuditApp.Domain.Common;
using AuditApp.Domain.Enums;

namespace AuditApp.Domain.Entities;

public class Payment : BaseEntity
{
    public string RecordType { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public PaymentOption? PaymentOption { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public List<ChequeDetail> ChequeDetails { get; set; } = new();
}
