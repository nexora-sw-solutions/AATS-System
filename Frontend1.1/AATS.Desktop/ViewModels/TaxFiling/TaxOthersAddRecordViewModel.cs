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
        [ObservableProperty] private string _clientId = string.Empty;
        [ObservableProperty] private DateTime? _date = DateTime.Now;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _company = string.Empty;
        [ObservableProperty] private string _assignment = string.Empty;
        [ObservableProperty] private string _description = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

        public decimal TotalPayment => SubTotal - Discount;

        [ObservableProperty] private string _paymentOption = "Cash";

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

        [ObservableProperty] private string _paymentStatus = "Unpaid";

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

        // Cheque Details
        [ObservableProperty] private string _chequeBank = string.Empty;
        [ObservableProperty] private string _chequeNumber = string.Empty;
        [ObservableProperty] private DateTime? _chequeDate = DateTime.Now;
        [ObservableProperty] private decimal? _chequeAmount = 0.00m;
        [ObservableProperty] private string _chequeStatus = "Pending";

        public ObservableCollection<string> ChequeStatusOptions { get; } = new()
        {
            "Pending", "Cleared", "Bounced"
        };

        private readonly AuditRecord? _originalRecord;
        private Guid? _clientGuid;
        private Guid? _branchGuid;
        private string? _branchName;

        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isConfirmSaveVisible = false;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";
        [ObservableProperty] private bool _isDiscardConfirmVisible = false;

        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        public TaxOthersAddRecordViewModel() 
        {
            _ = LoadClientCodesAsync(ClientId);
        }

        public TaxOthersAddRecordViewModel(AuditRecord record)
        {
            _ = LoadClientCodesAsync(record.ClientCode);
            _originalRecord = record;
            _clientGuid = record.ClientId;
            _branchGuid = record.BranchId;
            _branchName = record.Branch;
            ClientId = record.ClientCode ?? string.Empty;
            Date = record.Date;
            ClientName = record.ClientName ?? string.Empty;
            Company = record.Company ?? string.Empty;
            Assignment = record.Assignment ?? string.Empty;
            PaymentOption = record.PaymentOption ?? "Cash";
            PaymentStatus = record.PaymentStatus ?? "Unpaid";

            // Pre-fill Cheque Details
            ChequeBank = record.ChequeBank ?? string.Empty;
            ChequeNumber = record.ChequeNumber ?? string.Empty;
            ChequeDate = record.ChequeDate ?? DateTime.Now;
            ChequeAmount = record.ChequeAmount ?? 0.00m;
            ChequeStatus = record.ChequeStatus ?? "Pending";
        }

        [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
        [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

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
            try 
            {
                HasFormError = false;
                
                if (_originalRecord != null)
                {
                    _originalRecord.ClientCode = ClientId;
                    _originalRecord.ClientId = _clientGuid;
                    _originalRecord.BranchId = _branchGuid;
                    _originalRecord.Date = Date ?? DateTime.Now;
                    _originalRecord.ClientName = ClientName;
                    _originalRecord.Company = Company;
                    _originalRecord.Assignment = Assignment;
                    _originalRecord.PaymentOption = PaymentOption;
                    _originalRecord.PaymentStatus = PaymentStatus;
                    
                    // Map Cheque Details
                    _originalRecord.ChequeBank = ChequeBank;
                    _originalRecord.ChequeNumber = ChequeNumber;
                    _originalRecord.ChequeDate = ChequeDate;
                    _originalRecord.ChequeAmount = ChequeAmount;
                    _originalRecord.ChequeStatus = ChequeStatus;
                    
                    await DataService.Instance.UpdateAuditRecordAsync("Tax Others", _originalRecord);
                }
                else
                {
                    var newRecord = new AuditRecord
                    {
                        ClientCode = ClientId,
                        ClientId = _clientGuid,
                        BranchId = _branchGuid,
                        Branch = _branchName,
                        Date = Date ?? DateTime.Now,
                        ClientName = ClientName,
                        Company = Company,
                        Assignment = Assignment,
                        PaymentOption = PaymentOption,
                        PaymentStatus = PaymentStatus,
                        
                        // Map Cheque Details
                        ChequeBank = ChequeBank,
                        ChequeNumber = ChequeNumber,
                        ChequeDate = ChequeDate,
                        ChequeAmount = ChequeAmount,
                        ChequeStatus = ChequeStatus,
                        Process = "BOOKKEEP",
                        CurrentStep = 1
                    };
                    
                    await DataService.Instance.AddAuditRecordAsync("Tax Others", newRecord);
                }
            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"Error saving record: {ex.Message}");
                HasFormError = true;
                FormErrorMessage = "Database connection error. Record not saved.";
                return;
            }

            if (GoBack != null) await GoBack();
        }

        [RelayCommand] private void CancelSave() => IsConfirmSaveVisible = false;

        [RelayCommand]
        private void DiscardChanges() => IsDiscardConfirmVisible = true;

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmDiscard()
        {
            IsDiscardConfirmVisible = false;
            if (GoBack != null) await GoBack();
        }

        [RelayCommand]
        private void CancelDiscard() => IsDiscardConfirmVisible = false;

        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
        }

        partial void OnChequeBankChanged(string value)
        {
            FilterBanks(value);
        }

        public override void SelectClientCode(ClientRecord client)
        {
            ClientId = client.ClientCode ?? string.Empty;
            ClientName = client.Name ?? string.Empty;
            if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
            _branchGuid = client.BranchId;
            _branchName = client.Branch;
            IsClientCodeDropdownOpen = false;
        }

        public override void SelectBank(string bank)
        {
            ChequeBank = bank;
            IsBankDropdownOpen = false;
        }
    }
}
