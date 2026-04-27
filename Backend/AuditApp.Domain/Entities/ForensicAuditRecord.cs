namespace AuditApp.Domain.Entities;

public class ForensicAuditRecord : AuditBaseEntity
{
    public string? Assignment { get; set; }
    public string? PeriodNumber { get; set; }
    public string? PeriodType { get; set; }
}
