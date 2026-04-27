using AuditApp.Domain.Enums;

namespace AuditApp.Domain.Entities;

public class SyncTracking
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public SyncOperation Operation { get; set; }
    public DateTime? SyncedAt { get; set; }
    public string? DeviceId { get; set; }
    public bool ConflictResolved { get; set; }
    public DateTime CreatedAt { get; set; }
}
