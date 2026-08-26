using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels.Clients;

public partial class ClientDetailViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayClientId))]
    [NotifyPropertyChangedFor(nameof(DisplayTotalRevenue))]
    [NotifyPropertyChangedFor(nameof(DisplayDueAmount))]
    [NotifyPropertyChangedFor(nameof(HasDueAmount))]
    [NotifyPropertyChangedFor(nameof(DisplayBranch))]
    [NotifyPropertyChangedFor(nameof(DisplayCategory))]
    [NotifyPropertyChangedFor(nameof(DisplayRegistrationDate))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(DisplayStatus))]
    [NotifyPropertyChangedFor(nameof(DisplayPhone))]
    [NotifyPropertyChangedFor(nameof(DisplayEmail))]
    [NotifyPropertyChangedFor(nameof(AuditorNotes))]
    [NotifyPropertyChangedFor(nameof(DisplayAuditorNotes))]
    [NotifyPropertyChangedFor(nameof(ClientCategory))]
    [NotifyPropertyChangedFor(nameof(ClientCategoryColor))]
    [NotifyPropertyChangedFor(nameof(HasClientCategory))]
    [NotifyPropertyChangedFor(nameof(CategoryIcon))]
    private ClientRecord _record;

    public Action? GoBack { get; set; }

    // Top Summary Cards
    public string DisplayClientId => !string.IsNullOrEmpty(Record?.ClientCode) 
        ? Record.ClientCode 
        : (Record != null && !string.IsNullOrEmpty(Record.Id) ? "CL-" + (Record.Id.Length >= 5 ? Record.Id.Substring(0, 5).ToUpper() : Record.Id.ToUpper()) : "N/A");

    public string DisplayTotalRevenue => $"LKR {Record?.TotalRevenue:N2}";
    public bool HasDueAmount => Record?.HasDueAmount ?? false;
    public string DisplayDueAmount => $"Due: LKR {Record?.DueAmount:N2}";

    // General Information Card fields
    public string DisplayBranch => Record?.Branch ?? "Central";
    public string DisplayCategory => Record?.Category ?? "Loyal";
    public string DisplayRegistrationDate => (Record != null && Record.Date.Year > 1) 
        ? Record.Date.ToString("yyyy-MM-dd") 
        : ((Record != null && Record.CreatedAt.Year > 1) 
            ? Record.CreatedAt.ToString("yyyy-MM-dd") 
            : DateTime.UtcNow.ToString("yyyy-MM-dd"));
    public string DisplayName => Record?.Name ?? "N/A";
    public string DisplayStatus => Record?.Status ?? "Active";
    public string DisplayPhone => Record?.Phone ?? "N/A";
    public string DisplayEmail => Record?.Email ?? "N/A";

    public string AuditorNotes => Record?.Notes ?? string.Empty;
    public string DisplayAuditorNotes => !string.IsNullOrWhiteSpace(Record?.Notes) ? Record.Notes : "No notes available.";

    public string ClientCategory => Record?.Category ?? string.Empty;
    public string ClientCategoryColor => Record?.CategoryColor ?? "Transparent";
    public bool HasClientCategory => !string.IsNullOrEmpty(Record?.Category);
    public string CategoryIcon => Record?.CategoryIcon ?? "fa-solid fa-briefcase";

    // Document Preview Tab Pane
    [ObservableProperty] private ObservableCollection<AppDocument> _brDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _tinDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _form01Documents = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _articleOfAssociationDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _nicDocuments = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredDocumentFiles))]
    private string _selectedDocumentTab = "BR";

    public ObservableCollection<AppDocument> FilteredDocumentFiles => SelectedDocumentTab switch
    {
        "BR" => BrDocuments,
        "TIN" => TinDocuments,
        "Form 01" => Form01Documents,
        "Article of Association" => ArticleOfAssociationDocuments,
        "NIC" => NicDocuments,
        _ => new ObservableCollection<AppDocument>()
    };

    [RelayCommand]
    private void SelectDocumentTab(string tab) => SelectedDocumentTab = tab;

    public ClientDetailViewModel(ClientRecord record)
    {
        Record = record;
        LoadDocuments();
    }

    private void LoadDocuments()
    {
        if (Record == null) return;

        if (Record.BrAttachments != null)
        {
            foreach (var doc in Record.BrAttachments)
            {
                BrDocuments.Add(new AppDocument
                {
                    FileName = doc.FileName ?? "Document",
                    FileSize = "Unknown",
                    Category = "BR",
                    ImagePath = doc.Url ?? string.Empty,
                    IsExisting = true
                });
            }
        }
        if (Record.TinAttachments != null)
        {
            foreach (var doc in Record.TinAttachments)
            {
                TinDocuments.Add(new AppDocument
                {
                    FileName = doc.FileName ?? "Document",
                    FileSize = "Unknown",
                    Category = "TIN",
                    ImagePath = doc.Url ?? string.Empty,
                    IsExisting = true
                });
            }
        }
        if (Record.Form01Attachments != null)
        {
            foreach (var doc in Record.Form01Attachments)
            {
                Form01Documents.Add(new AppDocument
                {
                    FileName = doc.FileName ?? "Document",
                    FileSize = "Unknown",
                    Category = "Form 01",
                    ImagePath = doc.Url ?? string.Empty,
                    IsExisting = true
                });
            }
        }
        if (Record.ArticleOfAssociationAttachments != null)
        {
            foreach (var doc in Record.ArticleOfAssociationAttachments)
            {
                ArticleOfAssociationDocuments.Add(new AppDocument
                {
                    FileName = doc.FileName ?? "Document",
                    FileSize = "Unknown",
                    Category = "Article of Association",
                    ImagePath = doc.Url ?? string.Empty,
                    IsExisting = true
                });
            }
        }
        if (Record.NicAttachments != null)
        {
            foreach (var doc in Record.NicAttachments)
            {
                NicDocuments.Add(new AppDocument
                {
                    FileName = doc.FileName ?? "Document",
                    FileSize = "Unknown",
                    Category = "NIC",
                    ImagePath = doc.Url ?? string.Empty,
                    IsExisting = true
                });
            }
        }
    }

    [RelayCommand]
    private void PreviewDocument(AppDocument doc)
    {
        if (doc == null) return;
        var path = doc.ImagePath ?? doc.FileName;
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase) || System.IO.File.Exists(path))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
            else
            {
                NotificationService.Instance.AddNotification("Preview", $"Previewing document: {doc.FileName}");
            }
        }
        catch (Exception ex)
        {
            NotificationService.Instance.AddNotification("Error", $"Could not open preview: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DownloadDocument(AppDocument doc)
    {
        if (doc == null) return;
        var url = doc.ImagePath;
        if (!string.IsNullOrEmpty(url) && url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await ApiService.Instance.DownloadDocumentAsync(url, doc.FileName ?? "download");
                NotificationService.Instance.AddNotification("Download", $"Downloaded document: {doc.FileName}");
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download document: {ex.Message}");
            }
        }
        else
        {
            NotificationService.Instance.AddNotification("Download", $"Downloading document: {doc.FileName}");
        }
    }

    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private bool _isGuideVisible;

    [RelayCommand]
    private void EditRecord()
    {
        if (MainViewModel.Instance != null && Record != null)
        {
            MainViewModel.Instance.NavigateToClientEditRecord(Record);
        }
    }

    [RelayCommand]
    private void DeleteRecord()
    {
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmDelete()
    {
        IsDeleteConfirmVisible = false;
        if (Record != null)
        {
            string clientName = Record.Name ?? "Unknown";
            await DataService.Instance.DeleteClientsAsync(new[] { Record });
            LogService.Instance.AddLog("Delete", "Clients", Record.Branch ?? "Central", $"Deleted client: {clientName}");
            GoBack?.Invoke();
        }
    }

    [RelayCommand]
    private void OpenGuide()
    {
        IsGuideVisible = true;
    }

    [RelayCommand]
    private void CloseGuide()
    {
        IsGuideVisible = false;
    }
}
