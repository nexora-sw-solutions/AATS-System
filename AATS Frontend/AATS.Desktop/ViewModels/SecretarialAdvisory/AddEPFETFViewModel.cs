using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddEPFETFViewModel : ViewModelBase
{
    // Fields matching mockup
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
    [ObservableProperty] private string _noOfStaffsText = string.Empty;

    // UI state
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private bool _isEdit = false;
    [ObservableProperty] private string _pageTitle = "Add Record";
    [ObservableProperty] private string _saveButtonText = "Save Record";


    private readonly AuditRecord? _recordToEdit;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public AddEPFETFViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddEPFETFViewModel(AuditRecord record)
    {
        IsEdit = true;
        PageTitle = "Edit Record";
        SaveButtonText = "Update Record";

        _ = LoadClientCodesAsync(() => ClientId);
        _recordToEdit = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        NoOfStaffsText = record.NoOfStaffs.ToString();
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

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

        int staffCount = 0;
        int.TryParse(NoOfStaffsText, out staffCount);

        if (_recordToEdit != null)
        {
            _recordToEdit.ClientCode = ClientId;
            _recordToEdit.ClientId = _clientGuid;
            _recordToEdit.BranchId = _branchGuid ;
            _recordToEdit.Date = Date ?? DateTime.Now;
            _recordToEdit.ClientName = ClientName;
            _recordToEdit.Company = CompanyName;
            _recordToEdit.NoOfStaffs = staffCount;
            await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", _recordToEdit);
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
                NoOfStaffs = staffCount,
                PaymentStatus = "PENDING",
                Process = "PENDING",
                CurrentStep = 1,
                StaffList = new System.Collections.Generic.List<StaffMember>()
            };
            await DataService.Instance.AddAuditRecordAsync("EPF / ETF", newRecord);
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelSave() => IsConfirmSaveVisible = false;
    [RelayCommand] private void DiscardChanges() => IsDiscardConfirmVisible = true;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        if (GoBack != null) await GoBack();
    }

    [RelayCommand] private void CancelDiscard() => IsDiscardConfirmVisible = false;

    partial void OnClientIdChanged(string value)
    {
        FilterClientCodes(value);
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
}

