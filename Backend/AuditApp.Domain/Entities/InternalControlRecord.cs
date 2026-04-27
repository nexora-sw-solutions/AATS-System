namespace AuditApp.Domain.Entities;

public class InternalControlRecord : AuditBaseEntity
{
    public string? Assignment { get; set; }
    public string? PeriodNumber { get; set; }
    public string? PeriodType { get; set; }
}
