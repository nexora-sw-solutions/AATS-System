using AuditApp.Domain.Common;
using AuditApp.Domain.Enums;

namespace AuditApp.Domain.Entities;

public class User : BaseEntity, ISoftDeletable
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UserLogo { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
}
