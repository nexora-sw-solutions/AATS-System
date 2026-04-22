namespace AuditApp.Domain.Entities;

public class ImportExportClearance : SecretarialBaseEntity
{
    public string? ClearanceCode { get; set; }
    public string? Assignment { get; set; }
    public string? TinNumber { get; set; }
    public string? Status { get; set; }
}
