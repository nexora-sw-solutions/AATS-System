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
        [ObservableProperty] private ObservableCollection<CompanyCharacter> _shareholders = new();
                [ObservableProperty] private ObservableCollection<AppDocument> _allNicDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredNicDocuments = new();
        
        // TIN Registration Collections
        [ObservableProperty] private ObservableCollection<AppDocument> _allTinFiles = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredTinFiles = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _tempUploadFiles = new();
        
        [ObservableProperty] private string _selectedTinTab = "PROCESS";
        [ObservableProperty] private bool _isRegistrationPopupVisible;
        [ObservableProperty] private bool _isViewAllTinRecordsPopupVisible;
        
        // Modal State
        [ObservableProperty] private AppDocument? _selectedUploadFile;
        [ObservableProperty] private bool _isTinRecordDetailsVisible;
        [ObservableProperty] private AppDocument? _previewingTinRecord;
        [ObservableProperty] private bool _isTinRecordEditMode;
        [ObservableProperty] private AppDocument? _editingTinRecord;
        [ObservableProperty] private ObservableCollection<AppDocument> _additionalEditingFiles = new();

        // Process Document Collections
        [ObservableProperty] private ObservableCollection<AppDocument> _allProcessDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();
        [ObservableProperty] private string _selectedProcessDocumentTab = "Name Approval";
        [ObservableProperty] private string _selectedSignatureTab = "Form 01";
        [ObservableProperty] private string _selectedProcessDocumentsCardTab = "Name Approval";
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessCardDocuments = new();

        public bool IsNameApprovalUploadVisible => Record?.CurrentStep == 1;
        public bool IsSignatureUploadVisible => Record?.CurrentStep == 3;
        public bool IsIncorporationUploadVisible => Record?.CurrentStep == 5;
        public bool IsCertifiedCopiesUploadVisible => Record?.CurrentStep == 7;

        // Attachments Card
        [ObservableProperty] private ObservableCollection<AppDocument> _allAttachmentFiles = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredAttachmentFiles = new();
        [ObservableProperty] private string _selectedAttachmentTab = "Form 01";


        // Preview State
        [ObservableProperty] private bool _isPreviewVisible;
        [ObservableProperty] private Bitmap? _previewImage;
        [ObservableProperty] private AppDocument? _previewingFile;

        
        [RelayCommand]
        private void SelectProcessDocumentsCardTab(string tabName)
        {
            SelectedProcessDocumentsCardTab = tabName;
        }

        // Global Edit State
        [ObservableProperty] private bool _isGlobalEditVisible;
        [ObservableProperty] private AppDocument? _editingDocument;


        [ObservableProperty] private string _selectedPersonnelTab = "Directors";
        [ObservableProperty] private string _selectedNicTab = "Directors";

        [ObservableProperty] private string? _boResponsiblePersonName;
        [ObservableProperty] private string? _boResponsiblePersonNicFileName;
        public bool HasBoPersonNic => !string.IsNullOrEmpty(BoResponsiblePersonNicFileName);

        [RelayCommand] private void SelectPersonnelTab(string tabName) => SelectedPersonnelTab = tabName;
        [RelayCommand] private void SelectNicTab(string tabName) => SelectedNicTab = tabName;

        [RelayCommand]
        private void PreviewBoNic()
        {
            if (string.IsNullOrWhiteSpace(BoResponsiblePersonNicFileName)) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(BoResponsiblePersonNicFileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error previewing BO Person NIC: {ex.Message}");
            }
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task DownloadBoNic()
        {
            if (string.IsNullOrWhiteSpace(BoResponsiblePersonNicFileName)) return;
            try
            {
                if (BoResponsiblePersonNicFileName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = System.IO.Path.GetFileName(new Uri(BoResponsiblePersonNicFileName).LocalPath);
                    if (string.IsNullOrWhiteSpace(fileName)) fileName = "bo_person_nic";
                    await ApiService.Instance.DownloadDocumentAsync(BoResponsiblePersonNicFileName, fileName);
                    NotificationService.Instance.AddNotification("Downloaded", $"'{fileName}' saved to Downloads.");
                }
                else if (System.IO.File.Exists(BoResponsiblePersonNicFileName))
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(BoResponsiblePersonNicFileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download BO Person NIC: {ex.Message}");
            }
        }

        public string[] AvailableCategories { get; } = { "PROCESS", "APPROVED", "PIN", "SSID" };
        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }


        public CompanyRegistrationDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            LoadFromRecord();
            UpdateFilteredFiles();
            UpdateFilteredAttachments();
            _ = LoadFullRecordAsync(); // Load full record with Officers in background
        }

        protected override void OnRecordLoaded(AuditRecord? value)
        {
            LoadFromRecord();
            UpdateFilteredFiles();
            UpdateFilteredAttachments();
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(CompanyType));
        }

        protected override void UpdateStepStates()
        {
            base.UpdateStepStates();
            OnPropertyChanged(nameof(IsNameApprovalUploadVisible));
            OnPropertyChanged(nameof(IsSignatureUploadVisible));
            OnPropertyChanged(nameof(IsIncorporationUploadVisible));
            OnPropertyChanged(nameof(IsCertifiedCopiesUploadVisible));
            
            // Auto switch tab based on step
            if (Record?.CurrentStep == 1) SelectedProcessDocumentTab = "Name Approval";
            else if (Record?.CurrentStep == 3) SelectedProcessDocumentTab = "Signature";
            else if (Record?.CurrentStep == 5) SelectedProcessDocumentTab = "Incorporation";
            else if (Record?.CurrentStep == 7) SelectedProcessDocumentTab = "Certified Copies";
        }

        partial void OnSelectedProcessDocumentTabChanged(string value) => UpdateFilteredProcessDocuments();
        partial void OnSelectedSignatureTabChanged(string value) => UpdateFilteredProcessDocuments();

        partial void OnSelectedProcessDocumentsCardTabChanged(string value) => UpdateFilteredProcessCardDocuments();

        private void UpdateFilteredProcessCardDocuments()
        {
            var filtered = AllProcessDocuments.Where(f => f.Category == SelectedProcessDocumentsCardTab).ToList();
            FilteredProcessCardDocuments = new ObservableCollection<AppDocument>(filtered);
        }

        private void UpdateFilteredProcessDocuments()
        {
            var filtered = AllProcessDocuments.Where(f => f.Category == SelectedProcessDocumentTab);
            if (SelectedProcessDocumentTab == "Signature")
            {
                filtered = filtered.Where(f => f.Type == SelectedSignatureTab);
            }
            FilteredProcessDocuments = new ObservableCollection<AppDocument>(filtered.ToList());
            UpdateFilteredProcessCardDocuments();
        }

        [RelayCommand]
        private void SelectProcessDocumentTab(string tabName)
        {
            SelectedProcessDocumentTab = tabName;
        }

        [RelayCommand]
        private void SelectSignatureTab(string tabName)
        {
            SelectedSignatureTab = tabName;
        }



        [RelayCommand]
        private async System.Threading.Tasks.Task UploadProcessDocument(string documentType)
        {
            if (RequestFilePicker == null) return;
            var paths = await RequestFilePicker();
            if (paths == null || paths.Length == 0) return;

            string category = "Name Approval";
            if (Record?.CurrentStep == 3) category = "Signature";
            else if (Record?.CurrentStep == 5) category = "Incorporation";
            else if (Record?.CurrentStep == 7) category = "Certified Copies";

            foreach (var path in paths)
            {
                var doc = new AppDocument
                {
                    FileName = System.IO.Path.GetFileName(path),
                    ImagePath = path,
                    Category = category,
                    Type = documentType,
                    FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB",
                    IsExisting = false
                };
                AllProcessDocuments.Add(doc);
            }
            UpdateFilteredProcessDocuments();
        }

        [RelayCommand]
        private void RemoveProcessDocument(AppDocument doc)
        {
            if (doc != null)
            {
                var documentsInStage = AllProcessDocuments.Count(x => x.Category == doc.Category);
                if (documentsInStage <= 1)
                {
                    ShowConfirmDialog("Cannot delete the final remaining document for a completed stage.", () => System.Threading.Tasks.Task.CompletedTask);
                    return;
                }
                
                ShowConfirmDialog($"Are you sure you want to delete {doc.FileName}?", async () =>
                {
                    AllProcessDocuments.Remove(doc);
                    UpdateFilteredProcessDocuments();
                    await System.Threading.Tasks.Task.CompletedTask;
                });
            }
        }

        [RelayCommand]
        private void PreviewProcessDocument(AppDocument doc)
        {
            PreviewDocumentCommand.Execute(doc); // Re-use existing preview logic
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
                
                var shareholders = Record.Officers.Where(o => o.Position == "Shareholder")
                    .Select(o => new CompanyCharacter { Name = o.Name, Role = o.Position, TIN = o.NicNumber }).ToList();
                Shareholders = new ObservableCollection<CompanyCharacter>(shareholders);
            }
            else
            {
                if (Record.DirectorsList != null)
                    foreach (var d in Record.DirectorsList)
                        Directors.Add(new CompanyCharacter { Name = d.Name, Phone = d.Phone, Address = d.Address, TIN = d.TIN, Email = d.Email, NicFileName = d.NicFileName, HasNicFile = d.HasNicFile });
                        
                if (Record.ShareholdersList != null)
                    foreach (var s in Record.ShareholdersList)
                        Shareholders.Add(new CompanyCharacter { Name = s.Name, Phone = s.Phone, Address = s.Address, TIN = s.TIN, Email = s.Email, SharePercentage = s.SharePercentage, NicFileName = s.NicFileName, HasNicFile = s.HasNicFile });
                        
                if (Record.SecretariesList != null)
                    foreach (var sc in Record.SecretariesList)
                        Secretaries.Add(new CompanyCharacter { Name = sc.Name, Phone = sc.Phone, Address = sc.Address, TIN = sc.TIN, Email = sc.Email, NicFileName = sc.NicFileName, HasNicFile = sc.HasNicFile });
                        
                if (Record.OthersList != null)
                    foreach (var o in Record.OthersList)
                        Others.Add(new CompanyCharacter { Detail = o.Detail, Role = o.Role, Phone = o.Phone, Address = o.Address, Email = o.Email, NicFileName = o.NicFileName, HasNicFile = o.HasNicFile });
            }


            BoResponsiblePersonName = Record.BoResponsiblePersonName;
            BoResponsiblePersonNicFileName = Record.BoResponsiblePersonNicFileName;
            OnPropertyChanged(nameof(HasBoPersonNic));

            var allNics = new List<AppDocument>();
            foreach (var d in Directors.Where(x => x.HasNicFile))
                allNics.Add(new AppDocument { FileName = System.IO.Path.GetFileName(d.NicFileName) ?? "NIC", ImagePath = d.NicFileName ?? "", Category = "Directors", IsExisting = true });
            foreach (var d in Secretaries.Where(x => x.HasNicFile))
                allNics.Add(new AppDocument { FileName = System.IO.Path.GetFileName(d.NicFileName) ?? "NIC", ImagePath = d.NicFileName ?? "", Category = "Secretaries", IsExisting = true });
            foreach (var d in Shareholders.Where(x => x.HasNicFile))
                allNics.Add(new AppDocument { FileName = System.IO.Path.GetFileName(d.NicFileName) ?? "NIC", ImagePath = d.NicFileName ?? "", Category = "Shareholders", IsExisting = true });
            foreach (var d in Others.Where(x => x.HasNicFile))
                allNics.Add(new AppDocument { FileName = System.IO.Path.GetFileName(d.NicFileName) ?? "NIC", ImagePath = d.NicFileName ?? "", Category = "Others", IsExisting = true });
            AllNicDocuments = new ObservableCollection<AppDocument>(allNics);
            UpdateFilteredNicDocuments();

            if (Record.SourceDocuments != null)
            {
                var processDocs = Record.SourceDocuments
                    .Where(d => d.Description != null && d.Description.StartsWith("Process|"))
                    .Select(d => 
                    {
                        var parts = d.Description.Split('|');
                        var category = parts.Length > 1 ? parts[1] : "PROCESS";
                        var type = parts.Length > 2 ? parts[2] : "";
                        return new AppDocument
                        {
                            
                            FileName = d.FileName ?? string.Empty,
                            FileSize = d.FileSize?.ToString() ?? "Unknown",
                            Category = category,
                            Type = type,
                            Description = d.Description,
                            ImagePath = d.Url ?? string.Empty,
                            IsExisting = true
                        };
                    }).ToList();
                AllProcessDocuments = new ObservableCollection<AppDocument>(processDocs);

                var tins = Record.SourceDocuments
                    .Where(d => d.Description != "NIC" && d.Description != "NIC Document" && !(d.Description?.StartsWith("Process|") ?? false))
                    .Select(d => new AppDocument
                    {
                        
                        FileName = d.FileName ?? string.Empty,
                        FileSize = d.FileSize?.ToString() ?? "Unknown",
                        Category = d.Description ?? "PROCESS",
                        ImagePath = d.Url ?? string.Empty,
                        IsExisting = true
                    }).ToList();
                AllTinFiles = new ObservableCollection<AppDocument>(tins);
            }
            UpdateFilteredProcessDocuments();

            var allAttachments = new List<AppDocument>();
            if (Record.Form01Attachments != null)
            {
                allAttachments.AddRange(Record.Form01Attachments.Select(d => new AppDocument
                {
                    FileName = d.FileName ?? string.Empty,
                    FileSize = "Unknown",
                    Category = "Form 01",
                    ImagePath = d.Url ?? string.Empty,
                    IsExisting = true
                }));
            }
            if (Record.BoFormAttachments != null)
            {
                allAttachments.AddRange(Record.BoFormAttachments.Select(d => new AppDocument
                {
                    FileName = d.FileName ?? string.Empty,
                    FileSize = "Unknown",
                    Category = "BO Form",
                    ImagePath = d.Url ?? string.Empty,
                    IsExisting = true
                }));
            }
            if (Record.Form05Attachments != null)
            {
                allAttachments.AddRange(Record.Form05Attachments.Select(d => new AppDocument
                {
                    FileName = d.FileName ?? string.Empty,
                    FileSize = "Unknown",
                    Category = "Form 05",
                    ImagePath = d.Url ?? string.Empty,
                    IsExisting = true
                }));
            }
            AllAttachmentFiles = new ObservableCollection<AppDocument>(allAttachments);
        }

        private void UpdateFilteredFiles()
        {
            var filtered = AllTinFiles.Where(f => f.Category == SelectedTinTab).ToList();
            FilteredTinFiles = new ObservableCollection<AppDocument>(filtered);
        }

        private void UpdateFilteredAttachments()
        {
            var filtered = AllAttachmentFiles.Where(f => f.Category == SelectedAttachmentTab).ToList();
            FilteredAttachmentFiles = new ObservableCollection<AppDocument>(filtered);
        }

        partial void OnSelectedNicTabChanged(string value) => UpdateFilteredNicDocuments();
        private void UpdateFilteredNicDocuments()
        {
            var filtered = AllNicDocuments.Where(f => f.Category == SelectedNicTab).ToList();
            FilteredNicDocuments = new ObservableCollection<AppDocument>(filtered);
        }
        partial void OnSelectedTinTabChanged(string value) => UpdateFilteredFiles();
        partial void OnSelectedAttachmentTabChanged(string value) => UpdateFilteredAttachments();

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
                ("Certified copy", null)
            };

            SetupSteps(stepDefinitions);
        }

        [RelayCommand]
        private void SelectTinTab(string tabName)
        {
            SelectedTinTab = tabName;
        }

        [RelayCommand]
        private void SelectAttachmentTab(string tabName)
        {
            SelectedAttachmentTab = tabName;
        }

        [RelayCommand]
        private void AddRegistration()
        {
            TempUploadFiles.Clear();
            SelectedUploadFile = new AppDocument { Category = SelectedTinTab, IsExisting = false, FileName = "" };
            IsRegistrationPopupVisible = true;
        }

        [RelayCommand]
        private void OpenViewAllTinRecords()
        {
            IsViewAllTinRecordsPopupVisible = true;
        }

        [RelayCommand]
        private void CloseViewAllTinRecords()
        {
            IsViewAllTinRecordsPopupVisible = false;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickRegistrationFile()
        {
            System.Console.WriteLine("[DEBUG] PickRegistrationFile command invoked!");
            if (RequestFilePicker != null)
            {
                System.Console.WriteLine("[DEBUG] RequestFilePicker callback is not null, invoking...");
                var files = await RequestFilePicker();
                System.Console.WriteLine($"[DEBUG] RequestFilePicker returned files count: {files?.Length ?? 0}");
                if (files != null && files.Length > 0)
                {
                    var oldCat = SelectedUploadFile?.Category ?? SelectedTinTab;
                    var oldType = SelectedUploadFile?.Type;
                    var oldDesc = SelectedUploadFile?.Description;
                    
                    SelectedUploadFile = new AppDocument
                    {
                        FileName = files[0],
                        FileSize = "Pending...",
                        Category = oldCat,
                        Type = oldType,
                        Description = oldDesc,
                        IsExisting = false
                    };
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG] RequestFilePicker callback is NULL!");
            }
        }

        [RelayCommand]
        private void ConfirmSelectedFile()
        {
            if (SelectedUploadFile != null)
            {
                if (string.IsNullOrWhiteSpace(SelectedUploadFile.FileName))
                {
                    SelectedUploadFile.FileName = string.IsNullOrWhiteSpace(SelectedUploadFile.Description) ? "(No Document Attached)" : SelectedUploadFile.Description;
                }
                TempUploadFiles.Add(SelectedUploadFile);
                SelectedUploadFile = new AppDocument { Category = SelectedTinTab, IsExisting = false, FileName = "" };
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
        private async System.Threading.Tasks.Task SaveUploads()
        {
            IsBusy = true;
            try
            {
                var tempId = Record?.ID ?? Guid.NewGuid().ToString();
                
                // 1. Upload any new files to R2
                foreach (var file in TempUploadFiles)
                {
                    if (!file.IsExisting && !string.IsNullOrEmpty(file.FileName))
                    {
                        var localPath = file.FileName;
                        if (System.IO.File.Exists(localPath))
                        {
                            var uploaded = await ApiService.Instance.UploadDocumentsAsync(
                                new List<string> { localPath },
                                "Secretarial & Advisory",
                                tempId
                            );
                            if (uploaded != null && uploaded.Count > 0)
                            {
                                file.ImagePath = uploaded[0].Url ?? file.ImagePath;
                                file.FileName = System.IO.Path.GetFileName(localPath);
                            }
                        }
                    }
                    file.IsExisting = true;
                }

                // Update AllTinFiles list
                foreach (var file in TempUploadFiles)
                {
                    AllTinFiles.Add(file);
                }
                UpdateFilteredFiles();
                
                // 2. Synchronize to Record.SourceDocuments
                SyncToRecordSourceDocuments();

                // 3. Save to backend database
                if (Record != null)
                {
                    await DataService.Instance.UpdateAuditRecordAsync("Company Registration", Record);
                    NotificationService.Instance.AddNotification("Success", "Documents saved successfully.");
                }

                IsRegistrationPopupVisible = false;
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Failed to save documents: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void HandleOverlayClick()
        {
            if (IsTinRecordDetailsVisible)
            {
                if (IsTinRecordEditMode)
                {
                    ConfirmDialogTitle = "Discard Changes?";
                    ConfirmDialogMessage = "You have unsaved changes. Are you sure you want to close this window and discard them?";
                    ConfirmActionDelegate = () =>
                    {
                        CloseTinRecordDetails();
                        return System.Threading.Tasks.Task.CompletedTask;
                    };
                    IsConfirmDialogVisible = true;
                }
                else
                {
                    CloseTinRecordDetails();
                }
            }
            else if (IsRegistrationPopupVisible)
            {
                if (TempUploadFiles.Count > 0)
                {
                    ConfirmDialogTitle = "Discard Changes?";
                    ConfirmDialogMessage = "You have unsaved changes. Are you sure you want to close this window and discard them?";
                    ConfirmActionDelegate = () =>
                    {
                        CloseRegistrationPopup();
                        return System.Threading.Tasks.Task.CompletedTask;
                    };
                    IsConfirmDialogVisible = true;
                }
                else
                {
                    CloseRegistrationPopup();
                }
            }
            else if (IsViewAllTinRecordsPopupVisible)
            {
                IsViewAllTinRecordsPopupVisible = false;
            }
            else if (IsGlobalEditVisible)
            {
                if (AdditionalEditingFiles.Count > 0)
                {
                    ConfirmDialogTitle = "Discard Changes?";
                    ConfirmDialogMessage = "You have unsaved changes. Are you sure you want to close this window and discard them?";
                    ConfirmActionDelegate = () =>
                    {
                        CloseGlobalEdit();
                        return System.Threading.Tasks.Task.CompletedTask;
                    };
                    IsConfirmDialogVisible = true;
                }
                else
                {
                    CloseGlobalEdit();
                }
            }
            else if (IsPreviewVisible)
            {
                ClosePreview();
            }
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
        private void ViewTinRecord(AppDocument doc)
        {
            if (doc != null)
            {
                PreviewingTinRecord = doc;
                IsTinRecordEditMode = false;
                IsTinRecordDetailsVisible = true;
            }
        }

        [RelayCommand]
        private void CloseTinRecordDetails()
        {
            IsTinRecordDetailsVisible = false;
            IsTinRecordEditMode = false;
            PreviewingTinRecord = null;
        }

        [RelayCommand]
        private void EditTinRecord(AppDocument doc)
        {
            if (doc != null)
            {
                PreviewingTinRecord = doc;
                
                EditingTinRecord = new AppDocument
                {
                    FileName = doc.FileName,
                    FileSize = doc.FileSize,
                    Category = doc.Category,
                    Type = doc.Type,
                    Description = doc.Description,
                    ImagePath = doc.ImagePath,
                    IsExisting = doc.IsExisting
                };

                AdditionalEditingFiles.Clear();

                IsTinRecordEditMode = true;
                IsTinRecordDetailsVisible = true;
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PickEditingTinFile()
        {
            if (RequestFilePicker != null)
            {
                var files = await RequestFilePicker();
                if (files != null && files.Length > 0 && EditingTinRecord != null)
                {
                    // First file replaces the main attachment
                    EditingTinRecord.FileName = files[0];
                    EditingTinRecord.FileSize = "Pending...";
                    EditingTinRecord.IsExisting = false;

                    // Any additional files go to the queue
                    for (int i = 1; i < files.Length; i++)
                    {
                        AdditionalEditingFiles.Add(new AppDocument
                        {
                            FileName = files[i],
                            FileSize = "Pending...",
                            Category = EditingTinRecord.Category,
                            Type = EditingTinRecord.Type,
                            Description = EditingTinRecord.Description,
                            IsExisting = false
                        });
                    }
                }
            }
        }

        [RelayCommand]
        private void RemoveEditingTinFile()
        {
            if (EditingTinRecord != null)
            {
                EditingTinRecord.FileName = string.Empty;
                EditingTinRecord.FileSize = string.Empty;
                EditingTinRecord.ImagePath = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveAdditionalEditingFile(AppDocument doc)
        {
            if (doc != null && AdditionalEditingFiles.Contains(doc))
            {
                AdditionalEditingFiles.Remove(doc);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task SaveTinRecordEdit()
        {
            if (EditingTinRecord != null && PreviewingTinRecord != null)
            {
                IsBusy = true;
                try
                {
                    // Upload main file if new
                    if (!EditingTinRecord.IsExisting && !string.IsNullOrEmpty(EditingTinRecord.FileName))
                    {
                        var localPath = EditingTinRecord.FileName;
                        if (System.IO.File.Exists(localPath))
                        {
                            var uploaded = await ApiService.Instance.UploadDocumentsAsync(
                                new List<string> { localPath },
                                "Secretarial & Advisory",
                                Record?.ID ?? ""
                            );
                            if (uploaded != null && uploaded.Count > 0)
                            {
                                EditingTinRecord.ImagePath = uploaded[0].Url ?? EditingTinRecord.ImagePath;
                                EditingTinRecord.FileName = System.IO.Path.GetFileName(localPath);
                            }
                        }
                    }

                    // Copy values back
                    PreviewingTinRecord.FileName = EditingTinRecord.FileName;
                    PreviewingTinRecord.FileSize = EditingTinRecord.FileSize;
                    PreviewingTinRecord.Category = EditingTinRecord.Category;
                    PreviewingTinRecord.Type = EditingTinRecord.Type;
                    PreviewingTinRecord.Description = EditingTinRecord.Description;
                    PreviewingTinRecord.ImagePath = EditingTinRecord.ImagePath;
                    PreviewingTinRecord.IsExisting = true;
                    
                    // Process additional files if any
                    foreach(var file in AdditionalEditingFiles)
                    {
                        if (!file.IsExisting && !string.IsNullOrEmpty(file.FileName))
                        {
                            var localPath = file.FileName;
                            if (System.IO.File.Exists(localPath))
                            {
                                var uploaded = await ApiService.Instance.UploadDocumentsAsync(
                                    new List<string> { localPath },
                                    "Secretarial & Advisory",
                                    Record?.ID ?? ""
                                );
                                if (uploaded != null && uploaded.Count > 0)
                                {
                                    file.ImagePath = uploaded[0].Url ?? file.ImagePath;
                                    file.FileName = System.IO.Path.GetFileName(localPath);
                                }
                            }
                        }
                        file.IsExisting = true;
                        AllTinFiles.Add(file);
                    }

                    UpdateFilteredFiles();
                    SyncToRecordSourceDocuments();
                    
                    if (Record != null)
                    {
                        await DataService.Instance.UpdateAuditRecordAsync("Company Registration", Record);
                    }
                    NotificationService.Instance.AddNotification("Success", "Record updated successfully.");
                }
                catch (Exception ex)
                {
                    NotificationService.Instance.AddNotification("Error", $"Failed to update record: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }

            IsTinRecordEditMode = false;
        }

        [RelayCommand]
        private void CancelTinRecordEdit()
        {
            IsTinRecordEditMode = false;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task PreviewDocument(object parameter)
        {
            string? pathOrUrl = null;
            string? fileName = null;

            if (parameter is AppDocument appDoc)
            {
                pathOrUrl = !string.IsNullOrWhiteSpace(appDoc.Url) ? appDoc.Url : appDoc.ImagePath;
                fileName = appDoc.FileName;
                PreviewingFile = appDoc;
            }
            else if (parameter is SourceDocument srcDoc)
            {
                pathOrUrl = srcDoc.Url;
                fileName = srcDoc.FileName;
            }
            else if (parameter is string strParam)
            {
                pathOrUrl = strParam;
            }

            if (string.IsNullOrWhiteSpace(pathOrUrl)) return;

            string fullUrl = ApiService.GetFullDocumentUrl(pathOrUrl);
            string ext = System.IO.Path.GetExtension(fileName ?? fullUrl)?.ToLowerInvariant() ?? "";
            bool isImage = ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp";

            if (isImage)
            {
                try
                {
                    if (fullUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        var bytes = await ApiService.Instance.Client.GetByteArrayAsync(fullUrl);
                        using var stream = new System.IO.MemoryStream(bytes);
                        PreviewImage = new Bitmap(stream);
                    }
                    else if (System.IO.File.Exists(fullUrl))
                    {
                        using var stream = System.IO.File.OpenRead(fullUrl);
                        PreviewImage = new Bitmap(stream);
                    }
                    else
                    {
                        PreviewImage = new Bitmap(AssetLoader.Open(new Uri("avares://AATS.Desktop/Assets/New%20Logo.png")));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to load preview: {ex.Message}");
                    PreviewImage = new Bitmap(AssetLoader.Open(new Uri("avares://AATS.Desktop/Assets/New%20Logo.png")));
                }
                IsPreviewVisible = true;
            }
            else
            {
                // For non-images (like PDF), open in default viewer or browser
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullUrl) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to open document: {ex.Message}");
                }
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
                    ImagePath = doc.ImagePath,
                    Url = doc.Url
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
            AppDocument? docToDelete = parameter as AppDocument;
            if (docToDelete == null && parameter is string strName)
            {
                docToDelete = AllNicDocuments.FirstOrDefault(x => x.FileName == strName) ??
                              AllTinFiles.FirstOrDefault(x => x.FileName == strName) ??
                              AllAttachmentFiles.FirstOrDefault(x => x.FileName == strName) ??
                              AllProcessDocuments.FirstOrDefault(x => x.FileName == strName);
            }

            if (docToDelete != null)
            {
                ShowConfirmDialog($"Are you sure you want to delete '{docToDelete.FileName}'?", async () =>
                {
                    if (AllNicDocuments.Contains(docToDelete))
                    {
                        AllNicDocuments.Remove(docToDelete);
                        UpdateFilteredNicDocuments();
                    }
                    if (AllTinFiles.Contains(docToDelete))
                    {
                        AllTinFiles.Remove(docToDelete);
                        UpdateFilteredFiles();
                    }
                    if (AllAttachmentFiles.Contains(docToDelete))
                    {
                        AllAttachmentFiles.Remove(docToDelete);
                        UpdateFilteredAttachments();
                    }
                    if (AllProcessDocuments.Contains(docToDelete))
                    {
                        AllProcessDocuments.Remove(docToDelete);
                        UpdateFilteredProcessDocuments();
                    }

                    SyncToRecordSourceDocuments();

                    if (Record != null)
                    {
                        try
                        {
                            await DataService.Instance.UpdateAuditRecordAsync("Company Registration", Record);
                            NotificationService.Instance.AddNotification("Success", "Document deleted.");
                        }
                        catch (Exception ex)
                        {
                            NotificationService.Instance.AddNotification("Error", $"Failed to delete document: {ex.Message}");
                        }
                    }
                });
            }
        }

        private void SyncToRecordSourceDocuments()
        {
            if (Record == null) return;
            
            Record.SourceDocuments ??= new List<SourceDocument>();
            Record.SourceDocuments.Clear();
            
            foreach (var doc in AllNicDocuments)
            {
                Record.SourceDocuments.Add(new SourceDocument
                {
                    FileName = doc.FileName,
                    Url = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.ImagePath,
                    Description = "NIC"
                });
            }
            
            foreach (var doc in AllTinFiles)
            {
                Record.SourceDocuments.Add(new SourceDocument
                {
                    FileName = doc.FileName,
                    Url = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.ImagePath,
                    Description = doc.Category
                });
            }

            foreach (var doc in AllProcessDocuments)
            {
                Record.SourceDocuments.Add(new SourceDocument
                {
                    FileName = doc.FileName,
                    Url = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.ImagePath,
                    Description = $"Process|{doc.Category}|{doc.Type}"
                });
            }

            Record.Form01Attachments = AllAttachmentFiles.Where(a => a.Category == "Form 01")
                .Select(a => new SourceDocument { FileName = a.FileName, Url = !string.IsNullOrWhiteSpace(a.Url) ? a.Url : a.ImagePath }).ToList();

            Record.BoFormAttachments = AllAttachmentFiles.Where(a => a.Category == "BO Form")
                .Select(a => new SourceDocument { FileName = a.FileName, Url = !string.IsNullOrWhiteSpace(a.Url) ? a.Url : a.ImagePath }).ToList();

            Record.Form05Attachments = AllAttachmentFiles.Where(a => a.Category == "Form 05")
                .Select(a => new SourceDocument { FileName = a.FileName, Url = !string.IsNullOrWhiteSpace(a.Url) ? a.Url : a.ImagePath }).ToList();
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task DownloadDocument(object parameter)
        {
            string? pathOrUrl = null;
            string? fileName = null;

            if (parameter is AppDocument appDoc)
            {
                pathOrUrl = !string.IsNullOrWhiteSpace(appDoc.Url) ? appDoc.Url : appDoc.ImagePath;
                fileName = appDoc.FileName;
            }
            else if (parameter is SourceDocument srcDoc)
            {
                pathOrUrl = srcDoc.Url;
                fileName = srcDoc.FileName;
            }
            else if (parameter is string strParam)
            {
                pathOrUrl = strParam;
            }

            if (string.IsNullOrWhiteSpace(pathOrUrl)) return;
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "downloaded_document";

            string fullUrl = ApiService.GetFullDocumentUrl(pathOrUrl);

            try
            {
                if (fullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    await ApiService.Instance.DownloadDocumentAsync(fullUrl, fileName);
                    NotificationService.Instance.AddNotification("Downloaded", $"'{fileName}' saved to Downloads.");
                }
                else if (System.IO.File.Exists(fullUrl))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullUrl) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Could not download file: {ex.Message}");
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

