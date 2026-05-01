using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddBusinessPlanViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
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

    public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    private AuditRecord? _originalRecord;

    public AddBusinessPlanViewModel()
    {
        _ = LoadClientCodesAsync();
    }

    public AddBusinessPlanViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync();
        _originalRecord = record;
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
                UploadedFiles.Add(doc.FileName ?? "Document");
                FileCount++;
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
        ConfirmSaveTitle = "Save Record?";
        ConfirmSaveMessage = "Are you sure you want to create this new record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        if (_originalRecord != null)
        {
            // Update existing
            _originalRecord.ClientCode = ClientId;
            _originalRecord.Date = Date ?? DateTime.Now;
            _originalRecord.ClientName = ClientName;
            _originalRecord.Company = CompanyName;
            _originalRecord.Code = Code;
            _originalRecord.Assignment = Assignment;
            
            // Sync files to SourceDocuments
            _originalRecord.SourceDocuments = new List<SourceDocument>();
            foreach (var file in UploadedFiles)
            {
                _originalRecord.SourceDocuments.Add(new SourceDocument { FileName = file, Description = "Business Plan Document" });
            }

            await DataService.Instance.UpdateAuditRecordAsync("Business Plan and Asset Valuation Consulting", _originalRecord);
        }
        else
        {
            // Create new
            var newRecord = new AuditRecord
            {
                ClientCode = ClientId,
                Date = Date ?? DateTime.Now,
                ClientName = ClientName,
                Company = CompanyName,
                Code = Code,
                Assignment = Assignment,
                PaymentStatus = "PENDING",
                Process = "PENDING",
                CurrentStep = 1,
                SourceDocuments = new List<SourceDocument>()
            };

            foreach (var file in UploadedFiles)
            {
                newRecord.SourceDocuments.Add(new SourceDocument { FileName = file, Description = "Business Plan Document" });
            }
            
            await DataService.Instance.AddAuditRecordAsync("Business Plan and Asset Valuation Consulting", newRecord);
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
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        IsClientCodeDropdownOpen = false;
    }
}