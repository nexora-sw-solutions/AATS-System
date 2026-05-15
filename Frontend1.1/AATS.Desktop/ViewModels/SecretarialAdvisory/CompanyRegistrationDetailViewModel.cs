using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Services;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    // Using models from AATS.Desktop.Models (AppDocument, AuditRecord, CompanyCharacter)

    public partial class CompanyRegistrationDetailViewModel : DetailViewModelBase
    {
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _companyType = string.Empty;
        
        public override string Category => "Company Registration";

        [ObservableProperty] private ObservableCollection<CompanyCharacter> _directors = new();
        [ObservableProperty] private ObservableCollection<CompanyCharacter> _secretaries = new();
        [ObservableProperty] private ObservableCollection<CompanyCharacter> _others = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _nicDocuments = new();
        
        // TIN Registration Collections
        [ObservableProperty] private ObservableCollection<AppDocument> _allTinFiles = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredTinFiles = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _tempUploadFiles = new();
        
        [ObservableProperty] private string _selectedTinTab = "PROCESS";
        [ObservableProperty] private bool _isRegistrationPopupVisible;
        
        // Modal State
        [ObservableProperty] private AppDocument? _selectedUploadFile;

        // Preview State
        [ObservableProperty] private bool _isPreviewVisible;
        [ObservableProperty] private Bitmap? _previewImage;
        [ObservableProperty] private AppDocument? _previewingFile;

        // Global Edit State
        [ObservableProperty] private bool _isGlobalEditVisible;
        [ObservableProperty] private AppDocument? _editingDocument;

        public string[] AvailableCategories { get; } = { "PROCESS", "APPROVED", "PIN", "SSID" };
        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }


        public CompanyRegistrationDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            LoadFromRecord();
            UpdateFilteredFiles();
            _ = LoadFullRecordAsync(); // Load full record with Officers in background
        }

        protected override void OnRecordLoaded(AuditRecord? value)
        {
            LoadFromRecord();
            UpdateFilteredFiles();
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(CompanyType));
        }

        private void LoadFromRecord()
        {
            if (Record == null) return;

            Address = Record.Address ?? string.Empty;
            Email = Record.Email ?? string.Empty;
            Phone = Record.PhoneNo ?? string.Empty;
            CompanyType = Record.Type ?? string.Empty;

            if (Record.Officers != null && Record.Officers.Any())
            {
                var directors = Record.Officers.Where(o => o.Position == "Director")
                    .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
                Directors = new ObservableCollection<CompanyCharacter>(directors);

                var secretaries = Record.Officers.Where(o => o.Position == "Secretary")
                    .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
                Secretaries = new ObservableCollection<CompanyCharacter>(secretaries);

                var others = Record.Officers.Where(o => o.Position == "Other")
                    .Select(o => new CompanyCharacter { Name = o.Name, Detail = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
                Others = new ObservableCollection<CompanyCharacter>(others);
            }
            else
            {
                if (Record.DirectorsList != null)
                    Directors = new ObservableCollection<CompanyCharacter>(Record.DirectorsList);
                if (Record.SecretariesList != null)
                    Secretaries = new ObservableCollection<CompanyCharacter>(Record.SecretariesList);
                if (Record.OthersList != null)
                    Others = new ObservableCollection<CompanyCharacter>(Record.OthersList);
            }

            if (Record.RegistrationDocuments != null)
            {
                var nics = Record.RegistrationDocuments.Where(d => d.Category == "NIC").ToList();
                NicDocuments = new ObservableCollection<AppDocument>(nics);

                var tins = Record.RegistrationDocuments.Where(d => d.Category != "NIC").ToList();
                AllTinFiles = new ObservableCollection<AppDocument>(tins);
            }
        }

        private void UpdateFilteredFiles()
        {
            var filtered = AllTinFiles.Where(f => f.Category == SelectedTinTab).ToList();
            FilteredTinFiles = new ObservableCollection<AppDocument>(filtered);
        }

        partial void OnSelectedTinTabChanged(string value) => UpdateFilteredFiles();

        protected override void InitializeSteps()
        {
            var stepDefinitions = new List<(string Name, string? Icon)>
            {
                ("Name Approve", null),
                ("Forms Preparation", null),
                ("Signature", null),
                ("Payment", null),
                ("Incorporation", null),
                ("Seal", null),
                ("Certified copy", null),
                ("Document hand over", null)
            };

            SetupSteps(stepDefinitions);
        }

        [RelayCommand]
        private void SelectTinTab(string tabName)
        {
            SelectedTinTab = tabName;
        }

        [RelayCommand]
        private void AddRegistration()
        {
            TempUploadFiles.Clear();
            foreach (var file in AllTinFiles)
            {
                // Clone to temp for editing
                TempUploadFiles.Add(new AppDocument 
                { 
                    FileName = file.FileName, 
                    FileSize = file.FileSize, 
                    Category = file.Category,
                    Type = file.Type,
                    Description = file.Description,
                    IsExisting = true,
                    ImagePath = file.ImagePath
                });
            }
            SelectedUploadFile = null;
            IsRegistrationPopupVisible = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickRegistrationFile()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null && files.Length > 0)
                {
                    SelectedUploadFile = new AppDocument
                    {
                        FileName = files[0],
                        FileSize = "Pending...",
                        Category = SelectedTinTab,
                        IsExisting = false
                    };
                }
            }
        }

        [RelayCommand]
        private void ConfirmSelectedFile()
        {
            if (SelectedUploadFile != null)
            {
                TempUploadFiles.Add(SelectedUploadFile);
                SelectedUploadFile = null;
            }
        }

        [RelayCommand]
        private void EditTempFile(AppDocument file)
        {
            SelectedUploadFile = file;
        }

        [RelayCommand]
        private void RemoveTempFile(AppDocument file)
        {
            TempUploadFiles.Remove(file);
        }

        [RelayCommand]
        private void SaveUploads()
        {
            AllTinFiles.Clear();
            foreach (var file in TempUploadFiles)
            {
                file.IsExisting = true;
                AllTinFiles.Add(file);
            }
            UpdateFilteredFiles();
            IsRegistrationPopupVisible = false;
        }

        [RelayCommand]
        private void CloseRegistrationPopup()
        {
            IsRegistrationPopupVisible = false;
            SelectedUploadFile = null;
        }

        [RelayCommand]
        private void CancelPick()
        {
            SelectedUploadFile = null;
        }

        [RelayCommand]
        private void PreviewDocument(object parameter)
        {
            if (parameter is AppDocument doc)
            {
                PreviewingFile = doc;
                try
                {
                    PreviewImage = new Bitmap(AssetLoader.Open(new Uri(doc.ImagePath)));
                }
                catch
                {
                    PreviewImage = new Bitmap(AssetLoader.Open(new Uri("avares://AATS.Desktop/Assets/logo.png")));
                }
                IsPreviewVisible = true;
            }
        }

        [RelayCommand]
        private void ClosePreview()
        {
            IsPreviewVisible = false;
            PreviewImage = null;
            PreviewingFile = null;
        }

        [RelayCommand]
        private void EditDocument(object parameter)
        {
            if (parameter is AppDocument doc)
            {
                EditingDocument = new AppDocument
                {
                    FileName = doc.FileName,
                    FileSize = doc.FileSize,
                    Category = doc.Category,
                    Type = doc.Type,
                    Description = doc.Description,
                    ImagePath = doc.ImagePath
                };
                
                _originalEditingSource = doc;
                IsGlobalEditVisible = true;
            }
        }

        private AppDocument? _originalEditingSource;

        [RelayCommand]
        private void SaveGlobalEdit()
        {
            if (EditingDocument != null && _originalEditingSource != null)
            {
                _originalEditingSource.FileName = EditingDocument.FileName;
                _originalEditingSource.Category = EditingDocument.Category;
                _originalEditingSource.Type = EditingDocument.Type;
                _originalEditingSource.Description = EditingDocument.Description;
                
                UpdateFilteredFiles();
            }
            IsGlobalEditVisible = false;
        }

        [RelayCommand]
        private void CloseGlobalEdit()
        {
            IsGlobalEditVisible = false;
            EditingDocument = null;
            _originalEditingSource = null;
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadFromRecord();
            UpdateFilteredFiles();
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(CompanyType));
        }

        public override void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete record '{ID}' for '{ClientName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await DataService.Instance.DeleteAuditRecordsAsync("Company Registration", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }

        [RelayCommand]
        private void DeleteDocument(object parameter)
        {
            if (parameter is AppDocument doc)
            {
                ShowConfirmDialog("Are you sure you want to delete this document?", async () =>
                {
                    if (NicDocuments.Contains(doc))
                    {
                        NicDocuments.Remove(doc);
                    }
                    else if (AllTinFiles.Contains(doc))
                    {
                        AllTinFiles.Remove(doc);
                        UpdateFilteredFiles();
                    }
                });
            }
        }

        private void ShowConfirmDialog(string message, Func<System.Threading.Tasks.Task> confirmAction)
        {
             ConfirmDialogTitle = "Confirmation";
             ConfirmDialogMessage = message;
             ConfirmActionDelegate = confirmAction;
             IsConfirmDialogVisible = true;
        }
    }
}