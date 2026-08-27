using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddEPFETFStaffViewModel : ViewModelBase
{
    private readonly AuditRecord _parentRecord;
    private readonly StaffMember? _existingStaff;

    [ObservableProperty] private string _staffId = string.Empty;
    [ObservableProperty] private string _staffName = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _process = "SUBMIT"; // Default

    // Client Info (Syncs with parent record)
    public string ClientId => _parentRecord.ID ?? string.Empty;
    public new string SelectedClientCategoryColor => _parentRecord.ClientCategoryColor;
    public new bool HasSelectedClientCategory => _parentRecord.HasClientCategory;

    public DateTime? ClientDate
    {
        get => _parentRecord.Date;
        set
        {
            _parentRecord.Date = value ?? DateTime.Now;
            OnPropertyChanged();
        }
    }

    public string Branch
    {
        get => _parentRecord.Branch ?? string.Empty;
        set
        {
            _parentRecord.Branch = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> ProcessOptions { get; } = new() { "SUBMIT", "COMPLETE" };

    // Common UI State
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public AddEPFETFStaffViewModel(AuditRecord parentRecord, StaffMember? existingStaff = null)
    {
        _parentRecord = parentRecord;
        _existingStaff = existingStaff;

        if (_existingStaff != null)
        {
            StaffId = _existingStaff.StaffId ?? string.Empty;
            StaffName = _existingStaff.StaffName ?? string.Empty;
            Phone = _existingStaff.Phone ?? string.Empty;
            Process = _existingStaff.Process ?? "SUBMIT";
        }
    }

    [RelayCommand]
    private void SaveRecord()
    {
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;

        if (_existingStaff != null)
        {
            _existingStaff.StaffId = StaffId;
            _existingStaff.StaffName = StaffName;
            _existingStaff.Phone = Phone;
            _existingStaff.Process = Process;
        }
        else
        {
            if (_parentRecord.StaffList == null) _parentRecord.StaffList = new();
            _parentRecord.StaffList.Add(new StaffMember
            {
                StaffId = StaffId,
                StaffName = StaffName,
                Phone = Phone,
                Process = Process
            });
        }

        try
        {
            // Update the parent record in data service
            await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", _parentRecord);
            if (GoBack != null) await GoBack();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to save staff member: {ex.Message}");
            NotificationService.Instance.AddNotification("Error", $"Failed to save: {ex.Message}");
            IsConfirmSaveVisible = false;
        }
    }

    [RelayCommand]
    private void CancelSave() => IsConfirmSaveVisible = false;

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
}