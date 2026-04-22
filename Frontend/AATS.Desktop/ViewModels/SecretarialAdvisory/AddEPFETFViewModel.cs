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
    [ObservableProperty] private string _id = string.Empty;
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
        GenerateNextId();
    }

    public AddEPFETFViewModel(AuditRecord record)
    {
        _recordToEdit = record;
        Id = record.ID ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        NoOfStaffsText = record.NoOfStaffs.ToString();
    }

    private async void GenerateNextId()
    {
        try 
        {
            var records = await DataService.Instance.GetAuditRecordsAsync("EPF / ETF");
            int nextNumber = records.Count + 1;
            Id = $"EPF-{nextNumber:D3}";
        }
        catch 
        {
            Id = "EPF-001";
        }
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
            _recordToEdit.ID = Id;
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
                ID = Id,
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
}
