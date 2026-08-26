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
        [System.Text.Json.Serialization.JsonIgnore]
        public string? ClientStatus { get; set; }


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

        [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [System.Text.Json.Serialization.JsonPropertyName("recordDate")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        private DateTime _date;

        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public DateTime Date
        {
            get
            {
                if (_date != default && _date.Year > 1) return _date;
                if (RecordDate != default && RecordDate.Year > 1) return RecordDate;
                if (CreatedAt != default && CreatedAt.Year > 1) return CreatedAt;
                return DateTime.UtcNow;
            }
            set => _date = value;
        }

        [System.Text.Json.Serialization.JsonPropertyName("taxType")]
        public string? TaxType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("periodNumber")]
        public string? PeriodNumber { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("periodType")]
        public string? PeriodType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("process")]
        public string? Process { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalPayment")]
        public decimal Payment { get; set; }

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonPropertyName("notes")]
        private string? _notes;

        public bool IsPaid => Status == "Paid";
        public bool IsPending => Status == "Pending";
        public bool IsIRDPending => Status == "IRD pending" || Status == "IRD Paid";

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonPropertyName("clientCategory")]
        [NotifyPropertyChangedFor(nameof(ClientCategoryColor))]
        [NotifyPropertyChangedFor(nameof(HasClientCategory))]
        private string? _clientCategory;

        public string ClientCategoryColor => ClientCategory?.ToLower() switch
        {
            "loyal" => "#34D399",
            "active" => "#34D399",
            "blacklisted" => "#94A3B8",
            "black listed" => "#94A3B8",
            "suspend" => "#F87171",
            "suspended" => "#F87171",
            _ => "Transparent"
        };

        public bool HasClientCategory => !string.IsNullOrEmpty(ClientCategory);

        [ObservableProperty]
        [System.Text.Json.Serialization.JsonPropertyName("sourceDocuments")]
        private System.Collections.Generic.List<SourceDocument>? _sourceDocuments;
    }
}
