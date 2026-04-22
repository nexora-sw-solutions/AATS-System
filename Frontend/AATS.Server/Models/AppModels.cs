using System;
using System.Collections.Generic;

namespace AATS.Server.Models;

public class LoginRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordResetRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LastPassword { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Branch { get; set; }
}

public class ProfileUpdateRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Branch { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

public class TeamMember
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Branch { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClientRecord
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Branch { get; set; }
    public string? Category { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DueAmount { get; set; }
    public string? Status { get; set; }
}

public class NexoraRequest
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public string ClientFirstName { get; set; } = string.Empty;
    public string ClientLastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

public class ActivityLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string User { get; set; } = "Admin User";
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Branch { get; set; } = "Central";
    public string Details { get; set; } = string.Empty;
}

public class AuditRecord
{
    public int CurrentStep { get; set; }
    public string? ID { get; set; }
    public DateTime Date { get; set; }
    public string? ClientName { get; set; }
    public string? Company { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Process { get; set; }
    public string? PaymentOption { get; set; }
    public string? Assignment { get; set; }
    public string? Branch { get; set; }
    public int NoOfStaffs { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
    public string? Period { get; set; }
    public string? TIN { get; set; }
    public string? DirectorID { get; set; }
    public string? InvestmentValue { get; set; }
    public string? CountryAddress { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNo { get; set; }
    public string? Objective { get; set; }
    public string? Description { get; set; }
    public List<CompanyCharacter>? DirectorsList { get; set; }
    public List<CompanyCharacter>? SecretariesList { get; set; }
    public List<CompanyCharacter>? ShareholdersList { get; set; }
    public List<CompanyCharacter>? OthersList { get; set; }
    public List<AppDocument>? RegistrationDocuments { get; set; }
    public List<SourceDocument>? SourceDocuments { get; set; }
    public List<StaffMember>? StaffList { get; set; }
}

public class StaffMember
{
    public string? StaffId { get; set; }
    public string? StaffName { get; set; }
    public string? Phone { get; set; }
    public string? Process { get; set; }
    public List<StaffHistory>? History { get; set; }
}

public class StaffHistory
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}

public class SourceDocument
{
    public string? FileName { get; set; }
    public string? Description { get; set; }
}

public class AppDocument
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string Category { get; set; } = "PROCESS";
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsExisting { get; set; } = true;
    public string ImagePath { get; set; } = string.Empty;
}

public class CompanyCharacter
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public double SharePercentage { get; set; }
    public string? Note { get; set; }
    public string? Detail { get; set; }
}

public class TaxRecord
{
    public string? ID { get; set; }
    public string? ClientName { get; set; }
    public string? ClientNameSub { get; set; }
    public string? DINNo { get; set; }
    public string? TaxPeriod { get; set; }
    public string? Status { get; set; }
    public string? Branch { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}
