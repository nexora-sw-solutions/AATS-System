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

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0;

    // Payment Options (Boolean sync with Tax version)
    [ObservableProperty] private bool _isOptionCash = true;
    [ObservableProperty] private bool _isOptionOnline = false;
    [ObservableProperty] private bool _isOptionCheque = false;

    // Payment Status (Boolean sync with Tax version)
    [ObservableProperty] private bool _isStatusPaid = false;
    [ObservableProperty] private bool _isStatusUnpaid = true;
    [ObservableProperty] private bool _isStatusPartial = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0;

    public decimal TotalPayment => Math.Max(0, SubTotal - Discount);

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    public AddSecretarialOthersViewModel()
    {
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
            ID = ClientId,
            Date = Date ?? DateTime.Now,
            ClientName = ClientName,
            Company = CompanyName,
            Assignment = Assignment,
            PaymentOption = IsOptionCash ? "Cash" : IsOptionOnline ? "Online" : "Cheque",
            PaymentStatus = IsStatusPaid ? "Paid" : IsStatusUnpaid ? "Unpaid" : "Partial",
            Process = "PENDING",
            CurrentStep = 1
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
}


