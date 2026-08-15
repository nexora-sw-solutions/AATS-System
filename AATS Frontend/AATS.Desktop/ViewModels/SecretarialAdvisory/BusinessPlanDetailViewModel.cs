using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class BusinessPlanDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: Business Plan & Asset Valuation";
    public override string GuideDescription => "Organize and track business plan development and advisory services.";
    public override string GuideProTip => "Use the audit notes to document specific client strategy discussions and projections.";
    public override string Category => "Business Plans";

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    public ObservableCollection<SourceDocument> FinalizationDocuments
    {
        get
        {
            if (Record?.SourceDocuments == null) return new ObservableCollection<SourceDocument>();
            return new ObservableCollection<SourceDocument>(System.Linq.Enumerable.Where(Record.SourceDocuments, d => d.Description == "Finalization Document"));
        }
    }

    [ObservableProperty] private SourceDocument? _editingDocument;
    [ObservableProperty] private bool _isGlobalEditVisible;
    private SourceDocument? _originalEditingSource;

    public BusinessPlanDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Drafting", null),
            ("Review", null),
            ("Valuation", null),
            ("Finalization", null)
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the Business Plan record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("Business Plan and Asset Valuation Consulting", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task AddDocument()
    {
        if (RequestFilePicker == null) return;
        
        var paths = await RequestFilePicker();
        if (paths == null || paths.Length == 0) return;

        bool updated = false;
        foreach (var path in paths)
        {
            var fileName = System.IO.Path.GetFileName(path);
            var fileExt = System.IO.Path.GetExtension(path).TrimStart('.');
            
            var doc = new SourceDocument
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                FileType = string.IsNullOrEmpty(fileExt) ? "FILE" : fileExt.ToUpper(),
                CreatedAt = DateTime.UtcNow,
                Description = "Finalization Document",
                Url = path
            };
            
            if (Record != null)
            {
                if (Record.SourceDocuments == null) Record.SourceDocuments = new List<SourceDocument>();
                Record.SourceDocuments.Add(doc);
                updated = true;
            }
        }
        
        if (updated)
        {
            await SaveRecordUpdateAsync();
            OnPropertyChanged(nameof(FinalizationDocuments));
            Refresh();
        }
    }

    [RelayCommand]
    private void EditFinalizationDocument(SourceDocument doc)
    {
        if (doc != null)
        {
            EditingDocument = new SourceDocument
            {
                Id = doc.Id,
                FileName = doc.FileName,
                FileType = doc.FileType,
                Description = doc.Description,
                CreatedAt = doc.CreatedAt,
                Url = doc.Url
            };
            _originalEditingSource = doc;
            IsGlobalEditVisible = true;
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveGlobalEdit()
    {
        if (EditingDocument != null && _originalEditingSource != null)
        {
            _originalEditingSource.FileName = EditingDocument.FileName;
            _originalEditingSource.FileType = EditingDocument.FileType;
            
            await SaveRecordUpdateAsync();
            
            IsGlobalEditVisible = false;
            EditingDocument = null;
            _originalEditingSource = null;
            
            OnPropertyChanged(nameof(FinalizationDocuments));
            Refresh();
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
    private void DeleteFinalizationDocument(SourceDocument doc)
    {
        if (doc == null) return;
        
        if (FinalizationDocuments.Count <= 1)
        {
            ConfirmDialogTitle = "Cannot Delete Last Document";
            ConfirmDialogMessage = "At all times, the Finalization stage must contain at least one document. You cannot delete the final remaining document.";
            ConfirmActionDelegate = () => System.Threading.Tasks.Task.CompletedTask;
            IsConfirmDialogVisible = true;
            return;
        }
        
        ConfirmDialogTitle = "Delete Document?";
        ConfirmDialogMessage = $"Are you sure you want to delete the document '{doc.FileName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record?.SourceDocuments != null)
            {
                Record.SourceDocuments.Remove(doc);
                await SaveRecordUpdateAsync();
                OnPropertyChanged(nameof(FinalizationDocuments));
                Refresh();
            }
        };
        IsConfirmDialogVisible = true;
    }
}