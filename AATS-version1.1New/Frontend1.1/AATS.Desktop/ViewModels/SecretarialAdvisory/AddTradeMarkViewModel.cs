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

    [ObservableProperty] private string _paymentOption = "Cash";
    [ObservableProperty] private string _paymentStatus = "Unpaid";

    public bool IsOptionCash { get => PaymentOption == "Cash"; set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
    public bool IsOptionOnline { get => PaymentOption == "Online"; set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
    public bool IsOptionCheque { get => PaymentOption == "Cheque"; set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }

    public bool IsStatusPaid { get => PaymentStatus == "Paid"; set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
    public bool IsStatusUnpaid { get => PaymentStatus == "Unpaid"; set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
    public bool IsStatusPartial { get => PaymentStatus == "Partial"; set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }

    // Cheque Details
    [ObservableProperty] private string _chequeBank = string.Empty;
    [ObservableProperty] private string _chequeNumber = string.Empty;
    [ObservableProperty] private DateTime? _chequeDate = DateTime.Now;
    [ObservableProperty] private decimal? _chequeAmount = 0.00m;
    [ObservableProperty] private string _chequeStatus = "Pending";

    public ObservableCollection<string> ChequeStatusOptions { get; } = new()
    {
        "Pending", "Cleared", "Bounced"
    };

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
        
        PaymentStatus = record.PaymentStatus ?? "Unpaid";
        PaymentOption = record.PaymentOption ?? "Cash";

        IsStatusPaid = PaymentStatus == "Paid";
        IsStatusUnpaid = PaymentStatus == "Unpaid";
        IsStatusPartial = PaymentStatus == "Partial";
        
        IsOptionCash = PaymentOption == "Cash";
        IsOptionOnline = PaymentOption == "Online";
        IsOptionCheque = PaymentOption == "Cheque";

        // Pre-fill Cheque Details
        ChequeBank = record.ChequeBank ?? string.Empty;
        ChequeNumber = record.ChequeNumber ?? string.Empty;
        ChequeDate = record.ChequeDate ?? DateTime.Now;
        ChequeAmount = record.ChequeAmount ?? 0.00m;
        ChequeStatus = record.ChequeStatus ?? "Pending";
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
            
            _existingRecord.PaymentStatus = PaymentStatus;
            _existingRecord.PaymentOption = PaymentOption;
            
            // Map Cheque Details
            _existingRecord.ChequeBank = ChequeBank;
            _existingRecord.ChequeNumber = ChequeNumber;
            _existingRecord.ChequeDate = ChequeDate;
            _existingRecord.ChequeAmount = ChequeAmount;
            _existingRecord.ChequeStatus = ChequeStatus;

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
                PaymentStatus = PaymentStatus,
                PaymentOption = PaymentOption,
                
                // Map Cheque Details
                ChequeBank = ChequeBank,
                ChequeNumber = ChequeNumber,
                ChequeDate = ChequeDate,
                ChequeAmount = ChequeAmount,
                ChequeStatus = ChequeStatus,
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