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
}
