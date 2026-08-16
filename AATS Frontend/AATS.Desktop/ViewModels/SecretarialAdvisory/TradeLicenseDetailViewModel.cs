using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class TradeLicenseDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: Trade License Details";
    public override string GuideDescription => "Track and manage the trade license renewal process including assessment and registration.";
    public override string GuideProTip => "Ensure all source documents like NIC and BR are verified before advancing to the 'Finalize' step.";
    public override string Category => "Trade Licenses";

    // Read-only Client Information Properties
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private string _loginId = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _phoneNo = string.Empty;

    // Password Visibility Toggle
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayPassword))]
    [NotifyPropertyChangedFor(nameof(PasswordIcon))]
    private bool _isPasswordVisible = false;

    public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Math.Max(10, Password.Length));
    public string PasswordIcon => IsPasswordVisible ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;







    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredCorporateDocuments))]
    private string _selectedCorporateDocumentTab = "BR";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredSupportingDocuments))]
    private string _selectedSupportingDocumentTab = "Photos";

    public ObservableCollection<SourceDocument> FilteredCorporateDocuments =>
        new(SourceDocuments.Where(d => (d.Description ?? "BR") == SelectedCorporateDocumentTab));

    public ObservableCollection<SourceDocument> FilteredSupportingDocuments =>
        new(SourceDocuments.Where(d => (d.Description ?? "Photos") == SelectedSupportingDocumentTab));

    [RelayCommand]
    private void SelectCorporateDocumentTab(string tabName) => SelectedCorporateDocumentTab = tabName;

    [RelayCommand]
    private void SelectSupportingDocumentTab(string tabName) => SelectedSupportingDocumentTab = tabName;

    [RelayCommand]
    private void PreviewSourceDocument(SourceDocument doc)
    {
        if (doc != null && !string.IsNullOrWhiteSpace(doc.Url ?? doc.FileName))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.Url ?? doc.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error previewing document: {ex.Message}");
            }
        }
    }


    // Document Management Collections
    [ObservableProperty] private ObservableCollection<AppDocument> _allProcessDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();
    [ObservableProperty] private string _selectedProcessDocumentsCardTab = "Temporary Certificate";
    [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessCardDocuments = new();

    // Stage Visibility Flags
    public bool IsTempCertificateUploadVisible => Record?.CurrentStep == 1;
    public bool IsLetterUploadVisible => Record?.CurrentStep == 2;
    public bool IsCertificateUploadVisible => Record?.CurrentStep == 3;

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    public TradeLicenseDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();

        UpdateFilteredProcessDocuments();
        ClientId = record.ClientCode ?? string.Empty;
        LoginId = record.LoginId ?? string.Empty;
        Password = record.Password ?? string.Empty;
        PhoneNo = record.PhoneNo ?? string.Empty;

        

    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Temporary Certificate", null),
            ("Letter", null),
            ("Certificate", "fa-solid fa-check")
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the Trade License record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("Trade Licenses", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }

    protected override void UpdateStepStates()
    {
        base.UpdateStepStates();
        UpdateFilteredProcessDocuments();
    }

    public void UpdateFilteredProcessDocuments()
    {
        OnPropertyChanged(nameof(IsTempCertificateUploadVisible));
        OnPropertyChanged(nameof(IsLetterUploadVisible));
        OnPropertyChanged(nameof(IsCertificateUploadVisible));

        string category = "Temporary Certificate";
        if (Record?.CurrentStep == 2) category = "Letter";
        else if (Record?.CurrentStep == 3) category = "Certificate";

        var filtered = AllProcessDocuments.Where(d => d.Category == category).ToList();
        FilteredProcessDocuments.Clear();
        foreach (var doc in filtered)
        {
            FilteredProcessDocuments.Add(doc);
        }
    }

    [RelayCommand]
    private void SelectProcessDocumentsCardTab(string tabName)
    {
        SelectedProcessDocumentsCardTab = tabName;
        UpdateFilteredProcessCardDocuments();
    }

    public void UpdateFilteredProcessCardDocuments()
    {
        var filtered = AllProcessDocuments.Where(d => d.Category == SelectedProcessDocumentsCardTab).ToList();
        FilteredProcessCardDocuments.Clear();
        foreach (var doc in filtered)
        {
            FilteredProcessCardDocuments.Add(doc);
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadProcessDocument(string documentType)
    {
        if (RequestFilePicker == null) return;
        var paths = await RequestFilePicker();
        if (paths == null || paths.Length == 0) return;

        string category = "Temporary Certificate";
        if (Record?.CurrentStep == 2) category = "Letter";
        else if (Record?.CurrentStep == 3) category = "Certificate";

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
        UpdateFilteredProcessCardDocuments();
    }

    [RelayCommand]
    private void RemoveProcessDocument(AppDocument doc)
    {
        if (doc != null)
        {
            var documentsInStage = AllProcessDocuments.Count(x => x.Category == doc.Category);
            if (documentsInStage <= 1)
            {
                ShowConfirmDialog("Cannot delete the final remaining document for a completed stage.", () => System.Threading.Tasks.Task.CompletedTask);
                return;
            }
            
            ShowConfirmDialog($"Are you sure you want to delete {doc.FileName}?", async () =>
            {
                AllProcessDocuments.Remove(doc);
                UpdateFilteredProcessDocuments();
                UpdateFilteredProcessCardDocuments();
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

    [RelayCommand]
    private void PreviewProcessDocument(AppDocument doc)
    {
        if (doc != null && !string.IsNullOrWhiteSpace(doc.ImagePath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.ImagePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error previewing document: {ex.Message}");
            }
        }
    }
}