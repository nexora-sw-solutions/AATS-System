using System;
using System.Collections.Generic;
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
        if (parameter is AppDocument doc && !string.IsNullOrEmpty(doc.ImagePath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.ImagePath) { UseShellExecute = true });
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
        if (doc != null && !string.IsNullOrEmpty(doc.ImagePath))
        {
            try
            {
                string target = doc.ImagePath;
                if (target.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) || 
                    target.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = doc.FileName ?? "downloaded_file";
                    await ApiService.Instance.DownloadDocumentAsync(target, fileName);
                    NotificationService.Instance.AddNotification("Downloaded", $"'{fileName}' saved to Downloads.");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to download document: {ex.Message}");
            }
        }
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