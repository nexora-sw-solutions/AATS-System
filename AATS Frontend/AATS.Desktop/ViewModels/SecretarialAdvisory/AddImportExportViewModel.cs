using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddImportExportViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Client ID is required")]
    private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;

        partial void OnClientNameChanged(string value)
        {
            FilterClientNames(value);
        }
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Company name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    private string _companyName = string.Empty;

        partial void OnCompanyNameChanged(string value)
        {
            FilterClientNames(value);
        }
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _tin = string.Empty;
    [ObservableProperty] private string _assignment = string.Empty;
    [ObservableProperty] private string _country = string.Empty;

        public List<string> Countries => AATS.Desktop.Data.MockData.Countries;

    // Documents
    [ObservableProperty] private string _selectedCard1Tab = "TIN";
    [ObservableProperty] private string _selectedCard2Tab = "Utility";

    [RelayCommand] private void SelectCard1Tab(string tab) => SelectedCard1Tab = tab;
    [RelayCommand] private void SelectCard2Tab(string tab) => SelectedCard2Tab = tab;

    public ObservableCollection<SourceDocument> TinAttachments { get; } = new();
    public ObservableCollection<SourceDocument> ArticleOfAssociationAttachments { get; } = new();
    public ObservableCollection<SourceDocument> BrAttachments { get; } = new();
    public ObservableCollection<SourceDocument> Form01Attachments { get; } = new();
    public ObservableCollection<SourceDocument> BankStatementAttachments { get; } = new();
    public ObservableCollection<SourceDocument> UtilityAttachments { get; } = new();
    public ObservableCollection<SourceDocument> BlAttachments { get; } = new();
    public ObservableCollection<SourceDocument> CommercialInvoiceAttachments { get; } = new();
    public ObservableCollection<SourceDocument> GsInvoiceAttachments { get; } = new();
    public ObservableCollection<SourceDocument> CompanyLetterAttachments { get; } = new();

    [ObservableProperty] private bool _isRemoveConfirmVisible = false;
    [ObservableProperty] private string _removeConfirmTitle = string.Empty;
    [ObservableProperty] private string _removeConfirmMessage = string.Empty;
    private SourceDocument? _pendingDocumentToRemove;
    private string _pendingDocumentCategory = string.Empty;

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    private AuditRecord? _existingRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public AddImportExportViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddImportExportViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync(() => ClientId);
        _existingRecord = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Tin = record.TIN ?? string.Empty;
        Assignment = record.Assignment ?? string.Empty;
        Country = record.Country ?? string.Empty;

        if (record.SourceDocuments != null)
        {
            foreach (var doc in record.SourceDocuments)
            {
                switch (doc.Description)
                {
                    case "TIN":
                        TinAttachments.Add(doc);
                        break;
                    case "Article of Association":
                        ArticleOfAssociationAttachments.Add(doc);
                        break;
                    case "BR":
                        BrAttachments.Add(doc);
                        break;
                    case "Form 01":
                        Form01Attachments.Add(doc);
                        break;
                    case "Bank Statement":
                        BankStatementAttachments.Add(doc);
                        break;
                    case "Utility":
                        UtilityAttachments.Add(doc);
                        break;
                    case "BL":
                        BlAttachments.Add(doc);
                        break;
                    case "Commercial Invoice":
                        CommercialInvoiceAttachments.Add(doc);
                        break;
                    case "GS Invoice":
                        GsInvoiceAttachments.Add(doc);
                        break;
                    case "Company Letter":
                        CompanyLetterAttachments.Add(doc);
                        break;
                    default:
                        // If there's an uncategorized document, we can drop it in Company Letter or just ignore. 
                        // We'll put it in Company Letter as a fallback.
                        CompanyLetterAttachments.Add(doc);
                        break;
                }
            }
        }
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;


    [RelayCommand]
    private async System.Threading.Tasks.Task PickTinAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) TinAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "TIN" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveTinAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "Tin";
        RemoveConfirmTitle = "Remove TIN Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickArticleOfAssociationAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) ArticleOfAssociationAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Article of Association" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveArticleOfAssociationAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "ArticleOfAssociation";
        RemoveConfirmTitle = "Remove Article of Association Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBrAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) BrAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "BR" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveBrAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "Br";
        RemoveConfirmTitle = "Remove BR Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickForm01Attachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) Form01Attachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Form 01" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveForm01AttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "Form01";
        RemoveConfirmTitle = "Remove Form 01 Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBankStatementAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) BankStatementAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Bank Statement" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveBankStatementAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "BankStatement";
        RemoveConfirmTitle = "Remove Bank Statement Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickUtilityAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) UtilityAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Utility" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveUtilityAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "Utility";
        RemoveConfirmTitle = "Remove Utility Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBlAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) BlAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "BL" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveBlAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "Bl";
        RemoveConfirmTitle = "Remove BL Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickCommercialInvoiceAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) CommercialInvoiceAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Commercial Invoice" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveCommercialInvoiceAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "CommercialInvoice";
        RemoveConfirmTitle = "Remove Commercial Invoice Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickGsInvoiceAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) GsInvoiceAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "GS Invoice" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveGsInvoiceAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "GsInvoice";
        RemoveConfirmTitle = "Remove GS Invoice Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickCompanyLetterAttachment()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files) CompanyLetterAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Company Letter" });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveCompanyLetterAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        _pendingDocumentToRemove = doc;
        _pendingDocumentCategory = "CompanyLetter";
        RemoveConfirmTitle = "Remove Company Letter Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmRemove()
    {
        if (_pendingDocumentToRemove != null)
        {
            if (_pendingDocumentCategory == "Tin") TinAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "ArticleOfAssociation") ArticleOfAssociationAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "Br") BrAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "Form01") Form01Attachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "BankStatement") BankStatementAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "Utility") UtilityAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "Bl") BlAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "CommercialInvoice") CommercialInvoiceAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "GsInvoice") GsInvoiceAttachments.Remove(_pendingDocumentToRemove);
            if (_pendingDocumentCategory == "CompanyLetter") CompanyLetterAttachments.Remove(_pendingDocumentToRemove);
        }
        CancelRemove();
    }

    [RelayCommand]
    private void CancelRemove()
    {
        IsRemoveConfirmVisible = false;
        _pendingDocumentToRemove = null;
        _pendingDocumentCategory = string.Empty;
    }

    [RelayCommand]
    private void PreviewDocument(SourceDocument doc)
    {
        var filePath = doc?.Url ?? doc?.FileName;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error previewing document: {ex.Message}");
            }
        }
    }



    // UI State
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
    [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

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

        ConfirmSaveTitle = "Save Record?";
        ConfirmSaveMessage = "Are you sure you want to create this new import and export clearance record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        try
        {
            var tempId = _existingRecord?.ID ?? Guid.NewGuid().ToString();
            
            var allDocs = new List<SourceDocument>();
            allDocs.AddRange(TinAttachments);
            allDocs.AddRange(ArticleOfAssociationAttachments);
            allDocs.AddRange(BrAttachments);
            allDocs.AddRange(Form01Attachments);
            allDocs.AddRange(BankStatementAttachments);
            allDocs.AddRange(UtilityAttachments);
            allDocs.AddRange(BlAttachments);
            allDocs.AddRange(CommercialInvoiceAttachments);
            allDocs.AddRange(GsInvoiceAttachments);
            allDocs.AddRange(CompanyLetterAttachments);

            var localFiles = allDocs.Where(f => !f.Url!.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            var existingUrls = allDocs.Where(f => f.Url!.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var uploadedDocs = new List<SourceDocument>();
            foreach (var doc in existingUrls)
            {
                uploadedDocs.Add(doc);
            }
            
            if (localFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles.Select(f => f.Url!).ToList(), "Secretarial & Advisory", tempId);
                foreach (var u in uploaded)
                {
                    // Find the original to keep its description
                    var original = localFiles.FirstOrDefault(f => f.FileName == u.FileName);
                    uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = original?.Description ?? "Company Letter" });
                }
            }

            if (_existingRecord != null)
            {
                _existingRecord.ClientCode = ClientId;
                _existingRecord.ClientId = _clientGuid;
                _existingRecord.BranchId = _branchGuid;
                _existingRecord.Date = Date ?? DateTime.Now;
                _existingRecord.ClientName = ClientName;
                _existingRecord.Company = CompanyName;
                _existingRecord.TIN = Tin;
                _existingRecord.Assignment = Assignment;
                _existingRecord.Country = Country;
                _existingRecord.SourceDocuments = uploadedDocs;
                
                await DataService.Instance.UpdateAuditRecordAsync("Import / Export", _existingRecord);
            }
            else
            {
                var newRecord = new AuditRecord
                {
                    ClientCode = ClientId,
                    ClientId = _clientGuid,
                    BranchId = _branchGuid,
                    Branch = _branchName,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Company = CompanyName,
                    TIN = Tin,
                    Assignment = Assignment,
                    Country = Country,
                    PaymentStatus = "PENDING",
                    Process = "DOCUMENTATION",
                    CurrentStep = 1,
                    SourceDocuments = uploadedDocs
                };
                
                await DataService.Instance.AddAuditRecordAsync("Import / Export", newRecord);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Import/Export record: {ex.Message}");
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand]
    private void CancelSave() => IsConfirmSaveVisible = false;

    [RelayCommand]
    private void DiscardChanges()
    {
        IsDiscardConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        if (GoBack != null) await GoBack();
    }

    [RelayCommand]
    private void CancelDiscard()
    {
        IsDiscardConfirmVisible = false;
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
        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }
}

