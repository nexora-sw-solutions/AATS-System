using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class TaxAccountAddRecordViewModel : ViewModelBase
    {
        private readonly AuditRecord? _originalRecord;

        [ObservableProperty] private string _id = string.Empty;
        [ObservableProperty] private DateTime? _date = DateTime.Now;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _assignment = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasClientLogo))]
        private string _clientLogo = string.Empty;

        public bool HasClientLogo => !string.IsNullOrWhiteSpace(ClientLogo);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

        public decimal TotalPayment => SubTotal - Discount;

        [ObservableProperty] private string _paymentOption = "Cash";
        [ObservableProperty] private string _paymentStatus = "Unpaid";

        public bool IsOptionCash { get => PaymentOption == "Cash"; set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
        public bool IsOptionOnline { get => PaymentOption == "Online"; set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
        public bool IsOptionCheque { get => PaymentOption == "Cheque"; set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }

        public bool IsStatusPaid { get => PaymentStatus == "Paid"; set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
        public bool IsStatusUnpaid { get => PaymentStatus == "Unpaid"; set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
        public bool IsStatusPartial { get => PaymentStatus == "Partial"; set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }

        [ObservableProperty] private string _notes = string.Empty;

        // UI State
        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isConfirmSaveVisible = false;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

        public ObservableCollection<string> UploadedFiles { get; } = new();
        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }
        public Func<System.Threading.Tasks.Task<string?>>? RequestLogoPicker { get; set; }
        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        // Default constructor
        public TaxAccountAddRecordViewModel() { }

        // Edit constructor
        public TaxAccountAddRecordViewModel(AuditRecord record)
        {
            _originalRecord = record;
            _id = record.ID ?? string.Empty;
            _date = record.Date;
            _clientName = record.ClientName ?? string.Empty;
            _assignment = record.Assignment ?? string.Empty;
            _paymentOption = record.PaymentOption ?? "Cash";
            _paymentStatus = record.PaymentStatus ?? "Unpaid";
            _notes = record.Notes ?? string.Empty;
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
                : "Are you sure you want to create this new tax account record?";
            IsConfirmSaveVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmSave()
        {
            IsConfirmSaveVisible = false;
            
            if (_originalRecord != null)
            {
                _originalRecord.ID = Id;
                _originalRecord.Date = Date ?? DateTime.Now;
                _originalRecord.ClientName = ClientName;
                _originalRecord.Assignment = Assignment;
                _originalRecord.PaymentOption = PaymentOption;
                _originalRecord.PaymentStatus = PaymentStatus;
                _originalRecord.Notes = Notes;
                
                await DataService.Instance.UpdateAuditRecordAsync("Tax Accountings", _originalRecord);
            }
            else
            {
                var newRecord = new AuditRecord
                {
                    ID = Id,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Assignment = Assignment,
                    PaymentOption = PaymentOption,
                    PaymentStatus = PaymentStatus,
                    Notes = Notes,
                    Process = "BOOKKEEP", // Default for tax account
                    CurrentStep = 1
                };
                
                await DataService.Instance.AddAuditRecordAsync("Tax Accountings", newRecord);
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

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadLogo()
        {
            if (RequestLogoPicker != null)
            {
                var logo = await RequestLogoPicker();
                if (logo != null) ClientLogo = logo;
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadDocument()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (!UploadedFiles.Contains(file))
                            UploadedFiles.Add(file);
                    }
                }
            }
        }

        [RelayCommand]
        private void RemoveFile(string fileName)
        {
            if (UploadedFiles.Contains(fileName))
                UploadedFiles.Remove(fileName);
        }
    }
}


