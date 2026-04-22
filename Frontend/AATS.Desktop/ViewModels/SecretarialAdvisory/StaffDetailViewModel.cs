using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class StaffDetailViewModel : DetailViewModelBase
{
    private readonly AuditRecord _parentRecord;
    private readonly StaffMember _staffMember;

    [ObservableProperty] private ObservableCollection<StaffHistory> _history = new();
    
    // Top Card Metrics
    public string StaffIdDisplay => _staffMember.StaffId ?? "N/A";
    public int TotalStaffCount => _parentRecord.NoOfStaffs;
    public string StaffPhone => _staffMember.Phone ?? "N/A";

    // General Information fields
    public string RecordID => _parentRecord.ID ?? "N/A";
    public string StaffName => _staffMember.StaffName ?? "N/A";

    // Document filenames
    [ObservableProperty] private string _nicFileName = "Not uploaded";
    [ObservableProperty] private string _brFileName = "Not uploaded";
    [ObservableProperty] private string _r1FileName = "Not uploaded";
    [ObservableProperty] private string _artFileName = "Not uploaded";
    [ObservableProperty] private string _staffNicFileName = "Not uploaded";

    // Document picker delegates
    public Func<Task<string?>>? RequestNicPicker { get; set; }
    public Func<Task<string?>>? RequestBrPicker { get; set; }
    public Func<Task<string?>>? RequestR1Picker { get; set; }
    public Func<Task<string?>>? RequestArtPicker { get; set; }
    public Func<Task<string?>>? RequestStaffNicPicker { get; set; }
    public Action<AuditRecord, StaffMember>? NavigateToEditStaff { get; set; }

    public StaffDetailViewModel(AuditRecord parent, StaffMember member) : base(parent)
    {
        _parentRecord = parent;
        _staffMember = member;
        
        if (_staffMember.History != null)
        {
            History = new ObservableCollection<StaffHistory>(_staffMember.History);
        }

        InitializeSteps();
    }

    protected override void InitializeSteps()
    {
        var stepDefs = new List<(string Name, string? Icon)>
        {
            ("Submit", "fa-solid fa-circle-check"),
            ("Complete", "fa-solid fa-flag-checkered")
        };
        
        SetupSteps(stepDefs);
    }

    protected override void UpdateStepStates()
    {
        int currentStep = 1;
        if (_staffMember.Process?.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase) == true)
        {
            currentStep = 2;
        }

        for (int i = 0; i < Steps.Count; i++)
        {
            int stepNum = i + 1;
            Steps[i].IsActive = stepNum <= currentStep;
            Steps[i].IsClickable = true;
        }
    }

    [RelayCommand]
    private void StaffStepClick(ProcessStep step)
    {
        if (step.Number == 1 && _staffMember.IsSubmit) return;
        if (step.Number == 2 && _staffMember.IsComplete) return;

        ConfirmDialogTitle = "Change Process Status?";
        ConfirmDialogMessage = $"Are you sure you want to change the status to '{step.Name}' for '{StaffName}'?";
        ConfirmActionDelegate = () =>
        {
            if (step.Number == 1) _staffMember.Process = "SUBMIT";
            else if (step.Number == 2) _staffMember.Process = "COMPLETE";
            
            return ExecuteAsyncLambda(async () => 
            {
                await SaveChangesAsync();
                UpdateStepStates();
                Refresh();
            });
        };
        IsConfirmDialogVisible = true;
    }

    private async System.Threading.Tasks.Task ExecuteAsyncLambda(Func<System.Threading.Tasks.Task> action)
    {
        await action();
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", _parentRecord);
        }
        catch (Exception ex)
        {
            // Log or handle error if needed
            System.Diagnostics.Debug.WriteLine($"Failed to auto-save: {ex.Message}");
        }
    }

    public override void OnEditRecord()
    {
        NavigateToEditStaff?.Invoke(_parentRecord, _staffMember);
    }

    [RelayCommand]
    private async Task UploadNic()
    {
        if (RequestNicPicker != null)
        {
            var result = await RequestNicPicker();
            if (result != null) 
            {
                NicFileName = result;
                await SaveChangesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task UploadBr()
    {
        if (RequestBrPicker != null)
        {
            var result = await RequestBrPicker();
            if (result != null) 
            {
                BrFileName = result;
                await SaveChangesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task UploadR1()
    {
        if (RequestR1Picker != null)
        {
            var result = await RequestR1Picker();
            if (result != null) 
            {
                R1FileName = result;
                await SaveChangesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task UploadArt()
    {
        if (RequestArtPicker != null)
        {
            var result = await RequestArtPicker();
            if (result != null) 
            {
                ArtFileName = result;
                await SaveChangesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task UploadStaffNic()
    {
        if (RequestStaffNicPicker != null)
        {
            var result = await RequestStaffNicPicker();
            if (result != null) 
            {
                StaffNicFileName = result;
                await SaveChangesAsync();
            }
        }
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Staff Member?";
        ConfirmDialogMessage = $"Are you sure you want to delete staff member '{StaffName}' ({StaffIdDisplay})? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (_parentRecord.StaffList != null && _parentRecord.StaffList.Contains(_staffMember))
            {
                _parentRecord.StaffList.Remove(_staffMember);
                _parentRecord.NoOfStaffs = _parentRecord.StaffList.Count;
                await SaveChangesAsync();
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }

    public override void Refresh()
    {
        base.Refresh();
        OnPropertyChanged(nameof(StaffIdDisplay));
        OnPropertyChanged(nameof(StaffPhone));
        OnPropertyChanged(nameof(StaffName));
    }
}
