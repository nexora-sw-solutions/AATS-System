using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AATS.Desktop.Models
{
    public partial class TaxRecord : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public string? ID { get; set; }
        public string? ClientName { get; set; }
        public string? ClientNameSub { get; set; }
        public string? DINNo { get; set; }
        public string? TaxPeriod { get; set; }
        public string? Status { get; set; }
        public string? Branch { get; set; }
        public DateTime Date { get; set; }

        [ObservableProperty]
        private string? _notes;

        public bool IsPaid => Status == "Paid";
        public bool IsPending => Status == "Pending";
        public bool IsIRDPending => Status == "IRD pending";
    }
}
