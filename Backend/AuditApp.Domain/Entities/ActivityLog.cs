namespace AuditApp.Domain.Entities;

public class ActivityLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public Guid? BranchId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? RecordType { get; set; }
    public Guid? RecordId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
