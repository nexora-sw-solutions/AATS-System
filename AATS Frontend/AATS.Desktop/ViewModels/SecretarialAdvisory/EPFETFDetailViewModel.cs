using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class EPFETFDetailViewModel : DetailViewModelBase
    {
        [ObservableProperty] private ObservableCollection<StaffMember> _staffList = new();
        [ObservableProperty] private ObservableCollection<StaffMember> _filteredStaffList = new();
        [ObservableProperty] private ObservableCollection<StaffMember> _pagedStaffList = new();
        [ObservableProperty] private string _staffSearchText = string.Empty;
        [ObservableProperty] private StaffMember? _selectedStaff;
        
        // Document Management
        [ObservableProperty] private string _selectedDocumentTab = "BR";
        [ObservableProperty] private ObservableCollection<AppDocument> _allDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredDocuments = new();
        
        public override string Category => "EPF / ETF";

        public Action<AuditRecord>? NavigateToAddStaff { get; set; }
        public Action<AuditRecord, StaffMember>? NavigateToStaffDetail { get; set; }

        // Pagination
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _recordsPerPage = 10;
        [ObservableProperty] private int _totalPages = 1;

        public string PaginationDisplay => $"Page {CurrentPage} of {Math.Max(1, TotalPages)}";

        public string Branch => Record?.Branch ?? "Main";
        public int TotalStaff => Record?.NoOfStaffs ?? 0;

        public EPFETFDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            LoadFromRecord();
            _ = LoadFullRecordAsync();
        }

        protected override void OnRecordLoaded(AuditRecord? value)
        {
            LoadFromRecord();
            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(TotalStaff));
        }

        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

        private void LoadFromRecord()
        {
            if (Record?.StaffList != null)
            {
                StaffList = new ObservableCollection<StaffMember>(Record.StaffList);
            }
            else
            {
                StaffList = new ObservableCollection<StaffMember>();
            }
            UpdateFilteredStaff();
            
            AllDocuments.Clear();
            if (Record?.SourceDocuments != null)
            {
                foreach (var doc in Record.SourceDocuments)
                {
                    AllDocuments.Add(new AppDocument
                    {
                        FileName = doc.FileName,
                        FileSize = doc.FileSize > 0 ? $"{doc.FileSize / 1024.0:F1} KB" : "Source Document",
                        Category = SelectedDocumentTab,
                        Url = doc.Url
                    });
                }
            }
            UpdateFilteredDocuments();
        }

        private void UpdateFilteredStaff()
        {
            IEnumerable<StaffMember> filtered;
            if (string.IsNullOrWhiteSpace(StaffSearchText))
            {
                filtered = StaffList;
            }
            else
            {
                filtered = StaffList.Where(s => 
                    (s.StaffName?.Contains(StaffSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.StaffId?.Contains(StaffSearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            var filteredList = filtered.ToList();
            FilteredStaffList = new ObservableCollection<StaffMember>(filteredList);
            
            TotalPages = (int)Math.Ceiling((double)filteredList.Count / RecordsPerPage);
            if (CurrentPage > TotalPages) CurrentPage = Math.Max(1, TotalPages);
            
            UpdatePagedList();
        }

        private void UpdatePagedList()
        {
            var paged = FilteredStaffList.Skip((CurrentPage - 1) * RecordsPerPage).Take(RecordsPerPage).ToList();
            PagedStaffList = new ObservableCollection<StaffMember>(paged);
            OnPropertyChanged(nameof(PaginationDisplay));
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePagedList();
            }
        }

        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePagedList();
            }
        }

        partial void OnStaffSearchTextChanged(string value)
        {
            CurrentPage = 1;
            UpdateFilteredStaff();
        }

        protected override void InitializeSteps()
        {
            // EPF/ETF might not have a complex step process in the mockup, 
            // but we can add a simple one if needed. The mockup doesn't show steps.
            // SetupSteps(new List<(string Name, string? Icon)>());
        }

        [RelayCommand]
        private void AddStaff()
        {
            if (Record != null)
                NavigateToAddStaff?.Invoke(Record);
        }

        [RelayCommand]
        private void StaffSelected(StaffMember member)
        {
            if (Record != null && member != null)
                NavigateToStaffDetail?.Invoke(Record, member);
        }

        public override void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete the EPF/ETF record for '{CompanyName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await DataService.Instance.DeleteAuditRecordsAsync("EPF / ETF", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadFromRecord();
            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(TotalStaff));
        }

        [RelayCommand]
        private void SelectDocumentTab(string tabName)
        {
            if (SelectedDocumentTab != tabName)
            {
                SelectedDocumentTab = tabName;
                UpdateFilteredDocuments();
            }
        }

        private void UpdateFilteredDocuments()
        {
            var filtered = AllDocuments.Where(d => d.Category == SelectedDocumentTab).ToList();
            FilteredDocuments = new ObservableCollection<AppDocument>(filtered);
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadDocument()
        {
            if (RequestFilePicker == null) return;
            var localPaths = await RequestFilePicker.Invoke();
            if (localPaths == null || localPaths.Length == 0) return;

            var tempId = Record?.ID ?? Guid.NewGuid().ToString();
            var uploadedDocs = await ApiService.Instance.UploadDocumentsAsync(
                localPaths.ToList(),
                "EPF / ETF",
                tempId
            );

            if (uploadedDocs != null && uploadedDocs.Count > 0)
            {
                if (Record != null)
                {
                    Record.SourceDocuments ??= new List<SourceDocument>();
                    foreach (var doc in uploadedDocs)
                    {
                        Record.SourceDocuments.Add(doc);
                        AllDocuments.Add(new AppDocument
                        {
                            FileName = doc.FileName,
                            FileSize = doc.FileSize > 0 ? $"{doc.FileSize / 1024.0:F1} KB" : "Source Document",
                            Category = SelectedDocumentTab,
                            Url = doc.Url
                        });
                    }
                    await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", Record);
                }
                UpdateFilteredDocuments();
            }
        }

        [RelayCommand]
        private void PreviewDocument(AppDocument doc)
        {
            if (doc == null) return;
            var target = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.ImagePath;
            if (string.IsNullOrWhiteSpace(target)) return;

            string fullUrl = ApiService.GetFullDocumentUrl(target);
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to open document: {ex.Message}");
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task DownloadDocument(AppDocument doc)
        {
            if (doc == null) return;
            var target = !string.IsNullOrWhiteSpace(doc.Url) ? doc.Url : doc.ImagePath;
            if (string.IsNullOrWhiteSpace(target)) return;

            string fullUrl = ApiService.GetFullDocumentUrl(target);
            string fileName = !string.IsNullOrWhiteSpace(doc.FileName) ? doc.FileName : "epf_etf_document";

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

        [RelayCommand]
        private void EditDocument(AppDocument doc)
        {
        }

        [RelayCommand]
        private void DeleteDocument(AppDocument doc)
        {
            if (doc == null) return;
            ConfirmDialogTitle = "Delete Document?";
            ConfirmDialogMessage = $"Are you sure you want to delete '{doc.FileName}'?";
            ConfirmActionDelegate = async () =>
            {
                AllDocuments.Remove(doc);
                UpdateFilteredDocuments();

                if (Record != null && Record.SourceDocuments != null)
                {
                    var match = Record.SourceDocuments.FirstOrDefault(d => d.FileName == doc.FileName || d.Url == doc.Url);
                    if (match != null)
                    {
                        Record.SourceDocuments.Remove(match);
                    }
                    await DataService.Instance.UpdateAuditRecordAsync("EPF / ETF", Record);
                    NotificationService.Instance.AddNotification("Success", "Document deleted.");
                }
                IsConfirmDialogVisible = false;
            };
            IsConfirmDialogVisible = true;
        }
    }
}