using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AATS.Domain.Entities
{
    public enum UserRole
    {
        Admin = 1,
        Staff = 2,
        Manager = 3
    }

    public enum ClientStatus
    {
        Active = 1,
        Inactive = 2,
        Archived = 3
    }

    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
        [JsonIgnore]
        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }

    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        [NotMapped]
        public string? Password { get; set; }

        [NotMapped]
        public string? CurrentPassword { get; set; }
        public UserRole Role { get; set; }
        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string? BranchName => Branch?.Name;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
    }

    public class Client : BaseEntity
    {
        public string ClientCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public ClientStatus Status { get; set; } = ClientStatus.Active;
        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }
        public decimal TotalRevenue { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0;
        public string? LogoStorageKey { get; set; }
    }
}
