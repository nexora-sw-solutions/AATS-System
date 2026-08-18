using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.TaxFiling
{
    public partial class WHTAddRecordViewModel : ViewModelBase
    {
        private TaxRecord? _originalRecord;
        [ObservableProperty] private bool _isEdit;
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client ID is required")]
        private string _clientId = string.Empty;
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Client name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        private string _clientName = string.Empty;

        partial void OnClientNameChanged(string value)
        {
            FilterClientNames(value);
        }
        [ObservableProperty] private string? _duration = "1";
        [ObservableProperty] private string _durationUnit = "Months";
        [ObservableProperty] private string _WhtNo = string.Empty;
        [ObservableProperty] private string _paymentStatus = "Pending";
        [ObservableProperty] private string _additionalInfo = string.Empty;
        [ObservableProperty] private string _auditorNotes = string.Empty;
        
        public bool IsPaidStatus
        {
            get => PaymentStatus == "Paid";
            set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsPaidStatus)); OnPropertyChanged(nameof(IsPendingStatus)); OnPropertyChanged(nameof(IsIRDPaidStatus)); }
        }

        public bool IsPendingStatus
        {
            get => PaymentStatus == "Pending";
            set { if (value) PaymentStatus = "Pending"; OnPropertyChanged(nameof(IsPaidStatus)); OnPropertyChanged(nameof(IsPendingStatus)); OnPropertyChanged(nameof(IsIRDPaidStatus)); }
        }

        public bool IsIRDPaidStatus
        {
            get => PaymentStatus == "IRD Paid";
            set { if (value) PaymentStatus = "IRD Paid"; OnPropertyChanged(nameof(IsPaidStatus)); OnPropertyChanged(nameof(IsPendingStatus)); OnPropertyChanged(nameof(IsIRDPaidStatus)); }
        }

        public ObservableCollection<string> DurationUnitOptions { get; } = new() { "Months", "Years", "Days" };

        [ObservableProperty] private bool _hasFormError;
        [ObservableProperty] private string _formErrorMessage = string.Empty;

        // Client selection logic
        [ObservableProperty] private bool _isClientCodeDropdownOpen;
        [ObservableProperty] private ObservableCollection<ClientRecord> _clientCodeSuggestions = new();
        [ObservableProperty] private string _selectedClientCategoryColor = "Transparent";
        [ObservableProperty] private bool _hasSelectedClientCategory = false;
        
        private List<ClientRecord> _sharedClientsList = new();
        private Guid? _selectedClientGuid;
        private Guid? _selectedBranchGuid;

        [ObservableProperty] private bool _isGuideVisible = false;
        [ObservableProperty] private bool _isDiscardConfirmVisible = false;

        public Action? GoBack { get; set; }

        // Document Upload Logic
        [ObservableProperty] private ObservableCollection<string> _uploadedFiles = new();
        [ObservableProperty] private bool _isFileRemoveConfirmVisible;
        private string? _fileToRemove;

        [ObservableProperty] private bool _isFileRenameVisible;
        [ObservableProperty] private string _newFileName = string.Empty;
        private string? _fileToRename;

        public Func<Task<string[]?>>? RequestFilePicker { get; set; }

        [RelayCommand]
        private async Task UploadDocument()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (!string.IsNullOrWhiteSpace(file) && !UploadedFiles.Contains(file))
                            UploadedFiles.Add(file);
                    }
                }
            }
        }

        [RelayCommand]
        private void PreviewDocument(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error previewing document: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void ShowRemoveFileConfirm(string fileName)
        {
            _fileToRemove = fileName;
            IsFileRemoveConfirmVisible = true;
        }

        [RelayCommand]
        private void ConfirmRemoveFile()
        {
            if (_fileToRemove != null)
            {
                UploadedFiles.Remove(_fileToRemove);
                _fileToRemove = null;
            }
            IsFileRemoveConfirmVisible = false;
        }

        [RelayCommand]
        private void CancelRemoveFile()
        {
            _fileToRemove = null;
            IsFileRemoveConfirmVisible = false;
        }

        [RelayCommand]
        private void ShowRenameFile(string fileName)
        {
            _fileToRename = fileName;
            NewFileName = fileName;
            IsFileRenameVisible = true;
        }

        [RelayCommand]
        private void ConfirmRenameFile()
        {
            if (_fileToRename != null && !string.IsNullOrWhiteSpace(NewFileName))
            {
                var index = UploadedFiles.IndexOf(_fileToRename);
                if (index >= 0)
                {
                    UploadedFiles[index] = NewFileName;
                }
            }
            _fileToRename = null;
            IsFileRenameVisible = false;
        }

        [RelayCommand]
        private void CancelRenameFile()
        {
            _fileToRename = null;
            IsFileRenameVisible = false;
        }

        public WHTAddRecordViewModel()
        {
            _ = LoadClientCodesAsync();
        }

        public WHTAddRecordViewModel(TaxRecord record)
        {
            _ = LoadClientCodesAsync();
            _originalRecord = record;
            IsEdit = true;
            
            ClientId = record.ClientCode ?? string.Empty;
            ClientName = record.ClientName ?? string.Empty;
            WhtNo = record.TIN ?? string.Empty;
            PaymentStatus = record.Status ?? "Pending";
            AuditorNotes = record.Notes ?? string.Empty;
            AdditionalInfo = "2024/2025"; // Hardcoded Assessment Year in display? Or let's see where it gets it from. Actually, wait. WHTAddRecordView has AdditionalInfo bound to Assessment Year right now.

            // Pre-fill uploaded files
            if (record.SourceDocuments != null)
            {
                foreach (var doc in record.SourceDocuments)
                {
                    if (!string.IsNullOrEmpty(doc.Url)) UploadedFiles.Add(doc.Url);
                    else if (!string.IsNullOrEmpty(doc.FileName)) UploadedFiles.Add(doc.FileName);
                }
            }
            // For now, let's also just populate the dummy payment slip if it doesn't have documents
            if (UploadedFiles.Count == 0 && record.Code != null)
            {
                UploadedFiles.Add($"Payment_Slip_{record.Code}.pdf");
            }
        }

        private async Task LoadClientCodesAsync()
        {
            try
            {
                var clients = await DataService.Instance.GetClientsAsync();
                _sharedClientsList = clients.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to load client codes: {ex.Message}");
            }
        }

        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
        }

        protected override void FilterClientCodes(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ClientCodeSuggestions.Clear();
                IsClientCodeDropdownOpen = false;
                SelectedClientCategoryColor = "Transparent";
                HasSelectedClientCategory = false;
                return;
            }

            var filtered = _sharedClientsList
                .Where(c => (c.ClientCode?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (c.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(5)
                .ToList();

            ClientCodeSuggestions.Clear();
            foreach (var client in filtered)
            {
                ClientCodeSuggestions.Add(client);
            }

            IsClientCodeDropdownOpen = ClientCodeSuggestions.Any();
        }

        public override void SelectClientCode(ClientRecord client)
        {
            ClientId = client.ClientCode ?? string.Empty;
            ClientName = client.Name ?? string.Empty;
            _selectedClientGuid = Guid.TryParse(client.Id, out var guid) ? guid : null;
            _selectedBranchGuid = client.BranchId;
            
            SelectedClientCategoryColor = client.CategoryColor ?? "Transparent";
            HasSelectedClientCategory = true;
            IsClientCodeDropdownOpen = false;
        }

        [RelayCommand]
        public async Task SubmitAsync()
        {
                        ValidateAllProperties();
            if (HasErrors)
            {
                FormErrorMessage = "Please correct the highlighted errors.";
                HasFormError = true;
                return;
            }

            try
            {
                HasFormError = false;
                FormErrorMessage = string.Empty;

                if (!_selectedClientGuid.HasValue && !string.IsNullOrWhiteSpace(ClientId))
                {
                    var matchedClient = _sharedClientsList.FirstOrDefault(c => string.Equals(c.ClientCode, ClientId, StringComparison.OrdinalIgnoreCase));
                    if (matchedClient != null)
                    {
                        _selectedClientGuid = Guid.TryParse(matchedClient.Id, out var guid) ? guid : null;
                        _selectedBranchGuid = matchedClient.BranchId;
                        if (string.IsNullOrWhiteSpace(ClientName))
                        {
                            ClientName = matchedClient.Name;
                        }
                    }
                }

                if (!_selectedClientGuid.HasValue)
                {
                    HasFormError = true;
                    FormErrorMessage = "The entered Client ID is invalid or does not exist.";
                    return;
                }

                if (IsEdit && _originalRecord != null)
                {
                    // Update existing record
                    _originalRecord.ClientCode = ClientId;
                    _originalRecord.ClientId = _selectedClientGuid;
                    _originalRecord.BranchId = _selectedBranchGuid;
                    _originalRecord.ClientName = ClientName;
                    _originalRecord.ClientNameSub = ClientId;
                    _originalRecord.TIN = WhtNo;
                    _originalRecord.TaxPeriod = $"{Duration} {DurationUnit}";
                    _originalRecord.Status = PaymentStatus == "IRD Paid" ? "IRD pending" : PaymentStatus;
                    _originalRecord.PeriodType = DurationUnit.EndsWith("s") ? DurationUnit.Substring(0, DurationUnit.Length - 1) : DurationUnit;
                    _originalRecord.PeriodNumber = Duration ?? string.Empty;
                    _originalRecord.Notes = AuditorNotes;
                    
                    // Update Source Documents
                    var newSourceDocs = new List<SourceDocument>();
                    foreach (var file in UploadedFiles)
                    {
                        newSourceDocs.Add(new SourceDocument { FileName = file, Url = file });
                    }
                    _originalRecord.SourceDocuments = newSourceDocs;

                    await DataService.Instance.UpdateTaxRecordAsync("Withholding Tax (WHT)", _originalRecord);
                }
                else
                {
                    var record = new TaxRecord
                    {
                        ClientCode = ClientId,
                        ClientId = _selectedClientGuid,
                        BranchId = _selectedBranchGuid,
                        ClientName = ClientName,
                        ClientNameSub = ClientId,
                        TIN = WhtNo,
                        TaxPeriod = $"{Duration} {DurationUnit}",
                        Status = PaymentStatus == "IRD Paid" ? "IRD pending" : PaymentStatus,
                        Branch = "South",
                        Date = DateTime.Now,
                        TaxType = "WHT",
                        PeriodType = DurationUnit.EndsWith("s") ? DurationUnit.Substring(0, DurationUnit.Length - 1) : DurationUnit,
                        PeriodNumber = Duration ?? string.Empty,
                        Process = "Pending",
                        Notes = AuditorNotes
                    };
                    
                    var newSourceDocs = new List<SourceDocument>();
                    foreach (var file in UploadedFiles)
                    {
                        newSourceDocs.Add(new SourceDocument { FileName = file, Url = file });
                    }
                    record.SourceDocuments = newSourceDocs;

                    await DataService.Instance.AddTaxRecordAsync("Withholding Tax (WHT)", record);
                }
                GoBack?.Invoke();
            }
            catch (Exception ex)
            {
                HasFormError = true;
                FormErrorMessage = $"Failed to save record: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Clear()
        {
            ClientId = string.Empty;
            ClientName = string.Empty;
            WhtNo = string.Empty;
            Duration = "1";
            DurationUnit = "Months";
            PaymentStatus = "Pending";
            HasFormError = false;
            FormErrorMessage = string.Empty;
            SelectedClientCategoryColor = "Transparent";
            HasSelectedClientCategory = false;
        }

        [RelayCommand]
        private void Cancel()
        {
            IsDiscardConfirmVisible = true;
        }

        [RelayCommand]
        private void ConfirmDiscard()
        {
            IsDiscardConfirmVisible = false;
            GoBack?.Invoke();
        }

        [RelayCommand]
        private void CancelDiscard()
        {
            IsDiscardConfirmVisible = false;
        }

        [RelayCommand]
        private void OpenGuide()
        {
            IsGuideVisible = true;
        }

        [RelayCommand]
        private void CloseGuide()
        {
            IsGuideVisible = false;
        }
    }
}

