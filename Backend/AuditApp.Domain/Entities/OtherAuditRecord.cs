namespace AuditApp.Domain.Entities;

public class OtherAuditRecord : AuditBaseEntity
{
    public string? Assignment { get; set; }
    public string? Description { get; set; }
    public string? Company { get; set; }
}
