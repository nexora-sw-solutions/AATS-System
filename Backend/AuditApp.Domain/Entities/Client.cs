using AuditApp.Domain.Common;
using AuditApp.Domain.Enums;

namespace AuditApp.Domain.Entities;

public class Client : BaseEntity, ISoftDeletable
{
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    public Guid? BranchId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? LogoStorageKey { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Branch? Branch { get; set; }
}
