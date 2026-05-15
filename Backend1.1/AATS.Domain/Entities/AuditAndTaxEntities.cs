using System;

namespace AATS.Domain.Entities
{
    // Auditing
    public class AuditAssuranceRecord : RecordBase { public string? Assignment { get; set; } }
    
    public class ForensicAuditRecord : RecordBase 
    { 
        public string? Assignment { get; set; } 
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
    }
    
    public class InternalAuditRecord : RecordBase 
    { 
        public string? Assignment { get; set; } 
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
    }
    
    public class InternalControlRecord : RecordBase 
    { 
        public string? Assignment { get; set; } 
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
    }
    
    public class ManagementAccountRecord : RecordBase { public string? Assignment { get; set; } }
    
    public class OtherAuditRecord : RecordBase 
    { 
        public string? Assignment { get; set; } 
        public string? Description { get; set; } 
    }

    // Taxing
    public class TaxAccountRecord : RecordBase 
    { 
        public string? Assignment { get; set; } 
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
    }
    
    public class TaxFiling : RecordBase 
    { 
        public string? TaxType { get; set; }
        public string? TaxNumber { get; set; }
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
    }

    public class VatFiling : TaxFiling
    {
        public decimal TaxableTurnover { get; set; }
        public decimal ExemptTurnover { get; set; }
        public decimal OutputVat { get; set; }
        public decimal InputVat { get; set; }
        public decimal NetVatPayable { get; set; }
    }

    public class CitFiling : TaxFiling
    {
        public decimal NetProfit { get; set; }
        public decimal DisallowableExpenses { get; set; }
        public decimal AllowableExpenses { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxPayable { get; set; }
    }

    public class IitFiling : TaxFiling
    {
        public decimal EmploymentIncome { get; set; }
        public decimal BusinessIncome { get; set; }
        public decimal InvestmentIncome { get; set; }
        public decimal TotalStatutoryIncome { get; set; }
        public decimal TaxReliefs { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal TaxPayable { get; set; }
    }

    public class SsclFiling : TaxFiling
    {
        public decimal TaxableTurnover { get; set; }
        public decimal SsclRate { get; set; }
        public decimal SsclPayable { get; set; }
    }

    public class WhtFiling : TaxFiling
    {
        public decimal PaymentSubjectToWht { get; set; }
        public decimal WhtRate { get; set; }
        public decimal WhtPayable { get; set; }
    }
}
