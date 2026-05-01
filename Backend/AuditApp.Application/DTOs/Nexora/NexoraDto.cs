using System;

namespace AuditApp.Application.DTOs.Nexora;

public class NexoraServiceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class NexoraServiceRequestResponse
{
    public Guid Id { get; set; }
    public string RecordCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientFirstName { get; set; } = string.Empty;
    public string ClientLastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public Guid? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateNexoraRequestDto
{
    public DateTime Date { get; set; }
    public string ClientFirstName { get; set; } = string.Empty;
    public string ClientLastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? ServiceName { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
}

public class UpdateNexoraRequestDto
{
    public DateTime? Date { get; set; }
    public string? ClientFirstName { get; set; }
    public string? ClientLastName { get; set; }
    public string? CompanyName { get; set; }
    public string? ServiceName { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
}
