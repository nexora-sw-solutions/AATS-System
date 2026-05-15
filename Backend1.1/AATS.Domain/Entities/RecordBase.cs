using System;

namespace AATS.Domain.Entities
{
    public abstract class RecordBase : BaseEntity
    {
        public string RecordCode { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid? ClientId { get; set; }
        public Client? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientCode { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string? BranchName { get; set; }
        
        public string? Status { get; set; }
        public string? Process { get; set; }
        public int CurrentStep { get; set; }
        public string? Period { get; set; }
        
        // Financials
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal PartialAmount { get; set; }
        public string? PaymentOption { get; set; }
        public string? PaymentStatus { get; set; }
        
        // Additional Info
        public string? Notes { get; set; }
        public int NoOfStaffs { get; set; }
        public string? Country { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column("t_i_n")]
        public string? TIN { get; set; }
        public string? DirectorID { get; set; }
        public string? InvestmentValue { get; set; }
        public string? CountryAddress { get; set; }

        // Transient Cheque Properties
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ChequeBank { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ChequeNumber { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime? ChequeDate { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public decimal? ChequeAmount { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [System.Text.Json.Serialization.JsonPropertyName("chequeStatus")]
        public string? ChequeStatus { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? CreatedByName { get; set; }
    }
}
