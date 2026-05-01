using AuditApp.Domain.Common;

namespace AuditApp.Domain.Entities;

public class EpfEtfStaff : BaseEntity
{
    public Guid EpfEtfRecordId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Process { get; set; } = "Submit";

    // Navigation properties
    public EpfEtfRecord? ParentRecord { get; set; }
}
