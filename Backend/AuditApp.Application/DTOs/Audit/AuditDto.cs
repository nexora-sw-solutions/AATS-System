using AuditApp.Domain.Enums;

namespace AuditApp.Application.DTOs.Audit;

public class AuditRecordResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string? Process { get; set; }
    public Guid? ClientId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? PaymentMode { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal GovFee { get; set; }
    public decimal TotalFee { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentOption? PaymentOption { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAuditRecordRequest
{
    public string ClientName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public Guid? BranchId { get; set; }
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Process { get; set; }
    public Guid? ClientId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? PaymentMode { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal GovFee { get; set; }
    public decimal TotalFee { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentOption? PaymentOption { get; set; }
    public string Status { get; set; } = "Pending";
}

public class UpdateAuditRecordRequest
{
    public string? ClientName { get; set; }
    public DateOnly? Date { get; set; }
    public Guid? BranchId { get; set; }
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Process { get; set; }
    public Guid? ClientId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public PaymentOption? PaymentOption { get; set; }
    public decimal? ServiceFee { get; set; }
    public decimal? TotalFee { get; set; }
    public decimal? PaidAmount { get; set; }
}
