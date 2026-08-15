using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddForm15ViewModel : ViewModelBase
{


    private bool _isEdit = false;
    private AuditRecord? _originalRecord;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Client ID is required")]
    private string _clientId = string.Empty;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Company name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    private string _companyName = string.Empty;
    [ObservableProperty] private string _loginId = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;

    // UI state
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;

    // Attachment Tab State
    [ObservableProperty] private string _selectedAttachmentTab = "BR";

    // Per-tab attachment collections
    [ObservableProperty] private ObservableCollection<AppDocument> _brDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _form01Documents = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _articlesDocuments = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _form20Documents = new();
    [ObservableProperty] private ObservableCollection<AppDocument> _auditReportDocuments = new();

    public Func<Task<string[]?>>? RequestMultipleFiles { get; set; }
    public Func<Task>? GoBack { get; set; }

    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public AddForm15ViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddForm15ViewModel(AuditRecord record)
    {
        _isEdit = true;
        _originalRecord = record;
        _ = LoadClientCodesAsync(() => ClientId);

        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        
        ClientId = record.ClientCode ?? string.Empty;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        LoginId = record.LoginId ?? string.Empty;
        Password = record.Password ?? string.Empty;
        Phone = record.PhoneNo ?? string.Empty;

        if (record.SourceDocuments != null)
        {
            foreach (var doc in record.SourceDocuments)
            {
                var appDoc = new AppDocument
                {
                    FileName = doc.FileName,
                    FileSize = "Existing",
                    IsExisting = true
                };

                switch (doc.Description)
                {
                    case "BR": BrDocuments.Add(appDoc); break;
                    case "Form 01": Form01Documents.Add(appDoc); break;
                    case "Articles of Association": ArticlesDocuments.Add(appDoc); break;
                    case "Form 20": Form20Documents.Add(appDoc); break;
                    case "Audit Report": AuditReportDocuments.Add(appDoc); break;
                }
            }
        }
    }


    // â”€â”€ Tab Selection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [RelayCommand] private void SelectAttachmentTab(string tab) => SelectedAttachmentTab = tab;

    // â”€â”€ Save / Discard â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [RelayCommand]
    private void SaveRecord()
    {
        HasFormError = false;
        var clientExists = SharedClientsList.Any(c => c.ClientCode != null && c.ClientCode.Equals(ClientId, System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(ClientId) || !clientExists)
        {
            HasFormError = true;
            FormErrorMessage = "Invalid Client ID. Please select an existing client before saving.";
            return;
        }

        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        AuditRecord targetRecord;

        if (_isEdit && _originalRecord != null)
        {
            targetRecord = _originalRecord;
            targetRecord.SourceDocuments?.Clear();
            if (targetRecord.SourceDocuments == null)
                targetRecord.SourceDocuments = new System.Collections.Generic.List<SourceDocument>();
        }
        else
        {
            targetRecord = new AuditRecord
            {
                ID = "F15-" + new Random().Next(1000, 9999),
                Date = DateTime.Now,
                Process = "FORM - 15",
                CurrentStep = 1,
                SourceDocuments = new System.Collections.Generic.List<SourceDocument>()
            };
        }

        targetRecord.ClientCode = ClientId;
        targetRecord.ClientId = _clientGuid;
        targetRecord.BranchId = _branchGuid;
        targetRecord.Branch = _branchName;
        targetRecord.ClientName = ClientName;
        targetRecord.Company = CompanyName;
        targetRecord.LoginId = LoginId;
        targetRecord.Password = Password;
        targetRecord.PhoneNo = Phone;

        void MapDocs(ObservableCollection<AppDocument> docs, string desc)
        {
            foreach (var d in docs)
                targetRecord.SourceDocuments.Add(new SourceDocument { FileName = d.FileName, Description = desc });
        }

        MapDocs(BrDocuments, "BR");
        MapDocs(Form01Documents, "Form 01");
        MapDocs(ArticlesDocuments, "Articles of Association");
        MapDocs(Form20Documents, "Form 20");
        MapDocs(AuditReportDocuments, "Audit Report");

        if (_isEdit)
        {
            await DataService.Instance.UpdateAuditRecordAsync("Form - 15", targetRecord);
        }
        else
        {
            await DataService.Instance.AddAuditRecordAsync("Form - 15", targetRecord);
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelSave() => IsConfirmSaveVisible = false;
    [RelayCommand] private void DiscardChanges() => IsDiscardConfirmVisible = true;

    [RelayCommand]
    private async Task ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelDiscard() => IsDiscardConfirmVisible = false;

    // â”€â”€ Per-tab Pick / Remove commands â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [RelayCommand] private async Task PickBrAttachment() => await AddDocsToCollection(BrDocuments);
    [RelayCommand] private void RemoveBrAttachment(AppDocument doc) => BrDocuments.Remove(doc);

    [RelayCommand] private async Task PickForm01Attachment() => await AddDocsToCollection(Form01Documents);
    [RelayCommand] private void RemoveForm01Attachment(AppDocument doc) => Form01Documents.Remove(doc);

    [RelayCommand] private async Task PickArticlesAttachment() => await AddDocsToCollection(ArticlesDocuments);
    [RelayCommand] private void RemoveArticlesAttachment(AppDocument doc) => ArticlesDocuments.Remove(doc);

    [RelayCommand] private async Task PickForm20Attachment() => await AddDocsToCollection(Form20Documents);
    [RelayCommand] private void RemoveForm20Attachment(AppDocument doc) => Form20Documents.Remove(doc);

    [RelayCommand] private async Task PickAuditReportAttachment() => await AddDocsToCollection(AuditReportDocuments);
    [RelayCommand] private void RemoveAuditReportAttachment(AppDocument doc) => AuditReportDocuments.Remove(doc);

    // â”€â”€ Legacy upload command (kept for backward compatibility) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [RelayCommand]
    private async Task UploadDocumentAsync(string category)
    {
        if (RequestMultipleFiles == null) return;
        var files = await RequestMultipleFiles();
        if (files == null || files.Length == 0) return;

        foreach (var file in files)
        {
            var doc = new AppDocument
            {
                FileName = System.IO.Path.GetFileName(file),
                FileSize = (new System.IO.FileInfo(file).Length / 1024) + " KB",
                IsExisting = false
            };

            switch (category)
            {
                case "BR": BrDocuments.Add(doc); break;
                case "Form01": Form01Documents.Add(doc); break;
                case "Articles": ArticlesDocuments.Add(doc); break;
                case "Form20": Form20Documents.Add(doc); break;
                case "AuditReport": AuditReportDocuments.Add(doc); break;
            }
        }
    }

    [RelayCommand]
    private void RemoveDocument(AppDocument doc)
    {
        if (BrDocuments.Contains(doc)) BrDocuments.Remove(doc);
        else if (Form01Documents.Contains(doc)) Form01Documents.Remove(doc);
        else if (ArticlesDocuments.Contains(doc)) ArticlesDocuments.Remove(doc);
        else if (Form20Documents.Contains(doc)) Form20Documents.Remove(doc);
        else if (AuditReportDocuments.Contains(doc)) AuditReportDocuments.Remove(doc);
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task AddDocsToCollection(ObservableCollection<AppDocument> collection)
    {
        if (RequestMultipleFiles == null) return;
        var files = await RequestMultipleFiles();
        if (files == null || files.Length == 0) return;
        foreach (var file in files)
        {
            collection.Add(new AppDocument
            {
                FileName = System.IO.Path.GetFileName(file),
                FileSize = (new System.IO.FileInfo(file).Length / 1024) + " KB",
                IsExisting = false
            });
        }
    }

    partial void OnClientIdChanged(string value)
    {
        FilterClientCodes(value);
    }

    public override void SelectClientCode(ClientRecord client)
    {
        _isSelectingClient = true;
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;
        
        // Form-15 specific client mapping logic
        Phone = client.Phone ?? string.Empty;

        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }
}
