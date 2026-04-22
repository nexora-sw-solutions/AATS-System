using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.TaxFiling
{
    public partial class TaxOthersAddRecordViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _clientId = "CL-0001";

        [ObservableProperty]
        private DateTime? _date = DateTime.Now;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _company = string.Empty;

        [ObservableProperty]
        private string _assignment = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

        public decimal TotalPayment => SubTotal - Discount;

        [ObservableProperty]
        private string _paymentOption = "Cash";

        public bool IsOptionCash
        {
            get => PaymentOption == "Cash";
            set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        public bool IsOptionOnline
        {
            get => PaymentOption == "Online";
            set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        public bool IsOptionCheque
        {
            get => PaymentOption == "Cheque";
            set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        [ObservableProperty]
        private string _paymentStatus = "Unpaid";

        public bool IsStatusPaid
        {
            get => PaymentStatus == "Paid";
            set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        public bool IsStatusUnpaid
        {
            get => PaymentStatus == "Unpaid";
            set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        public bool IsStatusPartial
        {
            get => PaymentStatus == "Partial";
            set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        private readonly AuditRecord? _originalRecord;

        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isConfirmSaveVisible = false;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        public TaxOthersAddRecordViewModel() { }

        public TaxOthersAddRecordViewModel(AuditRecord record)
        {
            _originalRecord = record;
            _clientId = record.ID ?? "CL-0001";
            _date = record.Date;
            _clientName = record.ClientName ?? string.Empty;
            _company = record.Company ?? string.Empty;
            _assignment = record.Assignment ?? string.Empty;
            _paymentOption = record.PaymentOption ?? "Cash";
            _paymentStatus = record.PaymentStatus ?? "Unpaid";
        }

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

        [RelayCommand]
        private void SaveRecord()
        {
            ConfirmSaveTitle = _originalRecord != null ? "Save Changes?" : "Save Record?";
            ConfirmSaveMessage = _originalRecord != null 
                ? "Are you sure you want to save the modifications to this record?" 
                : "Are you sure you want to create this new tax record?";
            IsConfirmSaveVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmSave()
        {
            IsConfirmSaveVisible = false;
            
            if (_originalRecord != null)
            {
                _originalRecord.ID = ClientId;
                _originalRecord.Date = Date ?? DateTime.Now;
                _originalRecord.ClientName = ClientName;
                _originalRecord.Company = Company;
                _originalRecord.Assignment = Assignment;
                _originalRecord.PaymentOption = PaymentOption;
                _originalRecord.PaymentStatus = PaymentStatus;
                
                await DataService.Instance.UpdateAuditRecordAsync("Tax Others", _originalRecord);
            }
            else
            {
                var newRecord = new AuditRecord
                {
                    ID = ClientId,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Company = Company,
                    Assignment = Assignment,
                    PaymentOption = PaymentOption,
                    PaymentStatus = PaymentStatus
                };
                
                await DataService.Instance.AddAuditRecordAsync("Tax Others", newRecord);
            }

            if (GoBack != null) await GoBack();
        }

        [RelayCommand]
        private void CancelSave() => IsConfirmSaveVisible = false;

        [RelayCommand]
        private async System.Threading.Tasks.Task DiscardChanges()
        {
            IsConfirmSaveVisible = false;
            if (GoBack != null) await GoBack();
        }
    }
}


