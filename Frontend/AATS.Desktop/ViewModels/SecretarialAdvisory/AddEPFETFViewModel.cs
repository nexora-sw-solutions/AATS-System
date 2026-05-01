using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddEPFETFViewModel : ViewModelBase
{
    // Fields matching mockup
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _noOfStaffsText = string.Empty;

    // UI state
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private bool _isGuideVisible = false;

    private readonly AuditRecord? _recordToEdit;

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public AddEPFETFViewModel()
    {
        _ = LoadClientCodesAsync();
    }

    public AddEPFETFViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync();
        _recordToEdit = record;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        NoOfStaffsText = record.NoOfStaffs.ToString();
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand]
    private void SaveRecord()
    {
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;

        int staffCount = 0;
        int.TryParse(NoOfStaffsText, out staffCount);

        if (_recordToEdit != null)
        {
            _recordToEdit.ClientCode = ClientId;
            _recordToEdit.Date = Date ?? DateTime.Now;
            _recordToEdit.ClientName = ClientName;
            _recordToEdit.Company = CompanyName;
            _recordToEdit.NoOfStaffs = staffCount;
            await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", _recordToEdit);
        }
        else
        {
            var newRecord = new AuditRecord
            {
                ClientCode = ClientId,
                Date = Date ?? DateTime.Now,
                ClientName = ClientName,
                Company = CompanyName,
                NoOfStaffs = staffCount,
                PaymentStatus = "PENDING",
                Process = "PENDING",
                CurrentStep = 1,
                StaffList = new System.Collections.Generic.List<StaffMember>()
            };
            await DataService.Instance.AddAuditRecordAsync("EPF / ETF", newRecord);
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelSave() => IsConfirmSaveVisible = false;
    [RelayCommand] private void DiscardChanges() => IsDiscardConfirmVisible = true;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelDiscard() => IsDiscardConfirmVisible = false;

    partial void OnClientIdChanged(string value)
    {
        FilterClientCodes(value);
    }

    public override void SelectClientCode(ClientRecord client)
    {
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        IsClientCodeDropdownOpen = false;
    }
}