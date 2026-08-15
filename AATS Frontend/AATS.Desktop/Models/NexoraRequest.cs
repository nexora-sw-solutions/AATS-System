using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class NexoraRequest : ObservableObject
    {
        [property: System.Text.Json.Serialization.JsonPropertyName("id")]
        [ObservableProperty] private Guid _dbId;

        [property: System.Text.Json.Serialization.JsonPropertyName("clientId")]
        [ObservableProperty] private Guid? _clientId;
        [ObservableProperty] private string _clientStatus = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("branchId")]
        [ObservableProperty] private Guid? _branchId;

        [property: System.Text.Json.Serialization.JsonPropertyName("recordCode")]
        [ObservableProperty] private string _id = string.Empty;
        [property: System.Text.Json.Serialization.JsonPropertyName("date")]
        [ObservableProperty] private DateTime _date = DateTime.Now;

        [property: System.Text.Json.Serialization.JsonPropertyName("clientFirstName")]
        [ObservableProperty] private string _clientFirstName = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("clientLastName")]
        [ObservableProperty] private string _clientLastName = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("companyName")]
        [ObservableProperty] private string _companyName = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("serviceName")]
        [ObservableProperty] private string _service = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("phone")]
        [ObservableProperty] private string _phone = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("notes")]
        [ObservableProperty] private string _notes = string.Empty;

        [property: System.Text.Json.Serialization.JsonPropertyName("status")]
        [ObservableProperty] private string _status = "Pending";
        [ObservableProperty] private bool _isSelected;

        public string ClientFullName => $"{ClientFirstName} {ClientLastName}";
        public string ServiceIcon => Service switch
        {
            "Accounting Software" => "fa-solid fa-calculator",
            "Website" => "fa-solid fa-globe",
            "POS System" => "fa-solid fa-cash-register",
            "Payroll Management" => "fa-solid fa-file-invoice-dollar",
            "KOT System" => "fa-solid fa-utensils",
            _ => "fa-solid fa-gear"
        };
    }
}
