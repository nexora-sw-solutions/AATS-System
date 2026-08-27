using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BOIDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: BOI Registration";
    public override string GuideDescription => "Manage Board of Investment (BOI) approvals, compliance, and reporting.";
    public override string GuideProTip => "Track periodic compliance requirements and status updates for BOI-registered companies.";
    public override string Category => "BOI Registrations";

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    public BOIDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
    }

    // Dynamic Upload Visibility Properties
    public bool IsPaymentUploadVisible => Record?.CurrentStep == 1;
    public bool IsTDLApprovalUploadVisible => Record?.CurrentStep == 2;
    public bool IsBOIApprovalUploadVisible => Record?.CurrentStep == 3;
    public bool IsROCApprovalUploadVisible => Record?.CurrentStep == 4;
    public bool IsVisaUploadVisible => Record?.CurrentStep == 5;
    public bool IsWorkerVisaUploadVisible => Record?.CurrentStep == 6;
    public bool IsVATUploadVisible => Record?.CurrentStep == 7;
    public bool IsBOISubscriptionUploadVisible => Record?.CurrentStep == 8;

    [ObservableProperty] private ObservableCollection<AppDocument> _allProcessDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();

    // Registration Process Document Card Tab logic
    [ObservableProperty] private string _selectedProcessDocumentsCardTab = "Payment";
    [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessCardDocuments = new();
    
    [ObservableProperty] private AppDocument? _editingDocument;
    [ObservableProperty] private bool _isGlobalEditVisible;
    private AppDocument? _originalEditingSource;

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Payment", null),
            ("TDL Approval", null),
            ("BOI Approval", null),
            ("ROC Approval", null),
            ("Visa", null),
            ("Worker Visa", null),
            ("VAT", null),
            ("BOI Subscription", null)
        };

        SetupSteps(stepDefinitions);
    }

    protected override void UpdateStepStates()
    {
        base.UpdateStepStates();
        OnPropertyChanged(nameof(IsPaymentUploadVisible));
        OnPropertyChanged(nameof(IsTDLApprovalUploadVisible));
        OnPropertyChanged(nameof(IsBOIApprovalUploadVisible));
        OnPropertyChanged(nameof(IsROCApprovalUploadVisible));
        OnPropertyChanged(nameof(IsVisaUploadVisible));
        OnPropertyChanged(nameof(IsWorkerVisaUploadVisible));
        OnPropertyChanged(nameof(IsVATUploadVisible));
        OnPropertyChanged(nameof(IsBOISubscriptionUploadVisible));
        UpdateFilteredProcessDocuments();
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the BOI record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("BOI", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }

    [ObservableProperty] private string _selectedPart1Tab = "Approval Letter";
    [ObservableProperty] private string _selectedPart2Tab = "BOI Payment Slip";
    [ObservableProperty] private ObservableCollection<SourceDocument> _filteredPart1Documents = new();
    [ObservableProperty] private ObservableCollection<SourceDocument> _filteredPart2Documents = new();

    [RelayCommand]
    private void SelectPart1Tab(string tabName)
    {
        SelectedPart1Tab = tabName;
        UpdateFilteredPart1Documents();
    }

    [RelayCommand]
    private void SelectPart2Tab(string tabName)
    {
        SelectedPart2Tab = tabName;
        UpdateFilteredPart2Documents();
    }

    public void UpdateFilteredPart1Documents()
    {
        var filtered = SourceDocuments.Where(d => (d.Description ?? "Approval Letter") == SelectedPart1Tab).ToList();
        FilteredPart1Documents.Clear();
        foreach (var doc in filtered)
        {
            FilteredPart1Documents.Add(doc);
        }
    }

    public void UpdateFilteredPart2Documents()
    {
        var filtered = SourceDocuments.Where(d => (d.Description ?? "BOI Payment Slip") == SelectedPart2Tab).ToList();
        FilteredPart2Documents.Clear();
        foreach (var doc in filtered)
        {
            FilteredPart2Documents.Add(doc);
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadProcessDocument(string documentType)
    {
        if (RequestFilePicker == null) return;
        var paths = await RequestFilePicker();
        if (paths == null || paths.Length == 0) return;

        string category = "Payment";
        if (Record?.CurrentStep == 2) category = "TDL Approval";
        else if (Record?.CurrentStep == 3) category = "BOI Approval";
        else if (Record?.CurrentStep == 4) category = "ROC Approval";
        else if (Record?.CurrentStep == 5) category = "Visa";
        else if (Record?.CurrentStep == 6) category = "Worker Visa";
        else if (Record?.CurrentStep == 7) category = "VAT";
        else if (Record?.CurrentStep == 8) category = "BOI Subscription";

        foreach (var path in paths)
        {
            var doc = new AppDocument
            {
                FileName = System.IO.Path.GetFileName(path),
                ImagePath = path,
                Category = category,
                Type = documentType,
                FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB",
                IsExisting = false
            };
            AllProcessDocuments.Add(doc);
        }
        UpdateFilteredProcessDocuments();
    }

    [RelayCommand]
    private void RemoveProcessDocument(AppDocument doc)
    {
        if (doc != null)
        {
            AllProcessDocuments.Remove(doc);
            UpdateFilteredProcessDocuments();
        }
    }

    private void UpdateFilteredProcessDocuments()
    {
        string category = "Payment";
        if (Record?.CurrentStep == 2) category = "TDL Approval";
        else if (Record?.CurrentStep == 3) category = "BOI Approval";
        else if (Record?.CurrentStep == 4) category = "ROC Approval";
        else if (Record?.CurrentStep == 5) category = "Visa";
        else if (Record?.CurrentStep == 6) category = "Worker Visa";
        else if (Record?.CurrentStep == 7) category = "VAT";
        else if (Record?.CurrentStep == 8) category = "BOI Subscription";

        var filtered = AllProcessDocuments.Where(f => f.Category == category).ToList();
        FilteredProcessDocuments.Clear();
        foreach (var doc in filtered)
        {
            FilteredProcessDocuments.Add(doc);
        }
        
        UpdateFilteredProcessCardDocuments();
    }

    [RelayCommand]
    private void SelectProcessDocumentsCardTab(string tabName)
    {
        SelectedProcessDocumentsCardTab = tabName;
        UpdateFilteredProcessCardDocuments();
    }

    private void UpdateFilteredProcessCardDocuments()
    {
        var filtered = AllProcessDocuments.Where(f => f.Category == SelectedProcessDocumentsCardTab).ToList();
        FilteredProcessCardDocuments.Clear();
        foreach (var doc in filtered)
        {
            FilteredProcessCardDocuments.Add(doc);
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task AddProcessCardDocument()
    {
        if (RequestFilePicker == null) return;
        var paths = await RequestFilePicker();
        if (paths == null || paths.Length == 0) return;

        foreach (var path in paths)
        {
            var doc = new AppDocument
            {
                FileName = System.IO.Path.GetFileName(path),
                ImagePath = path,
                Category = SelectedProcessDocumentsCardTab,
                Type = "PROCESS",
                FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB",
                IsExisting = false
            };
            AllProcessDocuments.Add(doc);
        }
        UpdateFilteredProcessDocuments();
    }

    [RelayCommand]
    private void EditProcessCardDocument(AppDocument doc)
    {
        if (doc != null)
        {
            EditingDocument = new AppDocument
            {
                FileName = doc.FileName,
                FileSize = doc.FileSize,
                Category = doc.Category,
                Type = doc.Type,
                Description = doc.Description,
                ImagePath = doc.ImagePath
            };
            
            _originalEditingSource = doc;
            IsGlobalEditVisible = true;
        }
    }

    [RelayCommand]
    private void SaveGlobalEdit()
    {
        if (EditingDocument != null && _originalEditingSource != null)
        {
            _originalEditingSource.FileName = EditingDocument.FileName;
            _originalEditingSource.Category = EditingDocument.Category;
            _originalEditingSource.Type = EditingDocument.Type;
            _originalEditingSource.Description = EditingDocument.Description;
            
            UpdateFilteredProcessDocuments();
            IsGlobalEditVisible = false;
            EditingDocument = null;
            _originalEditingSource = null;
        }
    }

    [RelayCommand]
    private void CancelGlobalEdit()
    {
        IsGlobalEditVisible = false;
        EditingDocument = null;
        _originalEditingSource = null;
    }

    [RelayCommand]
    private void DeleteProcessCardDocument(AppDocument doc)
    {
        if (doc != null)
        {
            var documentsInStage = AllProcessDocuments.Count(x => x.Category == doc.Category);
            if (documentsInStage <= 1)
            {
                ShowConfirmDialog($"Validation Error: Each registration process stage must maintain at least one document record. Cannot delete the final remaining document for '{doc.Category}'.", 
                    () => System.Threading.Tasks.Task.CompletedTask);
                return;
            }
            
            ShowConfirmDialog($"Are you sure you want to delete {doc.FileName}?", async () =>
            {
                AllProcessDocuments.Remove(doc);
                UpdateFilteredProcessDocuments();
                await System.Threading.Tasks.Task.CompletedTask;
            });
        }
    }

    private void ShowConfirmDialog(string message, Func<System.Threading.Tasks.Task> confirmAction)
    {
         ConfirmDialogTitle = "Confirmation";
         ConfirmDialogMessage = message;
         ConfirmActionDelegate = confirmAction;
         IsConfirmDialogVisible = true;
    }

    protected override void OnRecordLoaded(AuditRecord? value)
    {
        base.OnRecordLoaded(value);
        UpdateFilteredPart1Documents();
        UpdateFilteredPart2Documents();
    }
}