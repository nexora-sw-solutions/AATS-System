using System;

namespace AATS.Domain.Entities
{
    public class NexoraRequest : BaseEntity
    {
        public Guid? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ServiceType { get; set; }
        public string? Details { get; set; }
        public string Status { get; set; } = "PENDING";
    }

    public class ActivityLog
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AppNotification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}
