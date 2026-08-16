using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AATS.Domain.Entities
{
    public class TaxRecord : BaseEntity
    {
        public string? RecordCode { get; set; }
        public string TaxType { get; set; } = string.Empty;

        public Guid? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientNameSub { get; set; }

        public string? DirectorId { get; set; }
        public string? Tin { get; set; }
        public string? Period { get; set; }
        public string? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Paid, IRD pending, IRD Paid
        public string Process { get; set; } = "DRAFT";
        public decimal TotalPayment { get; set; } = 0;

        public Guid? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }
        public string? BranchName { get; set; }

        public Guid? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }
        public string? CreatedByName { get; set; }

        public string? Notes { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public List<SourceDocument> SourceDocuments { get; set; } = new List<SourceDocument>();
    }
}
