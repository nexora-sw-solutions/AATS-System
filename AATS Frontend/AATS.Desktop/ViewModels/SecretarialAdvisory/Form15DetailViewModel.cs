using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class Form15DetailViewModel : DetailViewModelBase
    {
        public override string GuideTitle => "Guide: Form - 15 Details";
        public override string GuideDescription => "Manage Form - 15 registration process and view attached documents.";
        public override string Category => "Form - 15";

        [ObservableProperty] private ObservableCollection<AppDocument> _form15Documents = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _paymentDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _certifiedCopyDocuments = new();

        // Read-only Client Information Properties
        [ObservableProperty] private string _clientId = string.Empty;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _companyName = string.Empty;
        [ObservableProperty] private string _loginId = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _phoneNo = string.Empty;

        // Password Visibility Toggle
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayPassword))]
        [NotifyPropertyChangedFor(nameof(PasswordIcon))]
        private bool _isPasswordVisible = false;

        public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Math.Max(10, Password.Length));
        public string PasswordIcon => IsPasswordVisible ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        // Document Categories for the Attached Documents Card
        [ObservableProperty] private ObservableCollection<AppDocument> _brDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _form01Documents = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _articlesDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _form20Documents = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _auditReportDocuments = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredAttachmentDocuments))]
        private string _selectedAttachmentTab = "BR";

        public ObservableCollection<AppDocument> FilteredAttachmentDocuments => SelectedAttachmentTab switch
        {
            "BR" => BrDocuments,
            "Form 01" => Form01Documents,
            "Articles of Association" => ArticlesDocuments,
            "Form 20" => Form20Documents,
            "Audit Report" => AuditReportDocuments,
            _ => new ObservableCollection<AppDocument>()
        };

        [RelayCommand]
        private void SelectAttachmentTab(string tab) => SelectedAttachmentTab = tab;

        public bool IsForm15UploadVisible => Record?.Process?.Equals("FORM - 15", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsPaymentUploadVisible => Record?.Process?.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsCertifiedCopyUploadVisible => Record?.Process?.Equals("CERTIFIED COPY", StringComparison.OrdinalIgnoreCase) == true;

        public Func<Task<string[]?>>? RequestMultipleFiles { get; set; }

        public Form15DetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            // Pre-fill read-only fields
            ClientId = record.ClientCode ?? string.Empty;
            ClientName = record.ClientName ?? string.Empty;
            CompanyName = record.Company ?? string.Empty;
            LoginId = record.LoginId ?? string.Empty;
            Password = record.Password ?? string.Empty;
            PhoneNo = record.PhoneNo ?? string.Empty;

            if (record.SourceDocuments != null)
            {
                foreach (var doc in record.SourceDocuments)
                {
                    var appDoc = new AppDocument
                    {
                        FileName = doc.FileName,
                        FileSize = "Existing",
                        Category = doc.Description
                    };

                    switch (doc.Description)
                    {
                        case "BR": BrDocuments.Add(appDoc); break;
                        case "Form 01": Form01Documents.Add(appDoc); break;
                        case "Articles of Association": ArticlesDocuments.Add(appDoc); break;
                        case "Form 20": Form20Documents.Add(appDoc); break;
                        case "Audit Report": AuditReportDocuments.Add(appDoc); break;
                    }
                }
            }
            ConfirmDialogMessage = $"Are you sure you want to delete this Form - 15 record? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                await DataService.Instance.DeleteAuditRecordsAsync("Form - 15", new[] { Record! });
                IsConfirmDialogVisible = false;
                DeleteRecordAction?.Invoke();
            };
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Form - 15", "fa-solid fa-file-contract"),
                ("Payment", "fa-solid fa-money-bill"),
                ("Certified Copy", "fa-solid fa-stamp")
            };
            SetupSteps(stepDefs);
        }

        protected override void UpdateStepStates()
        {
            if (Record == null || Steps == null || Steps.Count == 0) return;

            int currentStep = 1;
            if (Record.Process?.Equals("CERTIFIED COPY", StringComparison.OrdinalIgnoreCase) == true) currentStep = 3;
            else if (Record.Process?.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase) == true) currentStep = 2;
            else if (Record.Process?.Equals("FORM - 15", StringComparison.OrdinalIgnoreCase) == true) currentStep = 1;

            Record.CurrentStep = currentStep;

            for (int i = 0; i < Steps.Count; i++)
            {
                int stepNum = i + 1;
                Steps[i].IsActive = stepNum <= currentStep;
                Steps[i].IsClickable = (stepNum == currentStep + 1) || (stepNum < currentStep);
            }

            OnPropertyChanged(nameof(IsForm15UploadVisible));
            OnPropertyChanged(nameof(IsPaymentUploadVisible));
            OnPropertyChanged(nameof(IsCertifiedCopyUploadVisible));
        }

        [RelayCommand]
        public async Task UploadProcessDocumentAsync(string stage)
        {
            if (RequestMultipleFiles == null) return;
            var files = await RequestMultipleFiles();
            if (files == null || files.Length == 0) return;

            foreach (var file in files)
            {
                var doc = new AppDocument
                {
                    FileName = System.IO.Path.GetFileName(file),
                    FileSize = (new System.IO.FileInfo(file).Length / 1024) + " KB",
                    Type = stage
                };

                switch (stage)
                {
                    case "Form - 15": Form15Documents.Add(doc); break;
                    case "Payment": PaymentDocuments.Add(doc); break;
                    case "Certified Copy": CertifiedCopyDocuments.Add(doc); break;
                }
            }
        }

        [RelayCommand]
        private void RemoveProcessDocument(AppDocument doc)
        {
            if (Form15Documents.Contains(doc)) Form15Documents.Remove(doc);
            else if (PaymentDocuments.Contains(doc)) PaymentDocuments.Remove(doc);
            else if (CertifiedCopyDocuments.Contains(doc)) CertifiedCopyDocuments.Remove(doc);
        }

        [RelayCommand]
        private void PreviewProcessDocument(AppDocument doc)
        {
        }
    }
}
