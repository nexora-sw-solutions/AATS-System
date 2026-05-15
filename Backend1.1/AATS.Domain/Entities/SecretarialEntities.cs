using System;
using System.Collections.Generic;

namespace AATS.Domain.Entities
{
    public class CompanyRegistration : RecordBase 
    { 
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyType { get; set; }
        public string? Objective { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
        
        // Summary columns for names
        public string? DirectorNames { get; set; }
        public string? SecretaryNames { get; set; }
        public string? ShareholderNames { get; set; }
        public string? OtherNames { get; set; }
        
        public ICollection<CompanyOfficer> Officers { get; set; } = new List<CompanyOfficer>();
    }

    public class CompanyOfficer : BaseEntity
    {
        public Guid CompanyRegistrationId { get; set; }
        public CompanyRegistration? CompanyRegistration { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? OfficerType { get; set; }
        public string? NicNumber { get; set; }
        public decimal? SharePercentage { get; set; }
    }

    public class EpfEtfRecord : RecordBase 
    { 
        public string CompanyName { get; set; } = string.Empty;
        public int NumberOfStaff { get; set; }
        
        public ICollection<EpfEtfStaffMember> StaffMembers { get; set; } = new List<EpfEtfStaffMember>();
    }

    public class EpfEtfStaffMember : BaseEntity
    {
        public Guid EpfEtfRecordId { get; set; }
        public EpfEtfRecord? EpfEtfRecord { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        
        [System.ComponentModel.DataAnnotations.Schema.Column("staff_name")]
        public string Name { get; set; } = string.Empty;
        
        public string? Phone { get; set; }
        public string? ProcessStatus { get; set; }
    }

    public class TradeMark : RecordBase { public string? CompanyName { get; set; } }
    public class TradeLicense : RecordBase { public string? CompanyName { get; set; } public string? Assignment { get; set; } }
    public class ImportExportClearance : RecordBase { public string? Assignment { get; set; } public string? TinNumber { get; set; } }
    public class BoiRegistration : RecordBase { public string? Assignment { get; set; } public decimal? InvestmentValueUsd { get; set; } }
    public class BusinessPlanValuation : RecordBase { public string? Assignment { get; set; } }
    public class HrManagementConsulting : RecordBase { public string? Assignment { get; set; } }
    public class OtherSecretarialRecord : RecordBase { public string? Assignment { get; set; } public string? Description { get; set; } }
}
