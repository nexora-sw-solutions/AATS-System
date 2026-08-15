using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddSecretarialOthersViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Client ID is required")]
    private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Company name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    private string _companyName = string.Empty;
    [ObservableProperty] private string _assignment = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0;

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

    public bool IsOptionCash { get => PaymentOption == "Cash"; set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
    public bool IsOptionOnline { get => PaymentOption == "Online"; set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
    public bool IsOptionCheque { get => PaymentOption == "Cheque"; set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }

    public bool IsStatusPaid { get => PaymentStatus == "Paid"; set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
    public bool IsStatusUnpaid { get => PaymentStatus == "Unpaid"; set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
    public bool IsStatusPartial { get => PaymentStatus == "Partial"; set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }

    // Cheque Details
    [ObservableProperty] private string _chequeBank = string.Empty;
    [ObservableProperty] private string _chequeNumber = string.Empty;
    [ObservableProperty] private DateTime? _chequeDate = DateTime.Now;
    [ObservableProperty] private decimal? _chequeAmount = 0.00m;
    [ObservableProperty] private string _chequeStatus = "Pending";

    public System.Collections.ObjectModel.ObservableCollection<string> ChequeStatusOptions { get; } = new()
    {
        "Pending", "Cleared", "Bounced"
    };



    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    public AddSecretarialOthersViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    // UI State
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
    [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

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

        ConfirmSaveTitle = "Save Record?";
        ConfirmSaveMessage = "Are you sure you want to create this new secretarial record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        var newRecord = new AuditRecord
        {
            ClientCode = ClientId,
            ClientId = _clientGuid,
            BranchId = _branchGuid ,
            Branch = _branchName,
            Date = Date ?? DateTime.Now,
            ClientName = ClientName,
            Company = CompanyName,
            Assignment = Assignment,
            PaymentOption = PaymentOption,
            PaymentStatus = PaymentStatus,
            SubTotal = SubTotal,
            Discount = Discount,
            TotalPayment = TotalPayment,
            PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0),
            Process = "PENDING",
            CurrentStep = 1,
            
            // Map Cheque Details
            ChequeBank = ChequeBank,
            ChequeNumber = ChequeNumber,
            ChequeDate = ChequeDate,
            ChequeAmount = ChequeAmount,
            ChequeStatus = ChequeStatus
        };
        
        await DataService.Instance.AddAuditRecordAsync("Secretarial Others", newRecord);

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
        _isSelectingClient = true;
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;
        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }

    public override void SelectBank(string bank)
    {
        ChequeBank = bank;
        IsBankDropdownOpen = false;
    }
}

