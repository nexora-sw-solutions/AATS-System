namespace AuditApp.Domain.Entities;

public class TradeLicense : SecretarialBaseEntity
{
    public string? LicenseCode { get; set; }
    public string? Assignment { get; set; }
    public string? Status { get; set; }
}
