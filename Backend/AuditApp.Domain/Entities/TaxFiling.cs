using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class TaxFiling : BaseEntity, ISoftDeletable
{
    public string FilingCode { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty; // CIT, IIT, VAT, SSCL, WHT, Others
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public DateOnly FilingDate { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public string? PeriodNumber { get; set; }
    public string? PeriodType { get; set; } // Date, Month, Year
    public string? PaymentStatus { get; set; } // Paid, Pending, IRD Paid
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Client? Client { get; set; }
    public Branch? Branch { get; set; }
}
