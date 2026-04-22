using AuditApp.Domain.Enums;

namespace AuditApp.Application.DTOs.Tax;

public class TaxFilingResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateOnly Period { get; set; }
    public string? PeriodNumber { get; set; }
    public string? Process { get; set; }
    public string? Payment { get; set; }
    public PeriodType? PeriodType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTaxFilingRequest
{
    public string ClientName { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateOnly Period { get; set; }
    public string? Process { get; set; }
    public string? Payment { get; set; }
    public PeriodType? PeriodType { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Description { get; set; }
}

public class UpdateTaxFilingRequest
{
    public string? ClientName { get; set; }
    public TaxType? TaxType { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateOnly? Period { get; set; }
    public string? Process { get; set; }
    public string? Payment { get; set; }
    public PeriodType? PeriodType { get; set; }
    public string? PeriodNumber { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}
