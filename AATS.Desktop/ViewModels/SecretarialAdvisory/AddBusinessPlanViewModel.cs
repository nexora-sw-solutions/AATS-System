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

public partial class AddBusinessPlanViewModel : ViewModelBase
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

    private AuditRecord? _originalRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public AddBusinessPlanViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddBusinessPlanViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync(() => ClientId);
        _originalRecord = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Code = record.Code ?? string.Empty;
        Assignment = record.Assignment ?? string.Empty;

        if (record.SourceDocuments != null)
        {
            foreach (var doc in record.SourceDocuments)
            {
                var file = doc.Url ?? doc.FileName;
                if (!string.IsNullOrEmpty(file))
                {
                    UploadedFiles.Add(file);
                    FileCount++;
                }
            }
        }
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadDocument()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (!UploadedFiles.Contains(file))
                    {
                        UploadedFiles.Add(file);
                        FileCount++;
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
            if (UploadedFiles.Contains(_pendingFileToRemove))
            {
                UploadedFiles.Remove(_pendingFileToRemove);
                FileCount--;
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
        if (UploadedFiles.Contains(fileName))
        {
            UploadedFiles.Remove(fileName);
            FileCount--;
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
        ConfirmSaveMessage = "Are you sure you want to create this new record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        try
        {
            var tempId = _originalRecord?.ID ?? Guid.NewGuid().ToString();
            
            var localFiles = UploadedFiles.Where(f => !f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            var existingUrls = UploadedFiles.Where(f => f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var uploadedDocs = new List<SourceDocument>();
            foreach (var url in existingUrls)
            {
                var name = System.IO.Path.GetFileName(new Uri(url).LocalPath);
                uploadedDocs.Add(new SourceDocument { FileName = name, Url = url, Description = "Business Plan Document" });
            }
            
            if (localFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded)
                {
                    uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Business Plan Document" });
                }
            }
            if (_originalRecord == null && _selectedClient != null)
            {
                var clientDocs = _selectedClient.GetAllClientDocuments();
                foreach (var doc in clientDocs)
                {
                    if (!uploadedDocs.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                    {
                        uploadedDocs.Add(doc);
                    }
                }
            }

            if (_originalRecord != null)
            {
                // Update existing
                _originalRecord.ClientCode = ClientId;
                _originalRecord.ClientId = _clientGuid;
                _originalRecord.BranchId = _branchGuid;
                _originalRecord.Date = Date ?? DateTime.Now;
                _originalRecord.ClientName = ClientName;
                _originalRecord.Company = CompanyName;
                _originalRecord.Code = Code;
                _originalRecord.Assignment = Assignment;
                _originalRecord.SourceDocuments = uploadedDocs;

                await DataService.Instance.UpdateAuditRecordAsync("Business Plan and Asset Valuation Consulting", _originalRecord);
            }
            else
            {
                // Create new
                var newRecord = new AuditRecord
                {
                    ClientCode = ClientId,
                    ClientId = _clientGuid,
                    BranchId = _branchGuid,
                    Branch = _branchName,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Company = CompanyName,
                    Code = Code,
                    Assignment = Assignment,
                    PaymentStatus = "PENDING",
                    Process = "PENDING",
                    CurrentStep = 1,
                    SourceDocuments = uploadedDocs
                };

                await DataService.Instance.AddAuditRecordAsync("Business Plan and Asset Valuation Consulting", newRecord);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving record: {ex.Message}");
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

    private ClientRecord? _selectedClient;

    public override void SelectClientCode(ClientRecord client)
    {
        _isSelectingClient = true;
        _selectedClient = client;
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;
        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }
}

