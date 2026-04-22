using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class NexoraRequest : ObservableObject
    {
        [ObservableProperty] private string _id = string.Empty;
        [ObservableProperty] private DateTime _date = DateTime.Now;
        [ObservableProperty] private string _clientFirstName = string.Empty;
        [ObservableProperty] private string _clientLastName = string.Empty;
        [ObservableProperty] private string _companyName = string.Empty;
        [ObservableProperty] private string _service = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _notes = string.Empty;
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
