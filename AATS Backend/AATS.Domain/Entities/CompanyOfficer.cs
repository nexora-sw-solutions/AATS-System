using System;

namespace AATS.Domain.Entities
{
    public class CompanyOfficer : BaseEntity
    {
        public Guid RecordId { get; set; }
        public AuditRecord? Record { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = "Director"; // Director, Secretary, Shareholder, Other
        public string? NicNumber { get; set; }
    }
}
