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

public partial class AddTradeMarkViewModel : ViewModelBase
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
    
    // Description (Right Column)
    [ObservableProperty] private string _description = string.Empty;

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _partialAmount = 0.00m;

    public decimal TotalPayment => PaymentStatus switch
    {
        "Paid" => 0,
        "Partial" => Math.Max(0, SubTotal - Discount - PartialAmount),
        _ => Math.Max(0, SubTotal - Discount)
    };

    public bool IsPaymentStatusPartial => PaymentStatus == "Partial";

    partial void OnPaymentStatusChanged(string value)
    {
        if (value != "Partial")
        {
            PartialAmount = 0;
        }
    }

    [ObservableProperty] private string _paymentOption = "Cash";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    [NotifyPropertyChangedFor(nameof(IsPaymentStatusPartial))]
    private string _paymentStatus = "Unpaid";

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
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddTradeMarkViewModel(AuditRecord record)
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
        Description = record.Assignment ?? string.Empty;
        
        PaymentStatus = record.PaymentStatus ?? "Unpaid";
        PaymentOption = record.PaymentOption ?? "Cash";
        SubTotal = record.SubTotal;
        Discount = record.Discount;
        PartialAmount = record.PartialAmount;

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
        var clientExists = SharedClientsList.Any(c => c.ClientCode != null && c.ClientCode.Equals(ClientId, System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(ClientId) || !clientExists)
        {
            HasFormError = true;
            FormErrorMessage = "Invalid Client ID. Please select an existing client before saving.";
            return;
        }

        HasFormError = false;
        if (PaymentStatus == "Partial")
        {
            if (PartialAmount == 0)
            {
                HasFormError = true;
                FormErrorMessage = "Please enter the partially paid amount.";
                return;
            }
            if (PartialAmount < 0)
            {
                HasFormError = true;
                FormErrorMessage = "Amount must be greater than or equal to zero.";
                return;
            }
            if (PartialAmount > (SubTotal - Discount))
            {
                HasFormError = true;
                FormErrorMessage = "Partially paid amount cannot exceed the subtotal.";
                return;
            }
        }

        ConfirmSaveTitle = _existingRecord != null ? "Save Changes?" : "Save Record?";
        ConfirmSaveMessage = _existingRecord != null 
            ? "Are you sure you want to save the modifications to this record?" 
            : "Are you sure you want to create this new trade mark record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        try
        {
            var tempId = _existingRecord?.ID ?? Guid.NewGuid().ToString();
            
            var localFiles = UploadedFiles.Where(f => !f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            var existingUrls = UploadedFiles.Where(f => f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var uploadedDocs = new List<SourceDocument>();
            foreach (var url in existingUrls)
            {
                var name = System.IO.Path.GetFileName(url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(url).LocalPath : url);
                uploadedDocs.Add(new SourceDocument { FileName = name, Url = url, Description = "Uploaded document" });
            }
            
            if (localFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded)
                {
                    uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Uploaded document" });
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
                _existingRecord.Assignment = Description;
                
                _existingRecord.PaymentStatus = PaymentStatus;
                _existingRecord.PaymentOption = PaymentOption;
                _existingRecord.SubTotal = SubTotal;
                _existingRecord.Discount = Discount;
                _existingRecord.TotalPayment = TotalPayment;
                _existingRecord.PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0);
                
                // Map Cheque Details
                _existingRecord.ChequeBank = ChequeBank;
                _existingRecord.ChequeNumber = ChequeNumber;
                _existingRecord.ChequeDate = ChequeDate;
                _existingRecord.ChequeAmount = ChequeAmount;
                _existingRecord.ChequeStatus = ChequeStatus;
                _existingRecord.SourceDocuments = uploadedDocs;

                await DataService.Instance.UpdateAuditRecordAsync("Trade Mark", _existingRecord);
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
                    Assignment = Description,
                    PaymentStatus = PaymentStatus,
                    PaymentOption = PaymentOption,
                    SubTotal = SubTotal,
                    Discount = Discount,
                    TotalPayment = TotalPayment,
                    PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0),
                    
                    // Map Cheque Details
                    ChequeBank = ChequeBank,
                    ChequeNumber = ChequeNumber,
                    ChequeDate = ChequeDate,
                    ChequeAmount = ChequeAmount,
                    ChequeStatus = ChequeStatus,
                    Process = "BOOKKEEP",
                    CurrentStep = 1,
                    SourceDocuments = uploadedDocs
                };
                
                await DataService.Instance.AddAuditRecordAsync("Trade Mark", newRecord);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Trade Mark record: {ex.Message}");
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

    partial void OnChequeBankChanged(string value)
    {
        FilterBanks(value);
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

    public override void SelectBank(string bank)
    {
        ChequeBank = bank;
        IsBankDropdownOpen = false;
    }
}

