namespace AuditApp.Domain.Entities;

public class BoiRegistration : SecretarialBaseEntity
{
    public string? BoiCode { get; set; }
    public string? Assignment { get; set; }
    public string? Country { get; set; }
    public string? CountryAddress { get; set; }
    public decimal? InvestmentValueUsd { get; set; }
    public string? Status { get; set; }
}
