using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class NexoraServiceRequest : BaseEntity, ISoftDeletable
{
    public DateOnly RequestDate { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public Guid? ServiceId { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public Guid? BranchId { get; set; }
    public string Status { get; set; } = "Pending"; // Added as it's indexed in schema
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Client? Client { get; set; }
    public NexoraService? Service { get; set; }
    public Branch? Branch { get; set; }
}
