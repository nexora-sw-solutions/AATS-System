using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class TaxRecord : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? ID { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("recordCode")]
        public string? Code { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientId")]
        public Guid? ClientId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("branchId")]
        public Guid? BranchId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientCode")]
        public string? ClientCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientName")]
        public string? ClientName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientNameSub")]
        public string? ClientNameSub { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("directorId")]
        public string? DINNo { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("tin")]
        public string? TIN { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("period")]
        public string? TaxPeriod { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("branchName")]
        public string? Branch { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdBy")]
        public Guid? CreatedBy { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdByName")]
        public string? CreatedByName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("taxType")]
        public string? TaxType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("periodNumber")]
        public string? PeriodNumber { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("periodType")]
        public string? PeriodType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("process")]
        public string? Process { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalPayment")]
        public string? Payment { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("notes")]
        public string? Description { get; set; }

        [ObservableProperty]
        private string? _notes;

        public bool IsPaid => Status == "Paid";
        public bool IsPending => Status == "Pending";
        public bool IsIRDPending => Status == "IRD pending";

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonPropertyName("clientCategory")]
        [NotifyPropertyChangedFor(nameof(ClientCategoryColor))]
        [NotifyPropertyChangedFor(nameof(HasClientCategory))]
        private string? _clientCategory;

        public string ClientCategoryColor => ClientCategory?.ToLower() switch
        {
            "loyal" => "#75de33",      // Custom Green
            "blacklisted" => "#696b68", // Custom Gray
            "suspend" => "#c96363",     // Custom Red
            "suspended" => "#c96363",    // Alias
            _ => "Transparent"
        };

        public bool HasClientCategory => !string.IsNullOrEmpty(ClientCategory);
    }
}
