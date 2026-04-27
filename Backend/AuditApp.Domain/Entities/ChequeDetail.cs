using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class ChequeDetail : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string? BankName { get; set; }
    public string? ChequeNumber { get; set; }
    public DateOnly? ChequeDate { get; set; }
    public string? Status { get; set; } // Pending, Cleared, Return

    // Navigation properties
    public Payment? Payment { get; set; }
}
