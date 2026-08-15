using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AATS.Domain.Entities
{
    public class AuditRecord : BaseEntity
    {
        public string? RecordCode { get; set; }
        public string Category { get; set; } = string.Empty;

        public Guid? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientCode { get; set; }

        public Guid? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }
        public string? CreatedByName { get; set; }

        public Guid? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }
        public string? BranchName { get; set; }

        public string? CompanyName { get; set; }
        public string? CompanyType { get; set; }

        public string Status { get; set; } = "ACTIVE";
        public string PaymentStatus { get; set; } = "Unpaid";
        public string Process { get; set; } = "DRAFT";
        public int CurrentStep { get; set; } = 1;

        public decimal SubTotal { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal TotalPayment { get; set; } = 0;
        public decimal PartialAmount { get; set; } = 0;
        public string? PaymentOption { get; set; }

        public string? Assignment { get; set; }
        public int NoOfStaffs { get; set; }
        public string? Country { get; set; }
        public string? CountryAddress { get; set; }
        public string? Notes { get; set; }
        public string? Period { get; set; }
        public string? Tin { get; set; }
        public string? DirectorId { get; set; }
        public string? InvestmentValue { get; set; }
        public decimal? InvestmentValueUsd { get; set; }
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }

        public string? ChequeBank { get; set; }
        public string? ChequeNumber { get; set; }
        public DateTime? ChequeDate { get; set; }
        public decimal? ChequeAmount { get; set; }
        public string? ChequeStatus { get; set; }

        public string? LoginId { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Objective { get; set; }
        public string? Description { get; set; }

        public string? BoResponsiblePersonName { get; set; }
        public string? BoResponsiblePersonNicFileName { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        // Navigation Collections
        public ICollection<CompanyOfficer> Officers { get; set; } = new List<CompanyOfficer>();
        public ICollection<StaffMember> StaffMembers { get; set; } = new List<StaffMember>();

        [NotMapped]
        public List<SourceDocument> SourceDocuments { get; set; } = new List<SourceDocument>();
    }
}
