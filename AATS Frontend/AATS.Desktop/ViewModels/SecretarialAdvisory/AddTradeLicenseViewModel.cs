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

public partial class AddTradeLicenseViewModel : ViewModelBase
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
    
    // Assignment (Large Text Area)
    [ObservableProperty] private string _assignment = string.Empty;

    // Documents
    public ObservableCollection<string> UploadedFiles { get; } = new();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFiles))]
    [NotifyPropertyChangedFor(nameof(UploadedStatus))]
    private int _fileCount = 0;

    public bool HasFiles => FileCount > 0;
    public string UploadedStatus => FileCount == 0 ? "No files uploaded" : $"{FileCount} file{(FileCount > 1 ? "s" : "")} uploaded";

    [ObservableProperty] private bool _isRemoveConfirmVisible = false;
    [ObservableProperty] private string _removeConfirmTitle = string.Empty;
    [ObservableProperty] private string _removeConfirmMessage = string.Empty;
    private string? _pendingFileToRemove;

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    private readonly AuditRecord? _existingRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;
    public bool IsEditMode => _existingRecord != null;

    public AddTradeLicenseViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddTradeLicenseViewModel(AuditRecord record)
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
        Assignment = record.Assignment ?? string.Empty;
        
        if (record.SourceDocuments != null)
        {
            foreach (var doc in record.SourceDocuments)
            {
                var tab = !string.IsNullOrWhiteSpace(doc.Description) ? doc.Description : "General";
                if (!UploadedFilesByCategory.ContainsKey(tab))
                    UploadedFilesByCategory[tab] = new ObservableCollection<string>();
                
                if (!string.IsNullOrEmpty(doc.Url)) UploadedFilesByCategory[tab].Add(doc.Url);
                else if (!string.IsNullOrEmpty(doc.FileName)) UploadedFilesByCategory[tab].Add(doc.FileName);
            }
        }
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadDocument(string category)
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                var activeTab = category == "Corporate" ? SelectedCorporateDocumentTab : SelectedSupportingDocumentTab;
                foreach (var file in files)
                {
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        if (!UploadedFilesByCategory.ContainsKey(activeTab))
                            UploadedFilesByCategory[activeTab] = new ObservableCollection<string>();
                        
                        UploadedFilesByCategory[activeTab].Add(file);
                        
                        if (category == "Corporate")
                            OnPropertyChanged(nameof(CurrentCorporateDocuments));
                        else
                            OnPropertyChanged(nameof(CurrentSupportingDocuments));
                    }
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveFileConfirm(string fileName)
    {
        _pendingFileToRemove = fileName;
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{System.IO.Path.GetFileName(fileName)}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmRemove()
    {
        if (!string.IsNullOrEmpty(_pendingFileToRemove))
        {
            foreach (var kvp in UploadedFilesByCategory)
            {
                if (kvp.Value.Contains(_pendingFileToRemove))
                {
                    kvp.Value.Remove(_pendingFileToRemove);
                    OnPropertyChanged(nameof(CurrentCorporateDocuments));
                    OnPropertyChanged(nameof(CurrentSupportingDocuments));
                    break;
                }
            }
        }
        
        CancelRemove();
    }

    [RelayCommand]
    private void CancelRemove()
    {
        IsRemoveConfirmVisible = false;
        _pendingFileToRemove = null;
    }

    [RelayCommand]
    private void PreviewDocument(string filePath)
    {
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

    [RelayCommand]
    private void RemoveFile(string fileName)
    {
        foreach (var kvp in UploadedFilesByCategory)
        {
            if (kvp.Value.Contains(fileName))
            {
                kvp.Value.Remove(fileName);
                OnPropertyChanged(nameof(CurrentCorporateDocuments));
                OnPropertyChanged(nameof(CurrentSupportingDocuments));
                break;
            }
        }
    }

    // UI State
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;

    // Tabs
    private string _selectedCorporateDocumentTab = "BR";
    public string SelectedCorporateDocumentTab
    {
        get => _selectedCorporateDocumentTab;
        set
        {
            OnPropertyChanging(nameof(SelectedCorporateDocumentTab));
            _selectedCorporateDocumentTab = value;
            OnPropertyChanged(nameof(SelectedCorporateDocumentTab));
            OnPropertyChanged(nameof(CurrentCorporateDocuments));
        }
    }

    private string _selectedSupportingDocumentTab = "Photos";
    public string SelectedSupportingDocumentTab
    {
        get => _selectedSupportingDocumentTab;
        set
        {
            OnPropertyChanging(nameof(SelectedSupportingDocumentTab));
            _selectedSupportingDocumentTab = value;
            OnPropertyChanged(nameof(SelectedSupportingDocumentTab));
            OnPropertyChanged(nameof(CurrentSupportingDocuments));
        }
    }

    public System.Collections.Generic.Dictionary<string, ObservableCollection<string>> UploadedFilesByCategory { get; } = new();

    public ObservableCollection<string> CurrentCorporateDocuments
    {
        get
        {
            if (!UploadedFilesByCategory.ContainsKey(SelectedCorporateDocumentTab))
                UploadedFilesByCategory[SelectedCorporateDocumentTab] = new ObservableCollection<string>();
            return UploadedFilesByCategory[SelectedCorporateDocumentTab];
        }
    }

    public ObservableCollection<string> CurrentSupportingDocuments
    {
        get
        {
            if (!UploadedFilesByCategory.ContainsKey(SelectedSupportingDocumentTab))
                UploadedFilesByCategory[SelectedSupportingDocumentTab] = new ObservableCollection<string>();
            return UploadedFilesByCategory[SelectedSupportingDocumentTab];
        }
    }

    [RelayCommand]
    private void SelectCorporateDocumentTab(string tabName) => SelectedCorporateDocumentTab = tabName;

    [RelayCommand]
    private void SelectSupportingDocumentTab(string tabName) => SelectedSupportingDocumentTab = tabName;
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
        ConfirmSaveMessage = "Are you sure you want to create this new trade license record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        try
        {
            var tempId = _existingRecord?.ID ?? Guid.NewGuid().ToString();
            
            var uploadedDocs = new List<SourceDocument>();
            
            foreach (var kvp in UploadedFilesByCategory)
            {
                var tabName = kvp.Key;
                var localFiles = kvp.Value.Where(f => !f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
                var existingUrls = kvp.Value.Where(f => f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();

                // Keep existing URL-based docs
                foreach (var url in existingUrls)
                {
                    uploadedDocs.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(new Uri(url).LocalPath), Url = url, Description = tabName });
                }

                // Upload new local files
                if (localFiles.Count > 0)
                {
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Secretarial & Advisory", tempId);
                    foreach (var u in uploaded)
                    {
                        uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = tabName });
                    }
                }
            }

            if (IsEditMode && _existingRecord != null)
            {
                _existingRecord.ClientCode = ClientId;
                _existingRecord.ClientId = _clientGuid;
                _existingRecord.BranchId = _branchGuid;
                _existingRecord.Date = Date ?? DateTime.Now;
                _existingRecord.ClientName = ClientName;
                _existingRecord.Company = CompanyName;
                _existingRecord.Assignment = Assignment;
                _existingRecord.SourceDocuments = uploadedDocs;
                
                await DataService.Instance.UpdateAuditRecordAsync("Trade License", _existingRecord);
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
                    Assignment = Assignment,
                    PaymentStatus = "PENDING",
                    Process = "PENDING",
                    CurrentStep = 1,
                    SourceDocuments = uploadedDocs
                };
                
                await DataService.Instance.AddAuditRecordAsync("Trade License", newRecord);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Trade License record: {ex.Message}");
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

        if (!IsEditMode && client != null)
        {
            if (client.BrAttachments != null && client.BrAttachments.Count > 0)
            {
                var brList = UploadedFilesByCategory.ContainsKey("BR") ? UploadedFilesByCategory["BR"] : (UploadedFilesByCategory["BR"] = new());
                foreach (var d in client.BrAttachments)
                {
                    var path = !string.IsNullOrEmpty(d.Url) ? d.Url : d.FileName;
                    if (!string.IsNullOrEmpty(path) && !brList.Contains(path)) brList.Add(path);
                }
            }
            if (client.TinAttachments != null && client.TinAttachments.Count > 0)
            {
                var tList = UploadedFilesByCategory.ContainsKey("TIN") ? UploadedFilesByCategory["TIN"] : (UploadedFilesByCategory["TIN"] = new());
                foreach (var d in client.TinAttachments)
                {
                    var path = !string.IsNullOrEmpty(d.Url) ? d.Url : d.FileName;
                    if (!string.IsNullOrEmpty(path) && !tList.Contains(path)) tList.Add(path);
                }
            }
            if (client.Form01Attachments != null && client.Form01Attachments.Count > 0)
            {
                var fList = UploadedFilesByCategory.ContainsKey("Form 01") ? UploadedFilesByCategory["Form 01"] : (UploadedFilesByCategory["Form 01"] = new());
                foreach (var d in client.Form01Attachments)
                {
                    var path = !string.IsNullOrEmpty(d.Url) ? d.Url : d.FileName;
                    if (!string.IsNullOrEmpty(path) && !fList.Contains(path)) fList.Add(path);
                }
            }
        }

        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }
}

