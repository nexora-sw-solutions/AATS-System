using System;
using System.Collections.Generic;
using System.Linq;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class HRConsultingDetailViewModel : DetailViewModelBase
{
    public override string GuideTitle => "Guide: HR & Management Consulting";
    public override string GuideDescription => "Track human resources consulting projects and advisory engagements.";
    public override string GuideProTip => "Maintain comprehensive notes on HR auditing results to provide high-quality policy development advice.";
    public override string Category => "HR Consulting";

    public System.Collections.ObjectModel.ObservableCollection<AppDocument> FilteredDocuments { get; } = new();

    public HRConsultingDetailViewModel(AuditRecord record) : base(record)
    {
        InitializeSteps();
        LoadDocuments();
    }

    private void LoadDocuments()
    {
        FilteredDocuments.Clear();
        if (Record?.SourceDocuments != null)
        {
            foreach (var doc in Record.SourceDocuments)
            {
                FilteredDocuments.Add(new AppDocument
                {
                    FileName = doc.FileName,
                    Category = doc.Description ?? "Document",
                    FileSize = "Unknown",
                    ImagePath = doc.Url
                });
            }
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void PreviewDocument(object parameter)
    {
        string? target = null;
        if (parameter is AppDocument doc) target = !string.IsNullOrEmpty(doc.Url) ? doc.Url : doc.ImagePath;
        else if (parameter is SourceDocument srcDoc) target = srcDoc.Url;
        else if (parameter is string str) target = str;

        if (!string.IsNullOrEmpty(target))
        {
            string fullUrl = ApiService.GetFullDocumentUrl(target);
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to open document: {ex.Message}");
            }
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    public async System.Threading.Tasks.Task DownloadDocument(AppDocument doc)
    {
        if (doc != null)
        {
            try
            {
                string target = !string.IsNullOrEmpty(doc.Url) ? doc.Url : doc.ImagePath;
                if (!string.IsNullOrEmpty(target))
                {
                    string fullUrl = ApiService.GetFullDocumentUrl(target);
                    var fileName = doc.FileName ?? "downloaded_file";
                    if (fullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        await ApiService.Instance.DownloadDocumentAsync(fullUrl, fileName);
                        NotificationService.Instance.AddNotification("Downloaded", $"'{fileName}' saved to Downloads.");
                    }
                    else if (System.IO.File.Exists(fullUrl))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullUrl) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download file: {ex.Message}");
            }
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void DeleteDocument(AppDocument doc)
    {
        if (doc == null) return;
        ConfirmDialogTitle = "Delete Document?";
        ConfirmDialogMessage = $"Are you sure you want to delete '{doc.FileName}'?";
        ConfirmActionDelegate = async () =>
        {
            FilteredDocuments.Remove(doc);
            if (Record != null && Record.SourceDocuments != null)
            {
                var match = Record.SourceDocuments.FirstOrDefault(d => d.FileName == doc.FileName || d.Url == doc.Url);
                if (match != null) Record.SourceDocuments.Remove(match);
                await DataService.Instance.UpdateAuditRecordAsync("HR and Management Consulting", Record);
                NotificationService.Instance.AddNotification("Success", "Document deleted.");
            }
            IsConfirmDialogVisible = false;
        };
        IsConfirmDialogVisible = true;
    }

    protected override void InitializeSteps()
    {
        var stepDefinitions = new List<(string Name, string? Icon)>
        {
            ("Consultation", null),
            ("Planning", null),
            ("Implementation", null),
            ("Review", null)
        };

        SetupSteps(stepDefinitions);
    }

    public override void OnDeleteRecord()
    {
        ConfirmDialogTitle = "Delete Record?";
        ConfirmDialogMessage = $"Are you sure you want to delete the HR Consulting record for '{CompanyName}'? This action cannot be undone.";
        ConfirmActionDelegate = async () =>
        {
            if (Record != null)
            {
                await DataService.Instance.DeleteAuditRecordsAsync("HR Consulting", new[] { Record });
                NavigateBack?.Invoke();
            }
        };
        IsConfirmDialogVisible = true;
    }
}