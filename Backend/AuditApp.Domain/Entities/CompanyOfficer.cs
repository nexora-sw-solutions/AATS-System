using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class CompanyOfficer : BaseEntity
{
    public Guid CompanyRegistrationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string OfficerType { get; set; } = string.Empty; // director, secretary, alternate_director, shareholder, other
    public decimal? SharePercentage { get; set; }
    public string? NicNumber { get; set; }

    // Navigation properties
    public CompanyRegistration CompanyRegistration { get; set; } = null!;
}
