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
    [ObservableProperty] private string _id = string.Empty;
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
    public ObservableCollection<CompanyCharacter> Secretaries { get; } = new();
    public ObservableCollection<CompanyCharacter> Shareholders { get; } = new();
    public ObservableCollection<CompanyCharacter> Others { get; } = new();

    // NIC Upload
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNicFile))]
    private string _nicFileName = string.Empty;
    public bool HasNicFile => !string.IsNullOrWhiteSpace(NicFileName);
    public Func<System.Threading.Tasks.Task<string?>>? RequestNicPicker { get; set; }

    // Description
    [ObservableProperty] private string _description = string.Empty;

    // Payment Summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _subTotal = 0.00m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPayment))]
    private decimal _discount = 0.00m;

    [ObservableProperty] private string _paymentOption = "Cash";
    [ObservableProperty] private string _paymentStatus = "Paid";

    public decimal TotalPayment => SubTotal - Discount;

    // Guide
    [ObservableProperty] private bool _isGuideVisible = false;

    public AddCompanyRegistrationViewModel()
    {
        // Add initial empty rows
        AddDirector();
        AddSecretary();
        AddShareholder();
        AddOther();
    }

    public AddCompanyRegistrationViewModel(AuditRecord record)
    {
        _isEdit = true;
        _originalRecord = record;
        LoadRecord(record);
    }

    private void LoadRecord(AuditRecord record)
    {
        Id = record.ID ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Type = record.Assignment ?? string.Empty;
        Address = record.Address ?? string.Empty;
        Email = record.Email ?? string.Empty;
        PhoneNo = record.PhoneNo ?? string.Empty;
        Objective = record.Objective ?? string.Empty;
        Description = record.Description ?? string.Empty;
        PaymentOption = record.PaymentOption ?? "Cash";
        PaymentStatus = record.PaymentStatus ?? "Paid";
        
        // Load collections
        Directors.Clear();
        foreach (var d in record.DirectorsList ?? new()) Directors.Add(new CompanyCharacter { Name = d.Name, Role = d.Role });
        if (Directors.Count == 0) AddDirector();

        Secretaries.Clear();
        foreach (var s in record.SecretariesList ?? new()) Secretaries.Add(new CompanyCharacter { Name = s.Name, Role = s.Role });
        if (Secretaries.Count == 0) AddSecretary();

        Shareholders.Clear();
        foreach (var s in record.ShareholdersList ?? new()) Shareholders.Add(new CompanyCharacter { Name = s.Name, SharePercentage = s.SharePercentage });
        if (Shareholders.Count == 0) AddShareholder();

        Others.Clear();
        foreach (var o in record.OthersList ?? new()) Others.Add(new CompanyCharacter { Name = o.Name, Detail = o.Detail, Role = o.Role });
        if (Others.Count == 0) AddOther();

        // Load NIC if exists
        var nic = record.RegistrationDocuments?.FirstOrDefault(d => d.Category == "NIC");
        if (nic != null)
        {
            NicFileName = nic.FileName ?? string.Empty;
        }
    }

    [RelayCommand] private void AddDirector() => Directors.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveDirector(CompanyCharacter item) { if (Directors.Count > 1) Directors.Remove(item); }

    [RelayCommand] private void AddSecretary() => Secretaries.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveSecretary(CompanyCharacter item) { if (Secretaries.Count > 1) Secretaries.Remove(item); }

    [RelayCommand] private void AddShareholder() => Shareholders.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveShareholder(CompanyCharacter item) { if (Shareholders.Count > 1) Shareholders.Remove(item); }

    [RelayCommand] private void AddOther() => Others.Add(new CompanyCharacter());
    [RelayCommand] private void RemoveOther(CompanyCharacter item) { if (Others.Count > 1) Others.Remove(item); }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task UploadNic()
    {
        if (RequestNicPicker != null)
        {
            var file = await RequestNicPicker();
            if (file != null)
                NicFileName = file;
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
        HasFormError = false;

        if (!ValidationHelper.IsValidName(ClientName) || !ValidationHelper.IsValidName(CompanyName))
        {
            FormErrorMessage = "Please enter valid client and company names.";
            HasFormError = true;
            return;
        }

        if (!string.IsNullOrWhiteSpace(Email) && !ValidationHelper.IsValidEmail(Email))
        {
            FormErrorMessage = "Please enter a valid email address.";
            HasFormError = true;
            return;
        }

        if (!string.IsNullOrWhiteSpace(PhoneNo) && !ValidationHelper.IsValidPhone(PhoneNo))
        {
            FormErrorMessage = "Please enter a valid phone number.";
            HasFormError = true;
            return;
        }

        ConfirmSaveTitle = _isEdit ? "Update Record?" : "Save Record?";
        ConfirmSaveMessage = _isEdit ? "Are you sure you want to update this company registration record?" : "Are you sure you want to create this new company registration record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        var recordToSave = _isEdit && _originalRecord != null ? _originalRecord : new AuditRecord();
        
        recordToSave.ID = Id;
        recordToSave.Date = Date ?? DateTime.Now;
        recordToSave.ClientName = ClientName;
        recordToSave.Company = CompanyName;
        recordToSave.Assignment = Type; 
        recordToSave.PaymentOption = PaymentOption;
        recordToSave.PaymentStatus = PaymentStatus;
        
        if (!_isEdit)
        {
            recordToSave.Process = "PENDING";
            recordToSave.CurrentStep = 1;
        }
        
        // Map specialized fields
        recordToSave.Address = Address;
        recordToSave.Email = Email;
        recordToSave.PhoneNo = PhoneNo;
        recordToSave.Objective = Objective;
        recordToSave.Description = Description;
        
        recordToSave.DirectorsList = new List<CompanyCharacter>(Directors.Where(d => !string.IsNullOrEmpty(d.Name)));
        recordToSave.SecretariesList = new List<CompanyCharacter>(Secretaries.Where(s => !string.IsNullOrEmpty(s.Name)));
        recordToSave.ShareholdersList = new List<CompanyCharacter>(Shareholders.Where(s => !string.IsNullOrEmpty(s.Name)));
        recordToSave.OthersList = new List<CompanyCharacter>(Others.Where(o => !string.IsNullOrEmpty(o.Name)));
        
        // Handle NIC separately if needed or rebuild docs list
        recordToSave.RegistrationDocuments = new List<AppDocument>();
        if (!string.IsNullOrEmpty(NicFileName))
        {
            recordToSave.RegistrationDocuments.Add(new AppDocument 
            { 
                FileName = NicFileName, 
                Category = "NIC", 
                Type = "National identity card",
                IsExisting = true 
            });
        }
        
        if (_isEdit)
        {
            await DataService.Instance.UpdateAuditRecordAsync("Company Registration", recordToSave);
        }
        else
        {
            await DataService.Instance.AddAuditRecordAsync("Company Registration", recordToSave);
        }

        if (GoBack != null) await GoBack();
    }

    [RelayCommand]
    private void CancelSave() => IsConfirmSaveVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task DiscardChanges()
        {
        IsConfirmSaveVisible = false;
        if (GoBack != null) await GoBack();
    }
}


