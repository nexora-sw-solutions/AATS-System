using AuditApp.Domain.Enums;

namespace AuditApp.Application.DTOs.Shared;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? Date { get; set; }
    public PaymentOption? Option { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Description { get; set; }
    public List<ChequeDetailResponse> Cheques { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ChequeDetailResponse
{
    public Guid Id { get; set; }
    public string ChequeNumber { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public ChequeStatus Status { get; set; }
}

public class CreatePaymentRequest
{
    public Guid? ClientId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public PaymentOption Option { get; set; }
    public Guid? ReferenceId { get; set; } // UUID of the record being paid
    public string? ReferenceType { get; set; } // e.g., "TaxFiling", "AuditRecord"
    public string? Description { get; set; }
    public List<CreateChequeRequest> Cheques { get; set; } = new();
}

public class CreateChequeRequest
{
    public string ChequeNumber { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public ChequeStatus Status { get; set; } = ChequeStatus.Pending;
}

// Nexora
public class NexoraServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}

public class NexoraRequestResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ServiceName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNexoraRequest
{
    public Guid ServiceId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Message { get; set; }
}
