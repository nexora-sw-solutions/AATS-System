using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddTradeMarkViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    
    // Description (Right Column)
    [ObservableProperty] private string _description = string.Empty;

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0;

    public decimal TotalPayment => Math.Max(0, SubTotal - Discount);

    [ObservableProperty] private bool _isOptionCash = true;
    [ObservableProperty] private bool _isOptionOnline = false;
    [ObservableProperty] private bool _isOptionCheque = false;

    [ObservableProperty] private bool _isStatusPaid = false;
    [ObservableProperty] private bool _isStatusUnpaid = true;
    [ObservableProperty] private bool _isStatusPartial = false;

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

    private AuditRecord? _existingRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public AddTradeMarkViewModel()
    {
        _ = LoadClientCodesAsync();
    }

    public AddTradeMarkViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync();
        _existingRecord = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Description = record.Assignment ?? string.Empty;
        
        // Populate flags based on status
        IsStatusPaid = record.PaymentStatus == "Paid";
        IsStatusUnpaid = record.PaymentStatus == "Unpaid";
        IsStatusPartial = record.PaymentStatus == "Partial";
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
        ConfirmSaveMessage = "Are you sure you want to create this new trade mark record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        if (_existingRecord != null)
        {
            _existingRecord.ClientCode = ClientId;
            _existingRecord.ClientId = _clientGuid;
            _existingRecord.BranchId = _branchGuid ?? Guid.Empty;
            _existingRecord.Date = Date ?? DateTime.Now;
            _existingRecord.ClientName = ClientName;
            _existingRecord.Company = CompanyName;
            _existingRecord.Assignment = Description;
            
            if (IsStatusPaid) _existingRecord.PaymentStatus = "Paid";
            else if (IsStatusPartial) _existingRecord.PaymentStatus = "Partial";
            else _existingRecord.PaymentStatus = "Unpaid";

            await DataService.Instance.UpdateAuditRecordAsync("Trade Mark", _existingRecord);
        }
        else
        {
            var newRecord = new AuditRecord
            {
                ClientCode = ClientId,
                ClientId = _clientGuid,
                BranchId = _branchGuid ?? Guid.Empty,
                Branch = _branchName,
                Date = Date ?? DateTime.Now,
                ClientName = ClientName,
                Company = CompanyName,
                Assignment = Description,
                PaymentStatus = IsStatusPaid ? "Paid" : (IsStatusPartial ? "Partial" : "Unpaid"),
                Process = "BOOKKEEP",
                CurrentStep = 1
            };
            
            await DataService.Instance.AddAuditRecordAsync("Trade Mark", newRecord);
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
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;
        IsClientCodeDropdownOpen = false;
    }
}