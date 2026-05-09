using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class InternalControlAddRecordViewModel : ViewModelBase
    {
        private readonly AuditRecord? _originalRecord;

        [ObservableProperty] private string _clientId = string.Empty;
        private Guid? _clientGuid;
        private Guid? _branchGuid;
        [ObservableProperty] private DateTime? _date = DateTime.UtcNow;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _assignment = string.Empty;
        [ObservableProperty] private string _period = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasClientLogo))]
        private string _clientLogo = string.Empty;

        public bool HasClientLogo => !string.IsNullOrWhiteSpace(ClientLogo);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _subTotal = 0.00m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPayment))]
        private decimal _discount = 0.00m;

        public decimal TotalPayment => SubTotal - Discount;

        [ObservableProperty] private string _paymentOption = "Cash";
        [ObservableProperty] private string _paymentStatus = "Paid";

        public bool IsOptionCash { get => PaymentOption == "Cash"; set { if (value) PaymentOption = "Cash"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
        public bool IsOptionOnline { get => PaymentOption == "Online"; set { if (value) PaymentOption = "Online"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }
        public bool IsOptionCheque { get => PaymentOption == "Cheque"; set { if (value) PaymentOption = "Cheque"; OnPropertyChanged(nameof(IsOptionCash)); OnPropertyChanged(nameof(IsOptionOnline)); OnPropertyChanged(nameof(IsOptionCheque)); } }

        public bool IsStatusPaid { get => PaymentStatus == "Paid"; set { if (value) PaymentStatus = "Paid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
        public bool IsStatusUnpaid { get => PaymentStatus == "Unpaid"; set { if (value) PaymentStatus = "Unpaid"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }
        public bool IsStatusPartial { get => PaymentStatus == "Partial"; set { if (value) PaymentStatus = "Partial"; OnPropertyChanged(nameof(IsStatusPaid)); OnPropertyChanged(nameof(IsStatusUnpaid)); OnPropertyChanged(nameof(IsStatusPartial)); } }

        [ObservableProperty] private string _notes = string.Empty;

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

        public ObservableCollection<string> UploadedFiles { get; } = new();
        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }
        public Func<System.Threading.Tasks.Task<string?>>? RequestLogoPicker { get; set; }
        public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

        public InternalControlAddRecordViewModel() 
        {
            _ = LoadClientCodesAsync(); 
        }

        public InternalControlAddRecordViewModel(AuditRecord record)
        {
            _ = LoadClientCodesAsync();
            _originalRecord = record;
            ClientId = record.ClientCode ?? string.Empty;
            _clientGuid = record.ClientId;
            Date = record.Date;
            ClientName = record.ClientName ?? string.Empty;
            Assignment = record.Assignment ?? string.Empty;
            Period = record.Period ?? string.Empty;
            PaymentOption = record.PaymentOption ?? "Cash";
            PaymentStatus = record.PaymentStatus ?? "Paid";
            Notes = record.Notes ?? string.Empty;
        }

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

        [RelayCommand]
        private void SaveRecord()
        {
            ConfirmSaveTitle = _originalRecord != null ? "Save Changes?" : "Save Record?";
            ConfirmSaveMessage = _originalRecord != null 
                ? "Are you sure you want to save the modifications to this record?" 
                : "Are you sure you want to create this new internal control record?";
            IsConfirmSaveVisible = true;
        }

        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
        }

        public override void SelectClientCode(ClientRecord client)
        {
            ClientId = client.ClientCode ?? string.Empty;
            _clientGuid = Guid.TryParse(client.Id, out var guid) ? guid : null;
            _branchGuid = client.BranchId;
            ClientName = client.Name ?? string.Empty;
            IsClientCodeDropdownOpen = false;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ConfirmSave()
        {
            IsConfirmSaveVisible = false;
            try 
            {
                HasFormError = false;
                
                if (_originalRecord != null)
                {
                    _originalRecord.ClientCode = ClientId;
                    _originalRecord.ClientId = _clientGuid;
                    _originalRecord.BranchId = _branchGuid ?? Guid.Empty;
                    _originalRecord.Date = Date ?? DateTime.UtcNow;
                    _originalRecord.ClientName = ClientName;
                    _originalRecord.Assignment = Assignment;
                    _originalRecord.Period = Period;
                    _originalRecord.PaymentOption = PaymentOption;
                    _originalRecord.PaymentStatus = PaymentStatus;
                    _originalRecord.Notes = Notes;
                    _originalRecord.SubTotal = SubTotal;
                    _originalRecord.Discount = Discount;
                    _originalRecord.TotalPayment = TotalPayment;
                    _originalRecord.PartialAmount = (PaymentStatus == "Paid") ? TotalPayment : (PaymentStatus == "Partial" ? TotalPayment / 2 : 0);
                    
                    await DataService.Instance.UpdateAuditRecordAsync("Internal Control Systems & Outsourcing", _originalRecord);
                }
                else
                {
                    var newRecord = new AuditRecord
                    {
                        ClientCode = ClientId,
                        ClientId = _clientGuid,
                        BranchId = _branchGuid ?? Guid.Empty,
                        Date = Date ?? DateTime.UtcNow,
                        ClientName = ClientName,
                        Assignment = Assignment,
                        Period = Period,
                        PaymentOption = PaymentOption,
                        PaymentStatus = PaymentStatus,
                        Notes = Notes,
                        SubTotal = SubTotal,
                        Discount = Discount,
                        TotalPayment = TotalPayment,
                        PartialAmount = (PaymentStatus == "Paid") ? TotalPayment : (PaymentStatus == "Partial" ? TotalPayment / 2 : 0),
                        Process = "REPORTING", // Default for internal control
                        CurrentStep = 1
                    };
                    
                    await DataService.Instance.AddAuditRecordAsync("Internal Control Systems & Outsourcing", newRecord);
                }
            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"Error saving record: {ex.Message}");
                HasFormError = true;
                FormErrorMessage = "Error saving record: " + ex.Message;
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
                if (logo != null) ClientLogo = logo;
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
                UploadedFiles.Remove(fileName);
        }
    }
}
