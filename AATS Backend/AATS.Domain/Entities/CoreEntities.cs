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
        Manager = 3,
        Tax = 4,
        Audit = 5,
        Secretarial = 6
    }

    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }

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

        public string? Phone { get; set; }
        public UserRole Role { get; set; } = UserRole.Staff;
        public string Status { get; set; } = "Active"; // Active, Inactive
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }

        [NotMapped]
        public string? BranchName => Branch?.Name;

        [NotMapped]
        public bool IsActive
        {
            get => Status != "Inactive";
            set => Status = value ? "Active" : "Inactive";
        }
    }

    public class Client : BaseEntity
    {
        public string ClientCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }

        [NotMapped]
        public string? BranchName => Branch?.Name;

        [NotMapped]
        public Guid? CreatedBy { get; set; }

        public string Category { get; set; } = "Active"; // Active, Black Listed, Suspended, Loyal, Corporate
        public string Status { get; set; } = "Active"; // Active, Inactive
        public decimal TotalRevenue { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0;
        public string? LogoStorageKey { get; set; }
        public string? Notes { get; set; }

        [NotMapped]
        public List<SourceDocument>? BrAttachments { get; set; } = new();

        [NotMapped]
        public List<SourceDocument>? TinAttachments { get; set; } = new();

        [NotMapped]
        public List<SourceDocument>? Form01Attachments { get; set; } = new();

        [NotMapped]
        public List<SourceDocument>? ArticleOfAssociationAttachments { get; set; } = new();

        [NotMapped]
        public List<SourceDocument>? NicAttachments { get; set; } = new();
    }
}
