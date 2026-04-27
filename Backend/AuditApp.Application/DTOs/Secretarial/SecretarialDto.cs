namespace AuditApp.Application.DTOs.Secretarial;

public class CompanyRegistrationResponse
{
    public Guid Id { get; set; }
    public string RegistrationCode { get; set; } = string.Empty;
    public DateOnly RegistrationDate { get; set; }
    public Guid? ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal PartialAmount { get; set; }
    public string? PaymentOption { get; set; }
    public Guid? BranchId { get; set; }
    public List<CompanyOfficerResponse> Officers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CompanyOfficerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? OfficerType { get; set; }
    public decimal? SharePercentage { get; set; }
    public string? NicNumber { get; set; }
}

public class CreateCompanyRegistrationRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? ClientName { get; set; }
    public Guid? ClientId { get; set; }
    public DateOnly? Date { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal PartialAmount { get; set; }
    public string? PaymentOption { get; set; }
    public Guid? BranchId { get; set; }
    public List<CreateCompanyOfficerRequest> Officers { get; set; } = new();
}

public class CreateCompanyOfficerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? OfficerType { get; set; }
    public decimal? SharePercentage { get; set; }
    public string? NicNumber { get; set; }
}

// EPF/ETF
public class EpfEtfRecordResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StaffCount { get; set; }
    public List<EpfEtfStaffResponse> Staff { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class EpfEtfStaffResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nic { get; set; } = string.Empty;
    public string? EpfNumber { get; set; }
}

public class CreateEpfEtfRecordRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public List<CreateEpfEtfStaffRequest> Staff { get; set; } = new();
}

public class CreateEpfEtfStaffRequest
{
    public string Name { get; set; } = string.Empty;
    public string Nic { get; set; } = string.Empty;
    public string? EpfNumber { get; set; }
}

// Generic Secretarial Response for simple types
public class SecretarialRecordResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateSecretarialRecordRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Description { get; set; }
    public string? Assignment { get; set; }
    public string Status { get; set; } = "Pending";
    
    // BOI specific
    public string? BoiCode { get; set; }
    public string? Country { get; set; }
    public string? CountryAddress { get; set; }
    public decimal? InvestmentValueUsd { get; set; }
}

// ── Update DTOs ──────────────────────────────────────────

public class UpdateCompanyRegistrationRequest
{
    public string? CompanyName { get; set; }
    public string? CompanyType { get; set; }
    public string? ClientName { get; set; }
    public Guid? ClientId { get; set; }
    public DateOnly? Date { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    public string? Process { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPayment { get; set; }
    public decimal? PartialAmount { get; set; }
    public string? PaymentOption { get; set; }
    public Guid? BranchId { get; set; }
}

public class UpdateEpfEtfRecordRequest
{
    public string? ClientName { get; set; }
    public string? Status { get; set; }
    public string? CompanyName { get; set; }
    public int? NumberOfStaff { get; set; }
    public string? Phone { get; set; }
}

public class UpdateSecretarialRecordRequest
{
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? CompanyName { get; set; }
    public string? Assignment { get; set; }
    
    // BOI specific
    public string? BoiCode { get; set; }
    public string? Country { get; set; }
    public string? CountryAddress { get; set; }
    public decimal? InvestmentValueUsd { get; set; }
}

public class UpdateCompanyOfficerRequest
{
    public string? Name { get; set; }
    public string? Position { get; set; }
    public string? OfficerType { get; set; }
    public decimal? SharePercentage { get; set; }
    public string? NicNumber { get; set; }
}

public class UpdateEpfEtfStaffRequest
{
    public string? Name { get; set; }
    public string? Nic { get; set; }
    public string? EpfNumber { get; set; }
}
