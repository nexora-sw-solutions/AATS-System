using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class ImportExportDetailViewModel : DetailViewModelBase
    {
        public override string GuideTitle => "Guide: Clearance Details";
        public override string GuideDescription => "Manage the import/export clearance workflow from documentation to final approval.";
        public override string GuideProTip => "Double check the 'TIN' and 'Assignment' type before submitting the application to customs.";
        public override string Category => "Import/Export";

        public ImportExportDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            

        }

        protected override void InitializeSteps()
        {
            var stepDefinitions = new List<(string Name, string? Icon)>
            {
                ("Documentation", null),
                ("Application", null),
                ("Submission", null),
                ("Approval", null)
            };

            SetupSteps(stepDefinitions);
        }

        
        [ObservableProperty] private string _selectedPart1Tab = "TIN";
        [ObservableProperty] private string _selectedPart2Tab = "Utility";
        [ObservableProperty] private ObservableCollection<SourceDocument> _filteredPart1Documents = new();
        [ObservableProperty] private ObservableCollection<SourceDocument> _filteredPart2Documents = new();

        [RelayCommand]
        private void SelectPart1Tab(string tabName)
        {
            SelectedPart1Tab = tabName;
            UpdateFilteredPart1Documents();
        }

        [RelayCommand]
        private void SelectPart2Tab(string tabName)
        {
            SelectedPart2Tab = tabName;
            UpdateFilteredPart2Documents();
        }

        public void UpdateFilteredPart1Documents()
        {
            var filtered = SourceDocuments.Where(d => (d.Description ?? "TIN") == SelectedPart1Tab).ToList();
            FilteredPart1Documents.Clear();
            foreach (var doc in filtered)
            {
                FilteredPart1Documents.Add(doc);
            }
        }

        public void UpdateFilteredPart2Documents()
        {
            var filtered = SourceDocuments.Where(d => (d.Description ?? "Utility") == SelectedPart2Tab).ToList();
            FilteredPart2Documents.Clear();
            foreach (var doc in filtered)
            {
                FilteredPart2Documents.Add(doc);
            }
        }

        protected override void OnRecordLoaded(AuditRecord? value)
        {
            base.OnRecordLoaded(value);
            UpdateFilteredProcessDocuments();
            UpdateFilteredProcessCardDocuments();
            UpdateFilteredPart1Documents();
            UpdateFilteredPart2Documents();
        }

        
        // Document Management Collections
        [ObservableProperty] private ObservableCollection<AppDocument> _allProcessDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();
        [ObservableProperty] private string _selectedProcessDocumentsCardTab = "Submission";
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessCardDocuments = new();

        // Stage Visibility Flags
        public bool IsSubmissionUploadVisible => Record?.CurrentStep == 3;
        public bool IsApprovalUploadVisible => Record?.CurrentStep == 4;

        public System.Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

        public void UpdateFilteredProcessDocuments()
        {
            OnPropertyChanged(nameof(IsSubmissionUploadVisible));
            OnPropertyChanged(nameof(IsApprovalUploadVisible));

            string category = "Submission";
            if (Record?.CurrentStep == 4) category = "Approval";
            
            var filtered = AllProcessDocuments.Where(d => d.Category == category).ToList();
            FilteredProcessDocuments.Clear();
            foreach (var doc in filtered)
            {
                FilteredProcessDocuments.Add(doc);
            }
        }

        [RelayCommand]
        private void SelectProcessDocumentsCardTab(string tabName)
        {
            SelectedProcessDocumentsCardTab = tabName;
            UpdateFilteredProcessCardDocuments();
        }

        public void UpdateFilteredProcessCardDocuments()
        {
            var filtered = AllProcessDocuments.Where(d => d.Category == SelectedProcessDocumentsCardTab).ToList();
            FilteredProcessCardDocuments.Clear();
            foreach (var doc in filtered)
            {
                FilteredProcessCardDocuments.Add(doc);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadProcessDocument(string documentType)
        {
            if (RequestFilePicker == null) return;
            var paths = await RequestFilePicker();
            if (paths == null || paths.Length == 0) return;

            string category = "Submission";
            if (Record?.CurrentStep == 4) category = "Approval";

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
            UpdateFilteredProcessCardDocuments();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ReplaceProcessDocument(AppDocument doc)
        {
            if (RequestFilePicker == null) return;
            var paths = await RequestFilePicker();
            if (paths == null || paths.Length == 0) return;

            doc.FileName = System.IO.Path.GetFileName(paths[0]);
            doc.ImagePath = paths[0];
            doc.FileSize = (new System.IO.FileInfo(paths[0]).Length / 1024).ToString() + " KB";
            doc.IsExisting = false;
            
            UpdateFilteredProcessDocuments();
            UpdateFilteredProcessCardDocuments();
        }

        [RelayCommand]
        private void RemoveProcessDocument(AppDocument doc)
        {
            var count = AllProcessDocuments.Count(d => d.Category == doc.Category);
            if (count <= 1)
            {
                ConfirmDialogTitle = "Cannot Delete Last Document";
                ConfirmDialogMessage = $"You must have at least one document for the {doc.Category} stage. Please upload a replacement before deleting this one.";
                ConfirmActionDelegate = () => System.Threading.Tasks.Task.CompletedTask;
                IsConfirmDialogVisible = true;
                return;
            }

            ConfirmDialogTitle = "Delete Document?";
            ConfirmDialogMessage = $"Are you sure you want to delete '{doc.FileName}'? This action cannot be undone.";
            ConfirmActionDelegate = () =>
            {
                AllProcessDocuments.Remove(doc);
                UpdateFilteredProcessDocuments();
                UpdateFilteredProcessCardDocuments();
                return System.Threading.Tasks.Task.CompletedTask;
            };
            IsConfirmDialogVisible = true;
        }

        protected override void UpdateStepStates()
        {
            base.UpdateStepStates();
            UpdateFilteredProcessDocuments();
        }

        public override void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Import/Export Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete the clearance record for '{CompanyName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await DataService.Instance.DeleteAuditRecordsAsync("Import/Export", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }
    }
}