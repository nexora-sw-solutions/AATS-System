using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.ViewModels.Shared;
using System.Threading.Tasks;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class AuditAssuranceAddRecordViewModel : ViewModelBase, IDraftFormViewModel
    {
        public bool HasUnsavedChanges => (!string.IsNullOrWhiteSpace(ClientId) || !string.IsNullOrWhiteSpace(ClientName) || SubTotal > 0);

        public async Task SaveAsDraftAsync()
        {
            try
            {
                var draftRecord = new AuditRecord
                {
                    ClientCode = string.IsNullOrWhiteSpace(ClientId) ? "DRAFT" : ClientId,
                    ClientId = _clientGuid,
                    BranchId = _branchGuid,
                    Date = Date ?? DateTime.UtcNow,
                    ClientName = string.IsNullOrWhiteSpace(ClientName) ? "Draft Record" : ClientName,
                    Assignment = Assignment,
                    PaymentOption = PaymentOption,
                    PaymentStatus = PaymentStatus,
                    Notes = Notes,
                    SubTotal = SubTotal,
                    Discount = Discount,
                    TotalPayment = TotalPayment,
                    Process = "DRAFT",
                    CurrentStep = 1
                };
                await DataService.Instance.AddAuditRecordAsync("Assurance", draftRecord);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving draft: {ex.Message}");
            }
        }

        public async Task DiscardChangesAsync()
        {
            await Task.CompletedTask;
        }

        private readonly AuditRecord? _originalRecord;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client ID is required")]
        private string _clientId = string.Empty;
        private Guid? _clientGuid;
        private Guid? _branchGuid;

        [ObservableProperty]
        private DateTime? _date = DateTime.UtcNow;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        private string _clientName = string.Empty;

        partial void OnClientNameChanged(string value)
        {
            FilterClientNames(value);
        }

        [ObservableProperty]
        private string _assignment = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

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
        private string _paymentStatus = "Paid";
        [ObservableProperty] private string _notes = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasClientLogo))]
        private string _clientLogo = string.Empty;

        public bool HasClientLogo => !string.IsNullOrWhiteSpace(ClientLogo);

        // UI State
        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isConfirmSaveVisible = false;
        [ObservableProperty] private bool _isDiscardConfirmVisible = false;
        [ObservableProperty] private bool _isRemoveConfirmVisible = false;
        [ObservableProperty] private string _removeConfirmTitle = string.Empty;
        [ObservableProperty] private string _removeConfirmMessage = string.Empty;
        private string? _pendingFileToRemove;
        private bool _isRemovingLogo;
        [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
        [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

        public System.Collections.ObjectModel.ObservableCollection<string> UploadedFiles { get; } = new();

        // Part 1 Tabs UI State
        [ObservableProperty] private string _selectedPart1Tab = "BR";
        [RelayCommand] private void SelectPart1Tab(string tabName) => SelectedPart1Tab = tabName;

        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> BRFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> Form01Files { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> ArticleofAssociationFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> PreviousAccountsFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> BankConfirmationFiles { get; } = new();

        // Part 2 Tabs UI State
        [ObservableProperty] private string _selectedPart2Tab = "Bank Statement";
        [RelayCommand] private void SelectPart2Tab(string tabName) => SelectedPart2Tab = tabName;

        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> BankStatementFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> AssetDetailsFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> LiabilityDetailsFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> ExpenseDetailsFiles { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<SourceDocument> LogoFiles { get; } = new();

        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }
        public Func<System.Threading.Tasks.Task<string?>>? RequestLogoPicker { get; set; }
        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickBRFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        BRFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "BR" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickForm01Files()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        Form01Files.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Form 01" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickArticleofAssociationFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        ArticleofAssociationFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Article of Association" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickPreviousAccountsFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        PreviousAccountsFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Previous Accounts" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickBankConfirmationFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        BankConfirmationFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Bank Confirmation" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickBankStatementFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        BankStatementFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Bank Statement" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickAssetDetailsFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        AssetDetailsFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Asset Details" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickLiabilityDetailsFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        LiabilityDetailsFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Liability Details" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickExpenseDetailsFiles()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        ExpenseDetailsFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Expense Details" });
                    }
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickLogoFiles()
        {
            if (RequestLogoPicker != null)
            {
                var file = await RequestLogoPicker();
                if (file != null)
                {
                    LogoFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Logo" });
                }
            }
            else if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        LogoFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Logo" });
                    }
                }
            }
        }

        // Default constructor for new records
        public AuditAssuranceAddRecordViewModel() 
        {
            _ = LoadClientCodesAsync(() => ClientId);
        }

        // Constructor for editing existing records (pre-fill fields)
        public AuditAssuranceAddRecordViewModel(AuditRecord record)
        {
            _ = LoadClientCodesAsync(() => ClientId);
            _originalRecord = record;
            ClientId = record.ClientCode ?? string.Empty;
            _clientGuid = record.ClientId;
            Date = record.Date;
            ClientName = record.ClientName ?? string.Empty;
            Assignment = record.Assignment ?? string.Empty;
            PaymentOption = record.PaymentOption ?? "Cash";
            PaymentStatus = record.PaymentStatus ?? "Paid";
            Notes = record.Notes ?? string.Empty;
            SubTotal = record.SubTotal;
            Discount = record.Discount;
            PartialAmount = record.PartialAmount;
            
            // Pre-fill Cheque Details
            ChequeBank = record.ChequeBank ?? string.Empty;
            ChequeNumber = record.ChequeNumber ?? string.Empty;
            ChequeDate = record.ChequeDate ?? DateTime.Now;
            ChequeAmount = record.ChequeAmount ?? 0.00m;
            ChequeStatus = record.ChequeStatus ?? "Pending";

            // Pre-fill UploadedFiles
            if (record.SourceDocuments != null)
            {
                foreach (var doc in record.SourceDocuments)
                {
                    if (!string.IsNullOrEmpty(doc.FileName))
                    {
                        UploadedFiles.Add(doc.FileName);
                    }
                }
            }
        }

        // Helper properties for RadioButtons
        public bool IsOptionCash
        {
            get => PaymentOption == "Cash";
            set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        public bool IsOptionOnline
        {
            get => PaymentOption == "Online";
            set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        public bool IsOptionCheque
        {
            get => PaymentOption == "Cheque";
            set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); }
        }

        public bool IsStatusPaid
        {
            get => PaymentStatus == "Paid";
            set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        public bool IsStatusUnpaid
        {
            get => PaymentStatus == "Unpaid";
            set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        public bool IsStatusPartial
        {
            get => PaymentStatus == "Partial";
            set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); }
        }

        // Cheque Details
        [ObservableProperty] private string _chequeBank = string.Empty;
        [ObservableProperty] private string _chequeNumber = string.Empty;
        [ObservableProperty] private DateTime? _chequeDate = DateTime.Now;
        [ObservableProperty] private decimal? _chequeAmount = 0.00m;
        [ObservableProperty] private string _chequeStatus = "Pending";

        public System.Collections.ObjectModel.ObservableCollection<string> ChequeStatusOptions { get; } = new()
        {
            "Pending", "Cleared", "Bounced"
        };

        public new System.Collections.Generic.List<string> Banks => AATS.Desktop.Helpers.BankHelper.GetBanks();

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

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

            ConfirmSaveTitle = _originalRecord != null ? "Save Changes?" : "Save Record?";
            ConfirmSaveMessage = _originalRecord != null 
                ? "Are you sure you want to save the modifications to this record?" 
                : "Are you sure you want to create this new audit record?";
            IsConfirmSaveVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmSave()
        {
            IsConfirmSaveVisible = false;
            try 
            {
                HasFormError = false;

                // Upload Client Logo if it's a new local path
                if (_clientGuid.HasValue)
                {
                    var client = await DataService.Instance.GetClientByIdAsync(_clientGuid.Value.ToString());
                    if (client != null)
                    {
                        if (!string.IsNullOrEmpty(ClientLogo) && !ClientLogo.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            var logoUrl = await ApiService.Instance.UploadLogoAsync(ClientLogo, client.Id!);
                            ClientLogo = logoUrl;
                            client.LogoStorageKey = logoUrl;
                            await DataService.Instance.UpdateClientAsync(client);
                        }
                        else if (string.IsNullOrEmpty(ClientLogo) && !string.IsNullOrEmpty(client.LogoStorageKey))
                        {
                            client.LogoStorageKey = null;
                            await DataService.Instance.UpdateClientAsync(client);
                        }
                    }
                }

                // Upload new local files to R2; keep existing R2 URLs as-is
                var localFiles = UploadedFiles.Where(f => !f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
                var existingUrls = UploadedFiles.Where(f => f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();

                var uploadedDocs = new System.Collections.Generic.List<SourceDocument>();

                // Keep existing URL-based docs
                foreach (var url in existingUrls)
                {
                    uploadedDocs.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(new Uri(url).LocalPath), Url = url, Description = "Uploaded document" });
                }

                // Upload new local files
                if (localFiles.Count > 0)
                {
                    var tempId = _originalRecord?.ID ?? Guid.NewGuid().ToString();
                    var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Assurance", tempId);
                    foreach (var u in uploaded)
                    {
                        uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Uploaded document" });
                    }
                }
                
                if (_originalRecord != null)
                {
                    _originalRecord.ClientCode = ClientId;
                    _originalRecord.ClientId = _clientGuid;
                    _originalRecord.BranchId = _branchGuid;
                    _originalRecord.Date = Date ?? DateTime.UtcNow;
                    _originalRecord.ClientName = ClientName;
                    _originalRecord.Assignment = Assignment;
                    _originalRecord.PaymentOption = PaymentOption;
                    _originalRecord.PaymentStatus = PaymentStatus;
                    _originalRecord.Notes = Notes;
                    _originalRecord.SubTotal = SubTotal;
                    _originalRecord.Discount = Discount;
                    _originalRecord.TotalPayment = TotalPayment;
                    _originalRecord.PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0);

                    // Map Cheque Details
                    _originalRecord.ChequeBank = ChequeBank;
                    _originalRecord.ChequeNumber = ChequeNumber;
                    _originalRecord.ChequeDate = ChequeDate;
                    _originalRecord.ChequeAmount = ChequeAmount;
                    _originalRecord.ChequeStatus = ChequeStatus;

                    _originalRecord.SourceDocuments = uploadedDocs;

                    await DataService.Instance.UpdateAuditRecordAsync("Assurance", _originalRecord);
                }
                else
                {
                    var newRecord = new AuditRecord
                    {
                        ClientCode = ClientId,
                        ClientId = _clientGuid,
                        BranchId = _branchGuid,
                        Date = Date ?? DateTime.UtcNow,
                        ClientName = ClientName,
                        Assignment = Assignment,
                        PaymentOption = PaymentOption,
                        PaymentStatus = PaymentStatus,
                        Notes = Notes,
                        SubTotal = SubTotal,
                        Discount = Discount,
                        TotalPayment = TotalPayment,
                        PartialAmount = (PaymentStatus == "Paid") ? (SubTotal - Discount) : (PaymentStatus == "Partial" ? PartialAmount : 0),
                        Process = "BOOKKEEP",
                        CurrentStep = 1,
                        
                        // Map Cheque Details
                        ChequeBank = ChequeBank,
                        ChequeNumber = ChequeNumber,
                        ChequeDate = ChequeDate,
                        ChequeAmount = ChequeAmount,
                        ChequeStatus = ChequeStatus,
                        SourceDocuments = uploadedDocs
                    };

                    await DataService.Instance.AddAuditRecordAsync("Assurance", newRecord);
                }

                if (GoBack != null) await GoBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving record: {ex.Message}");
                HasFormError = true;
                FormErrorMessage = "Error saving record: " + ex.Message;
            }
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

        [RelayCommand]
        private void ShowRemoveLogoConfirm()
        {
            _isRemovingLogo = true;
            _pendingFileToRemove = null;
            RemoveConfirmTitle = "Remove Logo?";
            RemoveConfirmMessage = "Are you sure you want to remove the uploaded client logo?";
            IsRemoveConfirmVisible = true;
        }

        [RelayCommand]
        private void ShowRemoveFileConfirm(string fileName)
        {
            _isRemovingLogo = false;
            _pendingFileToRemove = fileName;
            RemoveConfirmTitle = "Remove File?";
            RemoveConfirmMessage = $"Are you sure you want to remove '{System.IO.Path.GetFileName(fileName)}'?";
            IsRemoveConfirmVisible = true;
        }

        [RelayCommand]
        private void ConfirmRemove()
        {
            if (_isRemovingLogo)
            {
                ClientLogo = string.Empty;
            }
            else if (!string.IsNullOrEmpty(_pendingFileToRemove))
            {
                if (UploadedFiles.Contains(_pendingFileToRemove))
                    UploadedFiles.Remove(_pendingFileToRemove);
            }
            
            CancelRemove();
        }

        [RelayCommand]
        private void CancelRemove()
        {
            IsRemoveConfirmVisible = false;
            _pendingFileToRemove = null;
            _isRemovingLogo = false;
        }

        [RelayCommand]
        private void RemoveLogo()
        {
            ClientLogo = string.Empty;
        }

        [RelayCommand]
        private void PreviewLogo()
        {
            if (HasClientLogo)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(ClientLogo) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error previewing logo: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadLogo()
        {
            if (RequestLogoPicker != null)
            {
                var logo = await RequestLogoPicker();
                if (logo != null)
                {
                    ClientLogo = logo;
                }
            }
        }

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
                            UploadedFiles.Add(file);
                    }
                }
            }
        }

        [RelayCommand]
        private void PreviewDocument(object? parameter)
        {
            string? target = null;
            if (parameter is string s) target = s;
            else if (parameter is SourceDocument doc) target = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.FileName;

            if (!string.IsNullOrWhiteSpace(target))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error previewing document: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void RemoveDocument(object? parameter)
        {
            if (parameter is SourceDocument doc)
            {
                BRFiles.Remove(doc);
                Form01Files.Remove(doc);
                ArticleofAssociationFiles.Remove(doc);
                PreviousAccountsFiles.Remove(doc);
                BankConfirmationFiles.Remove(doc);
                BankStatementFiles.Remove(doc);
                AssetDetailsFiles.Remove(doc);
                LiabilityDetailsFiles.Remove(doc);
                ExpenseDetailsFiles.Remove(doc);
                LogoFiles.Remove(doc);
            }
            else if (parameter is string fileName)
            {
                RemoveFile(fileName);
            }
        }

        [RelayCommand]
        private void RemoveFile(string fileName)
        {
            if (UploadedFiles.Contains(fileName))
                UploadedFiles.Remove(fileName);
        }

        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
            var matched = SharedClientsList?.FirstOrDefault(c => string.Equals(c.ClientCode, value, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                SelectedClient = matched;
            }
        }

        public override void SelectClientCode(ClientRecord client)
        {
            _isSelectingClient = true;
            ClientId = client.ClientCode ?? string.Empty;
            SelectedClient = client;
            _isSelectingClient = false;
            IsClientCodeDropdownOpen = false;
        }

        protected override void OnClientSelected(ClientRecord client)
        {
            if (client == null) return;

            ClientName = client.Name ?? string.Empty;
            ClientLogo = client.LogoStorageKey ?? string.Empty;
            _clientGuid = Guid.TryParse(client.Id, out var guid) ? guid : null;
            _branchGuid = client.BranchId;

            if (client.BrAttachments != null && client.BrAttachments.Count > 0)
            {
                BRFiles.Clear();
                foreach (var doc in client.BrAttachments)
                {
                    BRFiles.Add(new SourceDocument 
                    { 
                        Id = doc.Id, 
                        FileName = doc.FileName, 
                        Url = doc.Url, 
                        Description = doc.Description ?? "BR Document" 
                    });
                }
            }

            if (client.Form01Attachments != null && client.Form01Attachments.Count > 0)
            {
                Form01Files.Clear();
                foreach (var doc in client.Form01Attachments)
                {
                    Form01Files.Add(new SourceDocument 
                    { 
                        Id = doc.Id, 
                        FileName = doc.FileName, 
                        Url = doc.Url, 
                        Description = doc.Description ?? "Form 01 Document" 
                    });
                }
            }

            if (client.ArticleOfAssociationAttachments != null && client.ArticleOfAssociationAttachments.Count > 0)
            {
                ArticleofAssociationFiles.Clear();
                foreach (var doc in client.ArticleOfAssociationAttachments)
                {
                    ArticleofAssociationFiles.Add(new SourceDocument 
                    { 
                        Id = doc.Id, 
                        FileName = doc.FileName, 
                        Url = doc.Url, 
                        Description = doc.Description ?? "Articles of Association" 
                    });
                }
            }
        }
    }
}

