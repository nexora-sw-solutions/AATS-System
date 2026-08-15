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
    public partial class StaffDetailViewModel : DetailViewModelBase
    {
        private readonly StaffMember _staffMember;
        private readonly AuditRecord _parentRecord;

        public StaffDetailViewModel(AuditRecord parent, StaffMember member) : base(parent)
        {
            _parentRecord = parent;
            _staffMember = member;

            InitializeSteps();
            _ = LoadStaffDocumentsAsync();
            
            // Populate history
            if (_staffMember.History != null)
            {
                foreach (var h in _staffMember.History)
                {
                    History.Add(h);
                }
            }
        }

        public override string Category => "EPF / ETF";

        public string StaffIdDisplay => _staffMember.StaffId ?? "N/A";
        public int TotalStaffCount => _parentRecord.NoOfStaffs;
        public string StaffPhone => _staffMember.Phone ?? "N/A";

        // General Information fields
        public string RecordID => _parentRecord.Code ?? _parentRecord.ID ?? "N/A";
        public string StaffName => _staffMember.StaffName ?? "N/A";

        [ObservableProperty] private DateTimeOffset? _newTransactionDate = DateTimeOffset.Now;
        [ObservableProperty] private string _newTransactionDescription = string.Empty;
        [ObservableProperty] private string _newTransactionAmountText = string.Empty;
        [ObservableProperty] private bool _isAddTransactionVisible = false;

        public Func<Task<string?>>? RequestNicPicker { get; set; }
        public Func<Task<string?>>? RequestBrPicker { get; set; }
        public Func<Task<string?>>? RequestR1Picker { get; set; }
        public Func<Task<string?>>? RequestArtPicker { get; set; }
        public Func<Task<string?>>? RequestStaffNicPicker { get; set; }

        public ObservableCollection<object> History { get; } = new();
        public Action<AuditRecord, StaffMember>? NavigateToEditStaff { get; set; }

        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();
        private List<AppDocument> AllProcessDocuments { get; set; } = new();

        public bool IsCompleteUploadVisible => _staffMember.Process?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true;

        [RelayCommand]
        private async Task UploadProcessDocument(string documentType)
        {
            if (RequestBrPicker == null) return;
            var path = await RequestBrPicker();
            if (string.IsNullOrEmpty(path)) return;

            await UploadAndSyncDocumentAsync(path, documentType);
        }

        [RelayCommand]
        private void PreviewProcessDocument(AppDocument doc)
        {
            if (!string.IsNullOrEmpty(doc.ImagePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = doc.ImagePath, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to preview: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task EditProcessDocument(AppDocument doc)
        {
            if (doc == null || RequestBrPicker == null) return;
            
            var path = await RequestBrPicker();
            if (string.IsNullOrEmpty(path)) return;

            doc.FileName = System.IO.Path.GetFileName(path);
            doc.ImagePath = path;
            doc.FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB";
            
            UpdateFilteredProcessDocuments();
        }

        private void UpdateFilteredProcessDocuments()
        {
            var filtered = AllProcessDocuments.Where(x => x.Category == "BR Document");
            FilteredProcessDocuments = new ObservableCollection<AppDocument>(filtered.ToList());
        }

        [ObservableProperty] private string _selectedRequiredDocumentTab = "NIC";
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredRequiredDocuments = new();
        private List<AppDocument> AllRequiredDocuments { get; set; } = new();

        [RelayCommand]
        private void SelectRequiredDocumentTab(string tabName)
        {
            SelectedRequiredDocumentTab = tabName;
            UpdateFilteredRequiredDocuments();
        }

        private void UpdateFilteredRequiredDocuments()
        {
            var filtered = AllRequiredDocuments.Where(x => x.Category == SelectedRequiredDocumentTab);
            FilteredRequiredDocuments = new ObservableCollection<AppDocument>(filtered.ToList());
        }

        [RelayCommand]
        private async Task UploadRequiredDocument()
        {
            Func<Task<string?>>? picker = SelectedRequiredDocumentTab switch
            {
                "NIC" => RequestNicPicker,
                "R1" => RequestR1Picker,
                "ART" => RequestArtPicker,
                "STAFF NIC" => RequestStaffNicPicker,
                _ => null
            };
            
            if (picker == null) return;
            var path = await picker();
            if (string.IsNullOrEmpty(path)) return;

            await UploadAndSyncDocumentAsync(path, SelectedRequiredDocumentTab);
        }

        [RelayCommand]
        private async Task EditRequiredDocument(AppDocument doc)
        {
            if (doc == null) return;

            Func<Task<string?>>? picker = doc.Category switch
            {
                "NIC" => RequestNicPicker,
                "R1" => RequestR1Picker,
                "ART" => RequestArtPicker,
                "STAFF NIC" => RequestStaffNicPicker,
                _ => null
            };

            if (picker == null) return;
            var path = await picker();
            if (string.IsNullOrEmpty(path)) return;

            doc.FileName = System.IO.Path.GetFileName(path);
            doc.ImagePath = path;
            doc.FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB";
        }

        [RelayCommand]
        private void PreviewRequiredDocument(AppDocument doc)
        {
            if (!string.IsNullOrEmpty(doc.ImagePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = doc.ImagePath, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to preview: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void RemoveRequiredDocument(AppDocument doc)
        {
            if (doc != null)
            {
                ConfirmDialogTitle = "Confirm Deletion";
                ConfirmDialogMessage = $"Are you sure you want to delete {doc.FileName}?";
                ConfirmActionDelegate = async () =>
                {
                    try
                    {
                        var categoryDocs = AllRequiredDocuments.Where(d => d.Category == doc.Category).ToList();
                        if (categoryDocs.Count <= 1)
                        {
                            NotificationService.Instance.AddNotification("Warning", "At least one document is required for this category. Please replace it instead of deleting.");
                            return;
                        }

                        AllRequiredDocuments.Remove(doc);
                        UpdateFilteredRequiredDocuments();
                        NotificationService.Instance.AddNotification("Success", "Document deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        NotificationService.Instance.AddNotification("Error", $"Delete failed: {ex.Message}");
                    }
                };
                IsConfirmDialogVisible = true;
            }
        }

        public async Task LoadStaffDocumentsAsync()
        {
            try
            {
                if (_staffMember.Id.HasValue)
                {
                    var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ApiDocument>>>($"/api/v1/documents?recordId={_staffMember.Id}");
                    if (response?.Data?.Items != null)
                    {
                        var staffDocs = response.Data.Items
                            .Where(d => d.RecordId == _staffMember.Id.Value && d.RecordType.Equals("StaffMember", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var doc in staffDocs)
                        {
                            if (doc.Category == "NIC" || doc.Category == "R1" || doc.Category == "ART" || doc.Category == "Staff NIC")
                            {
                                var appDoc = new AppDocument
                                {
                                    FileName = doc.FileName ?? "Unknown",
                                    Category = doc.Category == "Staff NIC" ? "STAFF NIC" : doc.Category,
                                    ImagePath = doc.StorageKey,
                                    IsExisting = true
                                };
                                AllRequiredDocuments.Add(appDoc);
                            }
                            else if (doc.Category == "BR")
                            {
                                var brDoc = new AppDocument
                                {
                                    FileName = doc.FileName ?? "Unknown",
                                    Category = "BR Document",
                                    ImagePath = doc.StorageKey,
                                    IsExisting = true
                                };
                                AllProcessDocuments.Add(brDoc);
                            }
                        }
                        UpdateFilteredRequiredDocuments();
                        UpdateFilteredProcessDocuments();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to load staff documents: {ex.Message}");
            }
        }

        private async Task UploadAndSyncDocumentAsync(string localPath, string category)
        {
            try
            {
                if (string.IsNullOrEmpty(localPath) || !System.IO.File.Exists(localPath)) return;

                var newDoc = new ApiDocument
                {
                    FileName = System.IO.Path.GetFileName(localPath),
                    StorageKey = localPath, // Simulated storage key
                    Category = category
                };

                if (category == "BR" || category == "BR Document") 
                {
                    var brDoc = new AppDocument
                    {
                        FileName = newDoc.FileName ?? "Unknown",
                        Category = "BR Document",
                        ImagePath = newDoc.StorageKey,
                        FileSize = (new System.IO.FileInfo(localPath).Length / 1024).ToString() + " KB",
                        IsExisting = true
                    };
                    AllProcessDocuments.Add(brDoc);
                    UpdateFilteredProcessDocuments();
                }
                else 
                {
                    var normCategory = category == "Staff NIC" ? "STAFF NIC" : category;
                    AllRequiredDocuments.Add(new AppDocument
                    {
                        FileName = newDoc.FileName ?? "Unknown",
                        Category = normCategory,
                        ImagePath = newDoc.StorageKey,
                        FileSize = (new System.IO.FileInfo(localPath).Length / 1024).ToString() + " KB",
                        IsExisting = true
                    });
                    UpdateFilteredRequiredDocuments();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Instance.AddNotification("Error", $"Upload failed: {ex.Message}");
            }
        }

        protected override void InitializeSteps()
        {
            var stepDefs = new List<(string Name, string? Icon)>
            {
                ("Processing", "fa-solid fa-spinner"),
                ("Submit", "fa-solid fa-circle-check"),
                ("Completed", "fa-solid fa-flag-checkered")
            };
            
            SetupSteps(stepDefs);
        }

        protected override void UpdateStepStates()
        {
            if (_staffMember == null || Steps == null || Steps.Count == 0) return;

            int currentStep = 1; 
            if (_staffMember.Process?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true) currentStep = 3;
            else if (_staffMember.Process?.Equals("SUBMIT", StringComparison.OrdinalIgnoreCase) == true) currentStep = 2;
            else if (_staffMember.Process?.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase) == true) currentStep = 1;
            
            for (int i = 0; i < Steps.Count; i++)
            {
                int stepNum = i + 1;
                Steps[i].IsActive = stepNum <= currentStep;
                Steps[i].IsClickable = (stepNum == currentStep + 1) || (stepNum < currentStep);
            }
        }

        [RelayCommand]
        private void StaffStepClick(ProcessStep step)
        {
            if (!step.IsClickable) return;

            int currentStep = 1;
            if (_staffMember.Process?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true) currentStep = 3;
            else if (_staffMember.Process?.Equals("SUBMIT", StringComparison.OrdinalIgnoreCase) == true) currentStep = 2;
            else if (_staffMember.Process?.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase) == true) currentStep = 1;

            ConfirmDialogTitle = step.Number > currentStep ? "Advance Process?" : "Move Back Process?";
            ConfirmDialogMessage = $"Are you sure you want to change the status to '{step.Name}'?";
            ConfirmActionDelegate = async () =>
            {
                _staffMember.Process = step.Name.ToUpper();
                
                try 
                {
                    await SaveRecordUpdateAsync();
                    NotificationService.Instance.AddNotification("Success", $"Status updated to {_staffMember.Process}");
                }
                catch (Exception ex)
                {
                    NotificationService.Instance.AddNotification("Error", $"Failed to update status: {ex.Message}");
                }
                
                UpdateStepStates();
                OnPropertyChanged(nameof(IsCompleteUploadVisible));
            };
            IsConfirmDialogVisible = true;
        }

        [RelayCommand]
        private void ShowAddTransaction()
        {
            NewTransactionDate = DateTime.Now;
            NewTransactionDescription = "";
            NewTransactionAmountText = "";
            IsAddTransactionVisible = true;
        }

        [RelayCommand]
        private void SaveNewTransaction()
        {
            if (string.IsNullOrWhiteSpace(NewTransactionDescription)) return;
            if (!decimal.TryParse(NewTransactionAmountText, out decimal amount) || amount <= 0) return;

            ConfirmDialogTitle = "Add Transaction?";
            ConfirmDialogMessage = $"Add transaction {NewTransactionDescription} for {amount:C}?";
            ConfirmActionDelegate = async () =>
            {
                History.Insert(0, new StaffHistory
                {
                    Date = NewTransactionDate?.DateTime ?? DateTime.Now,
                    Description = NewTransactionDescription,
                    Amount = amount
                });
                
                IsAddTransactionVisible = false;
                NewTransactionDescription = "";
                NewTransactionAmountText = "";
                NotificationService.Instance.AddNotification("Success", "Transaction added successfully.");
            };
            IsConfirmDialogVisible = true;
        }

        [RelayCommand]
        private void CancelAddTransaction()
        {
            if (!string.IsNullOrEmpty(NewTransactionDescription) || !string.IsNullOrEmpty(NewTransactionAmountText))
            {
                ConfirmDialogTitle = "Discard Changes?";
                ConfirmDialogMessage = "Are you sure you want to discard this transaction?";
                ConfirmActionDelegate = async () =>
                {
                    IsAddTransactionVisible = false;
                };
                IsConfirmDialogVisible = true;
            }
            else
            {
                IsAddTransactionVisible = false;
            }
        }
    }
}
