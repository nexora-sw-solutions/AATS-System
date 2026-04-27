using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class AuditAssuranceAddRecordViewModel : ViewModelBase
    {
        private readonly AuditRecord? _originalRecord;

        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private DateTime? _date = DateTime.Now;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _assignment = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

        public decimal TotalPayment => SubTotal - Discount;

        [ObservableProperty] private string _paymentOption = "Cash";
        [ObservableProperty] private string _paymentStatus = "Paid";
        [ObservableProperty] private string _notes = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasClientLogo))]
        private string _clientLogo = string.Empty;

        public bool HasClientLogo => !string.IsNullOrWhiteSpace(ClientLogo);

        // UI State
        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isConfirmSaveVisible = false;
        [ObservableProperty] private bool _isDiscardConfirmVisible = false;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

        public System.Collections.ObjectModel.ObservableCollection<string> UploadedFiles { get; } = new();

        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }
        public Func<System.Threading.Tasks.Task<string?>>? RequestLogoPicker { get; set; }
        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        // Default constructor for new records
        public AuditAssuranceAddRecordViewModel() { }

        // Constructor for editing existing records (pre-fill fields)
        public AuditAssuranceAddRecordViewModel(AuditRecord record)
        {
            _originalRecord = record;
            _id = record.ID ?? string.Empty;
            _date = record.Date;
            _clientName = record.ClientName ?? string.Empty;
            _assignment = record.Assignment ?? string.Empty;
            _paymentOption = record.PaymentOption ?? "Cash";
            _paymentStatus = record.PaymentStatus ?? "Paid";
            _notes = record.Notes ?? string.Empty;
        }

        // Helper properties for RadioButtons
        public bool IsOptionCash { get => PaymentOption == "Cash"; set { if (value) PaymentOption = "Cash"; } }
        public bool IsOptionOnline { get => PaymentOption == "Online"; set { if (value) PaymentOption = "Online"; } }
        public bool IsOptionCheque { get => PaymentOption == "Cheque"; set { if (value) PaymentOption = "Cheque"; } }

        public bool IsStatusPaid { get => PaymentStatus == "Paid"; set { if (value) PaymentStatus = "Paid"; } }
        public bool IsStatusUnpaid { get => PaymentStatus == "Unpaid"; set { if (value) PaymentStatus = "Unpaid"; } }
        public bool IsStatusPartial { get => PaymentStatus == "Partial"; set { if (value) PaymentStatus = "Partial"; } }

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
                : "Are you sure you want to create this new audit record?";
            IsConfirmSaveVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmSave()
        {
            IsConfirmSaveVisible = false;
            
            if (_originalRecord != null)
            {
                // Update existing record
                _originalRecord.ID = Id;
                _originalRecord.Date = Date ?? DateTime.Now;
                _originalRecord.ClientName = ClientName;
                _originalRecord.Assignment = Assignment;
                _originalRecord.PaymentOption = PaymentOption;
                _originalRecord.PaymentStatus = PaymentStatus;
                _originalRecord.Notes = Notes;
                
                await DataService.Instance.UpdateAuditRecordAsync("Audit & Assurance", _originalRecord);
            }
            else
            {
                // Logic for adding new record
                var newRecord = new AuditRecord
                {
                    ID = Id,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Assignment = Assignment,
                    PaymentOption = PaymentOption,
                    PaymentStatus = PaymentStatus,
                    Notes = Notes,
                    Process = "BOOKKEEP", // Default for new audit
                    CurrentStep = 1
                };
                
                await DataService.Instance.AddAuditRecordAsync("Audit & Assurance", newRecord);
            }

            if (GoBack != null) await GoBack();
        }

        [RelayCommand]
        private void CancelSave()
        {
            IsConfirmSaveVisible = false;
        }

        [RelayCommand]
        private void DiscardChanges()
        {
            IsDiscardConfirmVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmDiscard()
        {
            IsDiscardConfirmVisible = false;
            if (GoBack != null) await GoBack();
        }

        [RelayCommand]
        private void CancelDiscard()
        {
            IsDiscardConfirmVisible = false;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadLogo()
        {
            if (RequestLogoPicker != null)
            {
                var logo = await RequestLogoPicker();
                if (logo != null)
                {
                    ClientLogo = logo;
                }
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


