using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class AuditOthersAddRecordViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client ID is required")]
        private string _clientId = string.Empty;
        private Guid? _clientGuid;
        private Guid? _branchGuid;

        [ObservableProperty]
        private DateTime? _date = DateTime.UtcNow;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        private string _clientName = string.Empty;

        partial void OnClientNameChanged(string value)
        {
            FilterClientNames(value);
        }

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _partialAmount = 0.00m;

        public decimal TotalPayment => PaymentStatus switch
        {
            "Paid" => 0,
            "Partial" => Math.Max(0, SubTotal - Discount - PartialAmount),
            _ => Math.Max(0, SubTotal - Discount)
        };

        public bool IsPaymentStatusPartial => PaymentStatus == "Partial";

        partial void OnPaymentStatusChanged(string value)
        {
            if (value != "Partial")
            {
                PartialAmount = 0;
            }
        }

        [ObservableProperty] private string _paymentOption = "Cash";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        [NotifyPropertyChangedFor(nameof(IsPaymentStatusPartial))]
        private string _paymentStatus = "Unpaid";

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
        [ObservableProperty] private bool _isDiscardConfirmVisible = false;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        public AuditOthersAddRecordViewModel() 
        {
            _ = LoadClientCodesAsync(() => ClientId); 
        }

        public AuditOthersAddRecordViewModel(AuditRecord record)
        {
            _ = LoadClientCodesAsync(() => record.ClientCode);
            _originalRecord = record;
            ClientId = record.ClientCode ?? string.Empty;
            _clientGuid = record.ClientId;
            Date = record.Date;
            ClientName = record.ClientName ?? string.Empty;
            Company = record.Company ?? string.Empty;
            Assignment = record.Assignment ?? string.Empty;
            PaymentOption = record.PaymentOption ?? "Cash";
            PaymentStatus = record.PaymentStatus ?? "Unpaid";
            SubTotal = record.SubTotal;
            Discount = record.Discount;
            PartialAmount = record.PartialAmount;
        }

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

        [RelayCommand]
        private void SaveRecord()
        {
        var clientExists = SharedClientsList.Any(c => c.ClientCode != null && c.ClientCode.Equals(ClientId, System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(ClientId) || !clientExists)
        {
            HasFormError = true;
            FormErrorMessage = "Invalid Client ID. Please select an existing client before saving.";
            return;
        }

            HasFormError = false;
            if (PaymentStatus == "Partial")
            {
                if (PartialAmount == 0)
                {
                    HasFormError = true;
                    FormErrorMessage = "Please enter the partially paid amount.";
                    return;
                }
                if (PartialAmount < 0)
                {
                    HasFormError = true;
                    FormErrorMessage = "Amount must be greater than or equal to zero.";
                    return;
                }
                if (PartialAmount > (SubTotal - Discount))
                {
                    HasFormError = true;
                    FormErrorMessage = "Partially paid amount cannot exceed the subtotal.";
                    return;
                }
            }

            ConfirmSaveTitle = _originalRecord != null ? "Save Changes?" : "Save Record?";
            ConfirmSaveMessage = _originalRecord != null 
                ? "Are you sure you want to save the modifications to this record?" 
                : "Are you sure you want to create this new audit record?";
            IsConfirmSaveVisible = true;
        }

        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
        }

        private ClientRecord? _selectedClient;

        public override void SelectClientCode(ClientRecord client)
        {
            _isSelectingClient = true;
            _selectedClient = client;
            ClientId = client.ClientCode ?? string.Empty;
            _clientGuid = Guid.TryParse(client.Id, out var guid) ? guid : null;
            _branchGuid = client.BranchId;
            ClientName = client.Name ?? string.Empty;
            _isSelectingClient = false;
            IsClientCodeDropdownOpen = false;
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
                    _originalRecord.BranchId = _branchGuid ;
                    _originalRecord.Date = Date ?? DateTime.UtcNow;
                    _originalRecord.ClientName = ClientName;
                    _originalRecord.Company = Company;
                    _originalRecord.Assignment = Assignment;
                    _originalRecord.PaymentOption = PaymentOption;
                    _originalRecord.PaymentStatus = PaymentStatus;
                    _originalRecord.SubTotal = SubTotal;
                    _originalRecord.Discount = Discount;
                    _originalRecord.TotalPayment = TotalPayment;
                    _originalRecord.PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0);
                    
                    await DataService.Instance.UpdateAuditRecordAsync("Audit Others", _originalRecord);
                }
                else
                {
                    var newRecord = new AuditRecord
                    {
                        ClientCode = ClientId,
                        ClientId = _clientGuid,
                        BranchId = _branchGuid ,
                        Date = Date ?? DateTime.UtcNow,
                        ClientName = ClientName,
                        Company = Company,
                        Assignment = Assignment,
                        PaymentOption = PaymentOption,
                        PaymentStatus = PaymentStatus,
                        SubTotal = SubTotal,
                        Discount = Discount,
                        TotalPayment = TotalPayment,
                        PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0),
                        SourceDocuments = _selectedClient?.GetAllClientDocuments() ?? new()
                    };
                    
                    await DataService.Instance.AddAuditRecordAsync("Audit Others", newRecord);
                }
            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"Error saving record: {ex.Message}");
                HasFormError = true;
                FormErrorMessage = "Database connection error. Record not saved.";
            }

            if (GoBack != null) await GoBack();
        }

        [RelayCommand]
        private void CancelSave() => IsConfirmSaveVisible = false;

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
    }
}

