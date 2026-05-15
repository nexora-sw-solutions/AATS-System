using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Helpers;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddCompanyRegistrationViewModel : ViewModelBase
{
    private bool _isEdit = false;
    private AuditRecord? _originalRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    // General Details
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phoneNo = string.Empty;
    [ObservableProperty] private string _objective = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    // NIC Upload
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasNicFile))]
    private string? _nicFileName;
    public bool HasNicFile => !string.IsNullOrEmpty(NicFileName);

    // Payment Details
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount;

    public decimal TotalPayment => Math.Max(0, SubTotal - Discount);

    [ObservableProperty] private decimal _partialAmount;
    
    [ObservableProperty] private string _paymentOption = "Cash";
    [ObservableProperty] private string _paymentStatus = "Paid";

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

    public new List<string> Banks => BankHelper.GetBanks();

    
    // Type selection
    public ObservableCollection<string> CompanyTypes { get; } = new() 
    { 
        "Proprietary (Private LTD)", 
        "Public Company", 
        "Sole Proprietorship", 
        "Partnership", 
        "Corporation (LLC/Inc)", 
        "Nonprofit Corporations", 
        "Other" 
    };

    // Dynamic Collections
    public ObservableCollection<CompanyCharacter> Directors { get; } = new();
    public ObservableCollection<CompanyCharacter> Shareholders { get; } = new();
    public ObservableCollection<CompanyCharacter> Secretaries { get; } = new();
    public ObservableCollection<CompanyCharacter> Others { get; } = new();

    // UI State
    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private bool _isRemoveConfirmVisible = false;
    [ObservableProperty] private string _removeConfirmTitle = string.Empty;
    [ObservableProperty] private string _removeConfirmMessage = string.Empty;
    private string? _pendingFileToRemove;
    [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
    [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public Func<System.Threading.Tasks.Task<string?>>? RequestNicPicker { get; set; }

    public AddCompanyRegistrationViewModel()
    {
        _isEdit = false;
        Type = CompanyTypes[0];
        _ = LoadClientCodesAsync();
    }

    public AddCompanyRegistrationViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync();
        _isEdit = true;
        _originalRecord = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Type = record.Type ?? string.Empty;
        Address = record.Address ?? string.Empty;
        Email = record.Email ?? string.Empty;
        PhoneNo = record.PhoneNo ?? string.Empty;
        Objective = record.Assignment ?? string.Empty;
        Description = record.Description ?? string.Empty;
        
        SubTotal = record.SubTotal;
        Discount = record.Discount;
        PartialAmount = record.PartialAmount;
        PaymentOption = record.PaymentOption ?? "Cash";
        PaymentStatus = record.PaymentStatus ?? "Paid";
        
        IsOptionCash = PaymentOption == "Cash";
        IsOptionOnline = PaymentOption == "Online";
        IsOptionCheque = PaymentOption == "Cheque";

        IsStatusPaid = PaymentStatus == "Paid";
        IsStatusUnpaid = PaymentStatus == "Unpaid";
        IsStatusPartial = PaymentStatus == "Partial";

        // Pre-fill Cheque Details
        ChequeBank = record.ChequeBank ?? string.Empty;
        ChequeNumber = record.ChequeNumber ?? string.Empty;
        ChequeDate = record.ChequeDate ?? DateTime.Now;
        ChequeAmount = record.ChequeAmount ?? 0.00m;
        ChequeStatus = record.ChequeStatus ?? "Pending";
        
        // Load collections if they exist
        if (record.DirectorsList != null) foreach (var d in record.DirectorsList) Directors.Add(new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email });
        if (record.ShareholdersList != null) foreach (var s in record.ShareholdersList) Shareholders.Add(new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email });
        if (record.SecretariesList != null) foreach (var sc in record.SecretariesList) Secretaries.Add(new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email });
        if (record.OthersList != null) foreach (var o in record.OthersList) Others.Add(new CompanyCharacter { Detail = o.Detail, Role = o.Role });

        if (record.SourceDocuments != null && record.SourceDocuments.Count > 0)
        {
            NicFileName = record.SourceDocuments.FirstOrDefault(d => d.Description == "NIC Document")?.FileName;
        }
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand] private void AddDirector() => Directors.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveDirector(CompanyCharacter c) => Directors.Remove(c);
    [RelayCommand] private void AddShareholder() => Shareholders.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveShareholder(CompanyCharacter c) => Shareholders.Remove(c);
    [RelayCommand] private void AddSecretary() => Secretaries.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveSecretary(CompanyCharacter c) => Secretaries.Remove(c);
    [RelayCommand] private void AddOther() => Others.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveOther(CompanyCharacter c) => Others.Remove(c);

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadNic()
    {
        if (RequestNicPicker != null)
        {
            var file = await RequestNicPicker();
            if (file != null)
            {
                NicFileName = file;
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveNicConfirm()
    {
        if (!HasNicFile) return;
        _pendingFileToRemove = NicFileName;
        RemoveConfirmTitle = "Remove NIC?";
        RemoveConfirmMessage = $"Are you sure you want to remove the uploaded NIC document '{System.IO.Path.GetFileName(NicFileName)}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmRemove()
    {
        NicFileName = null;
        CancelRemove();
    }

    [RelayCommand]
    private void CancelRemove()
    {
        IsRemoveConfirmVisible = false;
        _pendingFileToRemove = null;
    }

    [RelayCommand]
    private void PreviewNic()
    {
        if (string.IsNullOrWhiteSpace(NicFileName)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(NicFileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error previewing NIC: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SaveRecord()
    {
        // Basic Validation
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            FormErrorMessage = "Client ID is required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(ClientName))
        {
            FormErrorMessage = "Client Name is required.";
            return;
        }

        ConfirmSaveTitle = _isEdit ? "Save Changes?" : "Create Record?";
        ConfirmSaveMessage = _isEdit 
            ? "Are you sure you want to save the modifications to this record?" 
            : "Are you sure you want to create this new company registration record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        if (_isEdit && _originalRecord != null)
        {
            _originalRecord.ClientCode = ClientId;
            _originalRecord.ClientId = _clientGuid;
            _originalRecord.BranchId = _branchGuid ;
            _originalRecord.Date = Date ?? DateTime.Now;
            _originalRecord.ClientName = ClientName;
            _originalRecord.Company = CompanyName;
            _originalRecord.Type = Type;
            _originalRecord.Address = Address;
            _originalRecord.Email = Email;
            _originalRecord.PhoneNo = PhoneNo;
            _originalRecord.Assignment = Objective;
            _originalRecord.Description = Description;
            
            _originalRecord.SubTotal = SubTotal;
            _originalRecord.Discount = Discount;
            _originalRecord.TotalPayment = TotalPayment;
            _originalRecord.PartialAmount = (PaymentStatus == "Paid") ? TotalPayment : (PaymentStatus == "Partial" ? TotalPayment / 2 : 0);
            _originalRecord.PaymentOption = PaymentOption;
            _originalRecord.PaymentStatus = PaymentStatus;
            
            // Map Cheque Details
            _originalRecord.ChequeBank = ChequeBank;
            _originalRecord.ChequeNumber = ChequeNumber;
            _originalRecord.ChequeDate = ChequeDate;
            _originalRecord.ChequeAmount = ChequeAmount;
            _originalRecord.ChequeStatus = ChequeStatus;
            
            _originalRecord.DirectorsList = Directors.Select(d => new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email }).ToList();
            _originalRecord.ShareholdersList = Shareholders.Select(s => new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email }).ToList();
            _originalRecord.SecretariesList = Secretaries.Select(sc => new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email }).ToList();
            _originalRecord.OthersList = Others.Select(o => new CompanyCharacter { Detail = o.Detail, Role = o.Role }).ToList();

            // Sync NIC file
            if (_originalRecord.SourceDocuments == null) _originalRecord.SourceDocuments = new List<SourceDocument>();
            _originalRecord.SourceDocuments.RemoveAll(d => d.Description == "NIC Document");
            if (HasNicFile)
            {
                _originalRecord.SourceDocuments.Add(new SourceDocument { FileName = NicFileName, Description = "NIC Document" });
            }

            await DataService.Instance.UpdateAuditRecordAsync("Company Registration", _originalRecord);
        }
        else
        {
            var newRecord = new AuditRecord
            {
                ClientCode = ClientId,
                ClientId = _clientGuid,
                BranchId = _branchGuid ,
                Branch = _branchName,
                Date = Date ?? DateTime.Now,
                ClientName = ClientName,
                Company = CompanyName,
                Type = Type,
                Address = Address,
                Email = Email,
                PhoneNo = PhoneNo,
                Assignment = Objective,
                Description = Description,
                SubTotal = SubTotal,
                Discount = Discount,
                TotalPayment = TotalPayment,
                PartialAmount = (PaymentStatus == "Paid") ? TotalPayment : (PaymentStatus == "Partial" ? TotalPayment / 2 : 0),
                PaymentOption = PaymentOption,
                PaymentStatus = PaymentStatus,
                Process = "NAME APPROVAL",
                CurrentStep = 1,
                
                // Map Cheque Details
                ChequeBank = ChequeBank,
                ChequeNumber = ChequeNumber,
                ChequeDate = ChequeDate,
                ChequeAmount = ChequeAmount,
                ChequeStatus = ChequeStatus,
                DirectorsList = Directors.Select(d => new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email }).ToList(),
                ShareholdersList = Shareholders.Select(s => new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email }).ToList(),
                SecretariesList = Secretaries.Select(sc => new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email }).ToList(),
                OthersList = Others.Select(o => new CompanyCharacter { Detail = o.Detail, Role = o.Role }).ToList(),
                SourceDocuments = new List<SourceDocument>()
            };

            if (HasNicFile)
            {
                newRecord.SourceDocuments.Add(new SourceDocument { FileName = NicFileName, Description = "NIC Document" });
            }
            
            await DataService.Instance.AddAuditRecordAsync("Company Registration", newRecord);
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelSave() => IsConfirmSaveVisible = false;
    [RelayCommand] private void DiscardChanges() => IsDiscardConfirmVisible = true;
    [RelayCommand] private async System.Threading.Tasks.Task ConfirmDiscard() { IsDiscardConfirmVisible = false; if (GoBack != null) await GoBack(); }
    [RelayCommand] private void CancelDiscard() => IsDiscardConfirmVisible = false;

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
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;
        IsClientCodeDropdownOpen = false;
    }

    public override void SelectBank(string bank)
    {
        ChequeBank = bank;
        IsBankDropdownOpen = false;
    }
}
