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

    // Dynamic Collections
    public ObservableCollection<CompanyCharacter> Directors { get; } = new();
    public ObservableCollection<CompanyCharacter> Shareholders { get; } = new();
    public ObservableCollection<CompanyCharacter> Secretaries { get; } = new();

    // UI State
    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private bool _isConfirmSaveVisible = false;
    [ObservableProperty] private bool _isDiscardConfirmVisible = false;
    [ObservableProperty] private string _confirmSaveTitle = "Save Record?";
    [ObservableProperty] private string _confirmSaveMessage = "Are you sure you want to save these changes?";

    public Func<System.Threading.Tasks.Task>? GoBack { get; set; }

    public Func<System.Threading.Tasks.Task<string?>>? RequestNicPicker { get; set; }

    public AddCompanyRegistrationViewModel()
    {
        _isEdit = false;
        _ = LoadClientCodesAsync();
    }

    public AddCompanyRegistrationViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync();
        _isEdit = true;
        _originalRecord = record;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Type = record.Type ?? string.Empty;
        Address = record.Address ?? string.Empty;
        Email = record.Email ?? string.Empty;
        PhoneNo = record.PhoneNo ?? string.Empty;
        Objective = record.Assignment ?? string.Empty;
        
        // Load collections if they exist
        if (record.DirectorsList != null) foreach (var d in record.DirectorsList) Directors.Add(new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email });
        if (record.ShareholdersList != null) foreach (var s in record.ShareholdersList) Shareholders.Add(new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email });
        if (record.SecretariesList != null) foreach (var sc in record.SecretariesList) Secretaries.Add(new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email });
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand] private void AddDirector() => Directors.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveDirector(CompanyCharacter c) => Directors.Remove(c);
    [RelayCommand] private void AddShareholder() => Shareholders.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveShareholder(CompanyCharacter c) => Shareholders.Remove(c);
    [RelayCommand] private void AddSecretary() => Secretaries.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveSecretary(CompanyCharacter c) => Secretaries.Remove(c);

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
            _originalRecord.Date = Date ?? DateTime.Now;
            _originalRecord.ClientName = ClientName;
            _originalRecord.Company = CompanyName;
            _originalRecord.Type = Type;
            _originalRecord.Address = Address;
            _originalRecord.Email = Email;
            _originalRecord.PhoneNo = PhoneNo;
            _originalRecord.Assignment = Objective;
            
            _originalRecord.DirectorsList = Directors.Select(d => new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email }).ToList();
            _originalRecord.ShareholdersList = Shareholders.Select(s => new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email }).ToList();
            _originalRecord.SecretariesList = Secretaries.Select(sc => new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email }).ToList();

            await DataService.Instance.UpdateAuditRecordAsync("Company Registration", _originalRecord);
        }
        else
        {
            var newRecord = new AuditRecord
            {
                ClientCode = ClientId,
                Date = Date ?? DateTime.Now,
                ClientName = ClientName,
                Company = CompanyName,
                Type = Type,
                Address = Address,
                Email = Email,
                PhoneNo = PhoneNo,
                Assignment = Objective,
                Process = "NAME APPROVAL",
                CurrentStep = 1,
                DirectorsList = Directors.Select(d => new CompanyCharacter { Name = d.Name, TIN = d.TIN, Email = d.Email }).ToList(),
                ShareholdersList = Shareholders.Select(s => new CompanyCharacter { Name = s.Name, TIN = s.TIN, Email = s.Email }).ToList(),
                SecretariesList = Secretaries.Select(sc => new CompanyCharacter { Name = sc.Name, TIN = sc.TIN, Email = sc.Email }).ToList()
            };
            
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

    public override void SelectClientCode(ClientRecord client)
    {
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        IsClientCodeDropdownOpen = false;
    }
}