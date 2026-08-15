using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory;

public partial class AddBOIViewModel : ViewModelBase
{
    // General Details
    [ObservableProperty] private string _clientId = string.Empty;
    [ObservableProperty] private DateTime? _date = DateTime.Now;
    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _countryAddress = string.Empty;
    [ObservableProperty] private string _investmentValue = string.Empty;
    [ObservableProperty] private string _assignment = string.Empty;

    // Documents
    public ObservableCollection<string> UploadedFiles { get; } = new();
    public ObservableCollection<SourceDocument> ApprovalLetterFiles { get; } = new();
    public ObservableCollection<SourceDocument> PassportFiles { get; } = new();
    public ObservableCollection<SourceDocument> InvestmentFiles { get; } = new();
    public ObservableCollection<SourceDocument> ResidentialVisaFiles { get; } = new();
    public ObservableCollection<SourceDocument> IiaAccountFiles { get; } = new();
    public ObservableCollection<SourceDocument> BankLetterFiles { get; } = new();
    
    [ObservableProperty]
    private bool _isBankLetterPopupVisible;

    [ObservableProperty]
    private string? _selectedCurrency;

    public ObservableCollection<string> Currencies { get; } = new(AATS.Desktop.Data.MockData.Currencies);

    private List<SourceDocument> _bankLetterSnapshot = new();

    [RelayCommand]
    private void OpenBankLetterPopup()
    {
        _bankLetterSnapshot = BankLetterFiles.ToList();
        IsBankLetterPopupVisible = true;
    }

    [RelayCommand]
    private void CancelBankLetterPopup()
    {
        BankLetterFiles.Clear();
        foreach (var file in _bankLetterSnapshot)
        {
            BankLetterFiles.Add(file);
        }
        SelectedCurrency = null;
        IsBankLetterPopupVisible = false;
    }

    [RelayCommand]
    private void SaveBankLetterPopup()
    {
        IsBankLetterPopupVisible = false;
    }
    public ObservableCollection<SourceDocument> CompanyRegistrationFiles { get; } = new();
    public ObservableCollection<SourceDocument> BoiPaymentSlipFiles { get; } = new();
    public ObservableCollection<SourceDocument> VatCertificateFiles { get; } = new();
    public ObservableCollection<SourceDocument> TdlLetterFiles { get; } = new();
    public ObservableCollection<SourceDocument> PlanFiles { get; } = new();
    public ObservableCollection<SourceDocument> BusinessProposalFiles { get; } = new();
    public ObservableCollection<SourceDocument> CoverLetterFiles { get; } = new();

    [ObservableProperty] private string _selectedCard1Tab = "Approval Letter";
    [ObservableProperty] private string _selectedCard2Tab = "BOI Payment Slip";

    [RelayCommand]
    private void SelectCard1Tab(string tabName) => SelectedCard1Tab = tabName;

    [RelayCommand]
    private void SelectCard2Tab(string tabName) => SelectedCard2Tab = tabName;
    
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

    private AuditRecord? _originalRecord;
    private Guid? _clientGuid;
    private Guid? _branchGuid;
    private string? _branchName;

    public AddBOIViewModel()
    {
        _ = LoadClientCodesAsync(() => ClientId);
    }

    public AddBOIViewModel(AuditRecord record)
    {
        _ = LoadClientCodesAsync(() => ClientId);
        _originalRecord = record;
        _clientGuid = record.ClientId;
        _branchGuid = record.BranchId;
        _branchName = record.Branch;
        ClientId = record.ClientCode ?? string.Empty;
        Date = record.Date;
        ClientName = record.ClientName ?? string.Empty;
        CompanyName = record.Company ?? string.Empty;
        Code = record.Code ?? string.Empty;
        Country = record.Country ?? string.Empty;
        CountryAddress = record.CountryAddress ?? string.Empty;
        InvestmentValue = record.InvestmentValue ?? string.Empty;
        Assignment = record.Assignment ?? string.Empty;

        if (record.SourceDocuments != null)
        {
            foreach (var doc in record.SourceDocuments)
            {
                if (doc.Description == "Approval Letter") ApprovalLetterFiles.Add(doc);
                else if (doc.Description == "Passport") PassportFiles.Add(doc);
                else if (doc.Description == "Investment") InvestmentFiles.Add(doc);
                else if (doc.Description == "Residential Visa") ResidentialVisaFiles.Add(doc);
                else if (doc.Description == "IIA Account") IiaAccountFiles.Add(doc);
                else if (doc.Description == "Bank Letter") BankLetterFiles.Add(doc);
                else if (doc.Description == "Company Registration") CompanyRegistrationFiles.Add(doc);
                else if (doc.Description == "BOI Payment Slip") BoiPaymentSlipFiles.Add(doc);
                else if (doc.Description == "VAT Certificate") VatCertificateFiles.Add(doc);
                else if (doc.Description == "TDL Letter") TdlLetterFiles.Add(doc);
                else if (doc.Description == "Plan") PlanFiles.Add(doc);
                else if (doc.Description == "Business Proposal") BusinessProposalFiles.Add(doc);
                else if (doc.Description == "Cover Letter") CoverLetterFiles.Add(doc);
                else 
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
        else if (_pendingSourceDocumentToRemove != null && _pendingCollectionToRemoveFrom != null)
        {

            if (_pendingCollectionToRemoveFrom == "ApprovalLetterFiles") ApprovalLetterFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "PassportFiles") PassportFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "InvestmentFiles") InvestmentFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "ResidentialVisaFiles") ResidentialVisaFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "IiaAccountFiles") IiaAccountFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "BankLetterFiles") BankLetterFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "CompanyRegistrationFiles") CompanyRegistrationFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "BoiPaymentSlipFiles") BoiPaymentSlipFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "VatCertificateFiles") VatCertificateFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "TdlLetterFiles") TdlLetterFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "PlanFiles") PlanFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "BusinessProposalFiles") BusinessProposalFiles.Remove(_pendingSourceDocumentToRemove);
            if (_pendingCollectionToRemoveFrom == "CoverLetterFiles") CoverLetterFiles.Remove(_pendingSourceDocumentToRemove);
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

    
    [RelayCommand]
    private async System.Threading.Tasks.Task PickApprovalLetterFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    ApprovalLetterFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Approval Letter" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveApprovalLetterFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "ApprovalLetterFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickPassportFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    PassportFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Passport" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemovePassportFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "PassportFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickInvestmentFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    InvestmentFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Investment" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveInvestmentFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "InvestmentFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickResidentialVisaFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    ResidentialVisaFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Residential Visa" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveResidentialVisaFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "ResidentialVisaFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickIiaAccountFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    IiaAccountFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "IIA Account" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveIiaAccountFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "IiaAccountFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBankLetterFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    BankLetterFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Bank Letter" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveBankLetterFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "BankLetterFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickCompanyRegistrationFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    CompanyRegistrationFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Company Registration" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveCompanyRegistrationFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "CompanyRegistrationFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBoiPaymentSlipFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    BoiPaymentSlipFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "BOI Payment Slip" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveBoiPaymentSlipFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "BoiPaymentSlipFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickVatCertificateFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    VatCertificateFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "VAT Certificate" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveVatCertificateFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "VatCertificateFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickTdlLetterFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    TdlLetterFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "TDL Letter" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveTdlLetterFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "TdlLetterFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickPlanFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    PlanFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Plan" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemovePlanFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "PlanFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickBusinessProposalFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    BusinessProposalFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Business Proposal" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveBusinessProposalFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "BusinessProposalFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PickCoverLetterFiles()
    {
        if (RequestFilePicker != null)
        {
            var files = await RequestFilePicker();
            if (files != null)
            {
                foreach (var file in files)
                {
                    CoverLetterFiles.Add(new SourceDocument { FileName = System.IO.Path.GetFileName(file), Url = file, Description = "Cover Letter" });
                }
            }
        }
    }

    [RelayCommand]
    private void ShowRemoveCoverLetterFilesConfirm(SourceDocument doc)
    {
        _pendingSourceDocumentToRemove = doc;
        _pendingCollectionToRemoveFrom = "CoverLetterFiles";
        RemoveConfirmTitle = "Remove File?";
        RemoveConfirmMessage = $"Are you sure you want to remove '{doc.FileName}'?";
        IsRemoveConfirmVisible = true;
    }

    private SourceDocument? _pendingSourceDocumentToRemove;
    private string? _pendingCollectionToRemoveFrom;

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
        var clientExists = SharedClientsList.Any(c => c.ClientCode != null && c.ClientCode.Equals(ClientId, System.StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(ClientId) || !clientExists)
        {
            HasFormError = true;
            FormErrorMessage = "Invalid Client ID. Please select an existing client before saving.";
            return;
        }

        ConfirmSaveTitle = "Save Record?";
        ConfirmSaveMessage = "Are you sure you want to create this new record?";
        IsConfirmSaveVisible = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmSave()
    {
        IsConfirmSaveVisible = false;
        
        try
        {
            var tempId = _originalRecord?.ID ?? Guid.NewGuid().ToString();
            
            var localFiles = UploadedFiles.Where(f => !f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            var existingUrls = UploadedFiles.Where(f => f.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            
            var uploadedDocs = new List<SourceDocument>();
            foreach (var url in existingUrls)
            {
                var name = System.IO.Path.GetFileName(new Uri(url).LocalPath);
                uploadedDocs.Add(new SourceDocument { FileName = name, Url = url, Description = "BOI Document" });
            }
            
            if (localFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded)
                {
                    uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "BOI Document" });
                }
            }
            
            // Add from specific collections

            var localApprovalLetterFiles = ApprovalLetterFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingApprovalLetterFiles = ApprovalLetterFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingApprovalLetterFiles);
            if (localApprovalLetterFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localApprovalLetterFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Approval Letter" });
            }

            var localPassportFiles = PassportFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingPassportFiles = PassportFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingPassportFiles);
            if (localPassportFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localPassportFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Passport" });
            }

            var localInvestmentFiles = InvestmentFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingInvestmentFiles = InvestmentFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingInvestmentFiles);
            if (localInvestmentFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localInvestmentFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Investment" });
            }

            var localResidentialVisaFiles = ResidentialVisaFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingResidentialVisaFiles = ResidentialVisaFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingResidentialVisaFiles);
            if (localResidentialVisaFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localResidentialVisaFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Residential Visa" });
            }

            var localIiaAccountFiles = IiaAccountFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingIiaAccountFiles = IiaAccountFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingIiaAccountFiles);
            if (localIiaAccountFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localIiaAccountFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "IIA Account" });
            }

            var localBankLetterFiles = BankLetterFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingBankLetterFiles = BankLetterFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingBankLetterFiles);
            if (localBankLetterFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localBankLetterFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Bank Letter" });
            }

            var localCompanyRegistrationFiles = CompanyRegistrationFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingCompanyRegistrationFiles = CompanyRegistrationFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingCompanyRegistrationFiles);
            if (localCompanyRegistrationFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localCompanyRegistrationFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Company Registration" });
            }

            var localBoiPaymentSlipFiles = BoiPaymentSlipFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingBoiPaymentSlipFiles = BoiPaymentSlipFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingBoiPaymentSlipFiles);
            if (localBoiPaymentSlipFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localBoiPaymentSlipFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "BOI Payment Slip" });
            }

            var localVatCertificateFiles = VatCertificateFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingVatCertificateFiles = VatCertificateFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingVatCertificateFiles);
            if (localVatCertificateFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localVatCertificateFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "VAT Certificate" });
            }

            var localTdlLetterFiles = TdlLetterFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingTdlLetterFiles = TdlLetterFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingTdlLetterFiles);
            if (localTdlLetterFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localTdlLetterFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "TDL Letter" });
            }

            var localPlanFiles = PlanFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingPlanFiles = PlanFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingPlanFiles);
            if (localPlanFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localPlanFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Plan" });
            }

            var localBusinessProposalFiles = BusinessProposalFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingBusinessProposalFiles = BusinessProposalFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingBusinessProposalFiles);
            if (localBusinessProposalFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localBusinessProposalFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Business Proposal" });
            }

            var localCoverLetterFiles = CoverLetterFiles.Where(f => !f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(f => f.Url).ToList();
            var existingCoverLetterFiles = CoverLetterFiles.Where(f => f.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToList();
            uploadedDocs.AddRange(existingCoverLetterFiles);
            if (localCoverLetterFiles.Count > 0)
            {
                var uploaded = await ApiService.Instance.UploadDocumentsAsync(localCoverLetterFiles, "Secretarial & Advisory", tempId);
                foreach (var u in uploaded) uploadedDocs.Add(new SourceDocument { FileName = u.FileName, Url = u.Url, Description = "Cover Letter" });
            }


            if (_originalRecord != null)
            {
                // Update existing
                _originalRecord.ClientCode = ClientId;
                _originalRecord.ClientId = _clientGuid;
                _originalRecord.BranchId = _branchGuid;
                _originalRecord.Date = Date ?? DateTime.Now;
                _originalRecord.ClientName = ClientName;
                _originalRecord.Company = CompanyName;
                _originalRecord.Country = Country;
                _originalRecord.CountryAddress = CountryAddress;
                _originalRecord.Code = Code;
                _originalRecord.InvestmentValue = InvestmentValue;
                _originalRecord.Assignment = Assignment;
                _originalRecord.SourceDocuments = uploadedDocs;

                await DataService.Instance.UpdateAuditRecordAsync("BOI", _originalRecord);
            }
            else
            {
                // Create new
                var newRecord = new AuditRecord
                {
                    ClientCode = ClientId,
                    ClientId = _clientGuid,
                    BranchId = _branchGuid,
                    Branch = _branchName,
                    Date = Date ?? DateTime.Now,
                    ClientName = ClientName,
                    Company = CompanyName,
                    Country = Country,
                    CountryAddress = CountryAddress,
                    Code = Code,
                    InvestmentValue = InvestmentValue,
                    Assignment = Assignment,
                    PaymentStatus = "PENDING",
                    Process = "PENDING",
                    CurrentStep = 1,
                    SourceDocuments = uploadedDocs
                };

                await DataService.Instance.AddAuditRecordAsync("BOI", newRecord);
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

    protected override void OnClientSelected(ClientRecord client)
    {
        if (client == null) return;
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        if (Guid.TryParse(client.Id, out var guid)) _clientGuid = guid;
        _branchGuid = client.BranchId;
        _branchName = client.Branch;

        if (client.BrAttachments != null)
        {
            foreach (var doc in client.BrAttachments)
            {
                if (!ApprovalLetterFiles.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                {
                    ApprovalLetterFiles.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.Url,
                        Description = "Inherited Business Registration (" + doc.FileName + ")"
                    });
                }
            }
        }

        if (client.NicAttachments != null)
        {
            foreach (var doc in client.NicAttachments)
            {
                if (!PassportFiles.Any(d => d.Url == doc.Url || d.FileName == doc.FileName))
                {
                    PassportFiles.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.Url,
                        Description = "Inherited Identification (" + doc.FileName + ")"
                    });
                }
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
}

