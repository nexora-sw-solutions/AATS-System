namespace AuditApp.Application.DTOs.Clients;

public class CreateClientRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";
    public Guid? BranchId { get; set; }
}

public class UpdateClientRequest
{
    public string? ClientName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; }
    public Guid? BranchId { get; set; }
}

public class ClientResponse
{
    public Guid Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? LogoStorageKey { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClientRevenueSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int TotalRecords { get; set; }
}
