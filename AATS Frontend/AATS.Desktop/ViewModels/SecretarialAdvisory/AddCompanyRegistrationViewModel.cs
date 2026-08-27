using System;
using System.ComponentModel.DataAnnotations;
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
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    private string _email = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    private string _phoneNo = string.Empty;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _partialAmount;

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
    private string _paymentStatus = "Paid";

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
    [ObservableProperty] private ObservableCollection<CompanyCharacter> _directors = new();
    [ObservableProperty] private ObservableCollection<CompanyCharacter> _shareholders = new();
    [ObservableProperty] private ObservableCollection<CompanyCharacter> _secretaries = new();
    [ObservableProperty] private ObservableCollection<CompanyCharacter> _others = new();

    // Character section computed visibility (reactive to collection changes)
    public bool HasDirectors => Directors.Count > 0;
    public bool HasSecretaries => Secretaries.Count > 0;
    public bool HasShareholders => Shareholders.Count > 0;
    public bool HasOthers => Others.Count > 0;
    public bool NoCharactersAddedYet => !Directors.Any() && !Secretaries.Any() && !Shareholders.Any() && !Others.Any();

    [ObservableProperty] private string _selectedCharacterTab = "Directors";
    [ObservableProperty] private string _selectedAttachmentTab = "Form 01";

    // UI State
    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private bool _isRemoveConfirmVisible = false;
    [ObservableProperty] private string _removeConfirmTitle = string.Empty;
    [ObservableProperty] private string _removeConfirmMessage = string.Empty;
    private string? _pendingFileToRemove;
    private CompanyCharacter? _pendingCharacterNicRemove;
    [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
    [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public Func<System.Threading.Tasks.Task<string?>>? RequestNicPicker { get; set; }
    public Func<System.Threading.Tasks.Task<string[]?>>? RequestMultipleFilePicker { get; set; }

    // Attachments
    public ObservableCollection<SourceDocument> Form01Attachments { get; } = new();
    public ObservableCollection<SourceDocument> BoFormAttachments { get; } = new();
    public ObservableCollection<SourceDocument> Form05Attachments { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedBoResponsiblePerson))]
    private CompanyCharacter? _selectedBoResponsiblePerson;

    public bool HasSelectedBoResponsiblePerson => SelectedBoResponsiblePerson != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBoResponsiblePersonNicFile))]
    private string? _boResponsiblePersonNicFileName;

    public bool HasBoResponsiblePersonNicFile => !string.IsNullOrEmpty(BoResponsiblePersonNicFileName);

    public AddCompanyRegistrationViewModel()
    {
        _isEdit = false;
        Type = CompanyTypes[0];
        _ = LoadClientCodesAsync(() => ClientId);

        
        Directors.CollectionChanged += (s, e) => 
        {
            if (SelectedBoResponsiblePerson != null && !Directors.Contains(SelectedBoResponsiblePerson))
            {
                SelectedBoResponsiblePerson = null;
                BoResponsiblePersonNicFileName = null;
            }
        };
    }

    public AddCompanyRegistrationViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync(() => ClientId);
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
        
        // Load collections (including Phone and Address)
        if (record.Officers != null && record.Officers.Any())
        {
            var directors = record.Officers.Where(o => o.Position == "Director")
                .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
            Directors = new ObservableCollection<CompanyCharacter>(directors);

            var secretaries = record.Officers.Where(o => o.Position == "Secretary")
                .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
            Secretaries = new ObservableCollection<CompanyCharacter>(secretaries);

            var shareholders = record.Officers.Where(o => o.Position == "Shareholder")
                .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
            Shareholders = new ObservableCollection<CompanyCharacter>(shareholders);

            var others = record.Officers.Where(o => o.Position == "Other")
                .Select(o => new CompanyCharacter { Detail = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
            Others = new ObservableCollection<CompanyCharacter>(others);
        }
        else
        {
            if (record.DirectorsList != null)
                foreach (var d in record.DirectorsList)
                    Directors.Add(new CompanyCharacter { Name = d.Name, Phone = d.Phone, Address = d.Address, TIN = d.TIN, Email = d.Email, NicFileName = d.NicFileName, HasNicFile = d.HasNicFile });
            if (record.ShareholdersList != null)
                foreach (var s in record.ShareholdersList)
                    Shareholders.Add(new CompanyCharacter { Name = s.Name, Phone = s.Phone, Address = s.Address, TIN = s.TIN, Email = s.Email, SharePercentage = s.SharePercentage, NicFileName = s.NicFileName, HasNicFile = s.HasNicFile });
            if (record.SecretariesList != null)
                foreach (var sc in record.SecretariesList)
                    Secretaries.Add(new CompanyCharacter { Name = sc.Name, Phone = sc.Phone, Address = sc.Address, TIN = sc.TIN, Email = sc.Email, NicFileName = sc.NicFileName, HasNicFile = sc.HasNicFile });
            if (record.OthersList != null)
                foreach (var o in record.OthersList)
                    Others.Add(new CompanyCharacter { Detail = o.Detail, Role = o.Role, Phone = o.Phone, Address = o.Address, Email = o.Email, NicFileName = o.NicFileName, HasNicFile = o.HasNicFile });
        }

        if (record.SourceDocuments != null && record.SourceDocuments.Count > 0)
        {
            var nicDoc = record.SourceDocuments.FirstOrDefault(d => d.Description == "NIC Document");
            if (nicDoc != null)
            {
                NicFileName = nicDoc.Url ?? nicDoc.FileName;
            }
        }

        // Attachments setup
        BoResponsiblePersonNicFileName = record.BoResponsiblePersonNicFileName;
        
        if (record.DirectorsList != null && !string.IsNullOrEmpty(record.BoResponsiblePersonName))
        {
            var matchingDirector = Directors.FirstOrDefault(d => d.Name == record.BoResponsiblePersonName);
            if (matchingDirector != null)
            {
                SelectedBoResponsiblePerson = matchingDirector;
            }
        }

        if (record.Form01Attachments != null)
        {
            foreach (var doc in record.Form01Attachments) Form01Attachments.Add(doc);
        }
        if (record.BoFormAttachments != null)
        {
            foreach (var doc in record.BoFormAttachments) BoFormAttachments.Add(doc);
        }
        if (record.Form05Attachments != null)
        {
            foreach (var doc in record.Form05Attachments) Form05Attachments.Add(doc);
        }

        Directors.CollectionChanged += (s, e) => 
        {
            if (SelectedBoResponsiblePerson != null && !Directors.Contains(SelectedBoResponsiblePerson))
            {
                SelectedBoResponsiblePerson = null;
                BoResponsiblePersonNicFileName = null;
            }
        };
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand] private void SelectCharacterTab(string tab) => SelectedCharacterTab = tab;
    [RelayCommand] private void SelectAttachmentTab(string tab) => SelectedAttachmentTab = tab;

    private Action? _pendingRemoveAction;

    // Character Management
    [RelayCommand] private void AddDirector() => Directors.Add(new CompanyCharacter { Role = "Director" });
    [RelayCommand] private void RemoveDirector(CompanyCharacter c) 
    {
        RemoveConfirmTitle = "Remove Director?";
        RemoveConfirmMessage = $"Are you sure you want to remove {c.Name ?? "this director"}? This will also remove any attached NIC documents.";
        _pendingRemoveAction = () => Directors.Remove(c);
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand] private void AddSecretary() => Secretaries.Add(new CompanyCharacter { Role = "Secretary" });
    [RelayCommand] private void RemoveSecretary(CompanyCharacter c)
    {
        RemoveConfirmTitle = "Remove Secretary?";
        RemoveConfirmMessage = $"Are you sure you want to remove {c.Name ?? "this secretary"}? This will also remove any attached NIC documents.";
        _pendingRemoveAction = () => Secretaries.Remove(c);
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand] private void AddShareholder() => Shareholders.Add(new CompanyCharacter { Role = "Shareholder" });
    [RelayCommand] private void RemoveShareholder(CompanyCharacter c)
    {
        RemoveConfirmTitle = "Remove Shareholder?";
        RemoveConfirmMessage = $"Are you sure you want to remove {c.Name ?? "this shareholder"}? This will also remove any attached NIC documents.";
        _pendingRemoveAction = () => Shareholders.Remove(c);
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand] private void AddOther() => Others.Add(new CompanyCharacter { Role = "Other" });
    [RelayCommand] private void RemoveOther(CompanyCharacter c)
    {
        RemoveConfirmTitle = "Remove Character?";
        RemoveConfirmMessage = $"Are you sure you want to remove {c.Name ?? "this character"}? This will also remove any attached NIC documents.";
        _pendingRemoveAction = () => Others.Remove(c);
        IsRemoveConfirmVisible = true;
    }

    // NIC Document Uploader for Characters
    [RelayCommand]
    private async System.Threading.Tasks.Task UploadCharacterNic(CompanyCharacter c)
    {
        if (RequestNicPicker != null && c != null)
        {
            var file = await RequestNicPicker();
            if (file != null) c.NicFileName = file;
        }
    }

    [RelayCommand]
    private void PreviewCharacterNic(CompanyCharacter c)
    {
        if (c != null && c.HasNicFile)
        {
            try
            {
                var url = c.NicFileName!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(c.NicFileName).LocalPath : c.NicFileName;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error previewing document: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveCharacterNicConfirm(CompanyCharacter c)
    {
        if (c == null) return;
        RemoveConfirmTitle = "Remove NIC Document?";
        RemoveConfirmMessage = $"Are you sure you want to remove the NIC document for {c.Name ?? "this character"}?";
        _pendingRemoveAction = () => { c.NicFileName = null; };
        IsRemoveConfirmVisible = true;
    }

    // Document Pickers
    [RelayCommand]
    private async System.Threading.Tasks.Task PickForm01Attachment()
    {
        if (RequestMultipleFilePicker != null)
        {
            var files = await RequestMultipleFilePicker();
            if (files != null)
            {
                foreach (var file in files) Form01Attachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveForm01AttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        RemoveConfirmTitle = "Remove Attachment?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        _pendingRemoveAction = () => Form01Attachments.Remove(doc);
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBoFormAttachment()
    {
        if (RequestMultipleFilePicker != null)
        {
            var files = await RequestMultipleFilePicker();
            if (files != null)
            {
                foreach (var file in files) BoFormAttachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveBoFormAttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        RemoveConfirmTitle = "Remove Attachment?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        _pendingRemoveAction = () => BoFormAttachments.Remove(doc);
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickForm05Attachment()
    {
        if (RequestMultipleFilePicker != null)
        {
            var files = await RequestMultipleFilePicker();
            if (files != null)
            {
                foreach (var file in files) Form05Attachments.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file });
            }
        }
    }
    
    [RelayCommand]
    private void ShowRemoveForm05AttachmentConfirm(SourceDocument doc)
    {
        if (doc == null) return;
        RemoveConfirmTitle = "Remove Attachment?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        _pendingRemoveAction = () => Form05Attachments.Remove(doc);
        IsRemoveConfirmVisible = true;
    }

    // BO Person NIC
    [RelayCommand]
    private async System.Threading.Tasks.Task UploadBoPersonNic()
    {
        if (RequestNicPicker != null)
        {
            var file = await RequestNicPicker();
            if (file != null) BoResponsiblePersonNicFileName = file;
        }
    }

    [RelayCommand]
    private void PreviewBoPersonNic()
    {
        if (HasBoResponsiblePersonNicFile)
        {
            try
            {
                var url = BoResponsiblePersonNicFileName!.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(BoResponsiblePersonNicFileName).LocalPath : BoResponsiblePersonNicFileName;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error previewing document: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveBoPersonNicConfirm()
    {
        RemoveConfirmTitle = "Remove NIC Document?";
        RemoveConfirmMessage = "Are you sure you want to remove the BO Responsible Person NIC document?";
        _pendingRemoveAction = () => { BoResponsiblePersonNicFileName = null; };
        IsRemoveConfirmVisible = true;
    }

    // Save and Discard Flow
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
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        try
        {
            var tempId = _originalRecord?.ID ?? Guid.NewGuid().ToString();
            
            var allCharacters = new List<CompanyOfficer>();
            foreach (var d in Directors) allCharacters.Add(new CompanyOfficer { Name = d.Name ?? "", Position = "Director", NicNumber = d.TIN ?? "" });
            foreach (var s in Secretaries) allCharacters.Add(new CompanyOfficer { Name = s.Name ?? "", Position = "Secretary", NicNumber = s.TIN ?? "" });
            foreach (var sh in Shareholders) allCharacters.Add(new CompanyOfficer { Name = sh.Name ?? "", Position = "Shareholder", NicNumber = sh.TIN ?? "" });
            foreach (var o in Others) allCharacters.Add(new CompanyOfficer { Name = o.Name ?? "", Position = "Other", NicNumber = o.TIN ?? "" });

            var uploadedDocs = new List<SourceDocument>();
            if (!string.IsNullOrEmpty(NicFileName))
            {
                uploadedDocs.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(NicFileName), Url = NicFileName, Description = "NIC Document" });
            }

            var uploadedForm01 = new List<SourceDocument>();
            foreach (var doc in Form01Attachments)
            {
                if (!string.IsNullOrEmpty(doc.Url) && System.IO.File.Exists(doc.Url))
                {
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(new List<string> { doc.Url }, "Company Registration", tempId);
                    if (uploaded != null && uploaded.Count > 0) uploadedForm01.Add(uploaded[0]);
                    else uploadedForm01.Add(doc);
                }
                else uploadedForm01.Add(doc);
            }

            var uploadedBoForm = new List<SourceDocument>();
            foreach (var doc in BoFormAttachments)
            {
                if (!string.IsNullOrEmpty(doc.Url) && System.IO.File.Exists(doc.Url))
                {
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(new List<string> { doc.Url }, "Company Registration", tempId);
                    if (uploaded != null && uploaded.Count > 0) uploadedBoForm.Add(uploaded[0]);
                    else uploadedBoForm.Add(doc);
                }
                else uploadedBoForm.Add(doc);
            }

            var uploadedForm05 = new List<SourceDocument>();
            foreach (var doc in Form05Attachments)
            {
                if (!string.IsNullOrEmpty(doc.Url) && System.IO.File.Exists(doc.Url))
                {
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(new List<string> { doc.Url }, "Company Registration", tempId);
                    if (uploaded != null && uploaded.Count > 0) uploadedForm05.Add(uploaded[0]);
                    else uploadedForm05.Add(doc);
                }
                else uploadedForm05.Add(doc);
            }

            var allSourceDocs = uploadedDocs.Concat(uploadedForm01).Concat(uploadedBoForm).Concat(uploadedForm05).ToList();

            if (_isEdit && _originalRecord != null)
            {
                _originalRecord.ClientCode = ClientId;
                _originalRecord.ClientId = _clientGuid;
                _originalRecord.BranchId = _branchGuid;
                _originalRecord.Branch = _branchName;
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
                _originalRecord.PartialAmount = PartialAmount;
                _originalRecord.PaymentOption = PaymentOption;
                _originalRecord.PaymentStatus = PaymentStatus;
                _originalRecord.ChequeBank = ChequeBank;
                _originalRecord.ChequeNumber = ChequeNumber;
                _originalRecord.ChequeDate = ChequeDate;
                _originalRecord.ChequeAmount = ChequeAmount;
                _originalRecord.ChequeStatus = ChequeStatus;

                _originalRecord.Officers = allCharacters;
                _originalRecord.SourceDocuments = allSourceDocs;

                _originalRecord.BoResponsiblePersonName = SelectedBoResponsiblePerson?.Name;
                _originalRecord.BoResponsiblePersonNicFileName = BoResponsiblePersonNicFileName;
                _originalRecord.Form01Attachments = uploadedForm01;
                _originalRecord.BoFormAttachments = uploadedBoForm;
                _originalRecord.Form05Attachments = uploadedForm05;

                await DataService.Instance.UpdateAuditRecordAsync("Company Registration", _originalRecord);
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
                    Type = Type,
                    Address = Address,
                    Email = Email,
                    PhoneNo = PhoneNo,
                    Assignment = Objective,
                    Description = Description,
                    SubTotal = SubTotal,
                    Discount = Discount,
                    PartialAmount = PartialAmount,
                    PaymentOption = PaymentOption,
                    PaymentStatus = PaymentStatus,
                    ChequeBank = ChequeBank,
                    ChequeNumber = ChequeNumber,
                    ChequeDate = ChequeDate,
                    ChequeAmount = ChequeAmount,
                    ChequeStatus = ChequeStatus,
                    Process = "PENDING",
                    CurrentStep = 1,
                    Officers = allCharacters,
                    SourceDocuments = allSourceDocs,
                    BoResponsiblePersonName = SelectedBoResponsiblePerson?.Name,
                    BoResponsiblePersonNicFileName = BoResponsiblePersonNicFileName,
                    Form01Attachments = uploadedForm01,
                    BoFormAttachments = uploadedBoForm,
                    Form05Attachments = uploadedForm05
                };
                
                await DataService.Instance.AddAuditRecordAsync("Company Registration", newRecord);
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
    private void DiscardChanges() => IsDiscardConfirmVisible = true;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        if (GoBack != null) await GoBack();
    }

    [RelayCommand]
    private void CancelDiscard() => IsDiscardConfirmVisible = false;

    // Generic Remove Flow
    [RelayCommand]
    private void ConfirmRemove()
    {
        IsRemoveConfirmVisible = false;
        _pendingRemoveAction?.Invoke();
        _pendingRemoveAction = null;
    }

    [RelayCommand]
    private void CancelRemove()
    {
        IsRemoveConfirmVisible = false;
        _pendingRemoveAction = null;
    }

    partial void OnClientIdChanged(string value)
    {
        FilterClientCodes(value);
    }

    partial void OnChequeBankChanged(string value)
    {
        FilterBanks(value);
    }

    protected override void OnClientSelected(ClientRecord client)
    {
        if (client == null) return;
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        Email = client.Email ?? string.Empty;
        PhoneNo = client.Phone ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;

        // Auto-populate documents uploaded during Client Registration
        if (client.Form01Attachments != null)
        {
            foreach (var doc in client.Form01Attachments)
            {
                if (!Form01Attachments.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                {
                    Form01Attachments.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.Url,
                        Description = "Inherited from Client Registration (" + doc.FileName + ")"
                    });
                }
            }
        }

        if (client.ArticleOfAssociationAttachments != null)
        {
            foreach (var doc in client.ArticleOfAssociationAttachments)
            {
                if (!Form01Attachments.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                {
                    Form01Attachments.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.Url,
                        Description = "Inherited Articles of Association (" + doc.FileName + ")"
                    });
                }
            }
        }

        if (client.BrAttachments != null)
        {
            foreach (var doc in client.BrAttachments)
            {
                if (!BoFormAttachments.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                {
                    BoFormAttachments.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.Url,
                        Description = "Inherited Business Registration (" + doc.FileName + ")"
                    });
                }
            }
        }

        if (client.NicAttachments != null && client.NicAttachments.Count > 0 && string.IsNullOrEmpty(NicFileName))
        {
            var firstNic = client.NicAttachments.FirstOrDefault();
            if (firstNic != null)
            {
                NicFileName = firstNic.Url ?? firstNic.FileName;
            }
        }
    }

    public override void SelectClientCode(ClientRecord client)
    {
        _isSelectingClient = true;
        base.SelectClientCode(client);
        _isSelectingClient = false;
        IsClientCodeDropdownOpen = false;
    }

    public override void SelectBank(string bank)
    {
        ChequeBank = bank;
        IsBankDropdownOpen = false;
    }
}
