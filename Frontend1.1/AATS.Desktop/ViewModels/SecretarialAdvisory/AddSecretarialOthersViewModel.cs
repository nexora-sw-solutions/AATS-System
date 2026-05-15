using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddSecretarialOthersViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _assignment = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0;

    [ObservableProperty] private string _paymentOption = "Cash";
    [ObservableProperty] private string _paymentStatus = "Unpaid";

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0;

    public decimal TotalPayment => Math.Max(0, SubTotal - Discount);

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    public AddSecretarialOthersViewModel()
    {
        _ = LoadClientCodesAsync(ClientId);
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
