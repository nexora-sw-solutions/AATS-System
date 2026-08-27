using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class TradeMarkDetailViewModel : DetailViewModelBase
    {
        public TradeMarkDetailViewModel(AuditRecord record) : base(record)
        {
            InitializeSteps();
            UpdateFilteredProcessDocuments();
            UpdateFilteredProcessCardDocuments();
        }

        public override string GuideTitle => "Guide: Trade Mark Details";
        public override string GuideDescription => "Monitor trademark registration, search, and intellectual property protection statuses.";
        public override string GuideProTip => "Review the 'Class Specification' document to ensure the trademark covers all relevant service categories.";
        public override string Category => "Trade Marks";

        // Document Management Collections
        [ObservableProperty] private ObservableCollection<AppDocument> _allProcessDocuments = new();
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessDocuments = new();

        [ObservableProperty] private string _selectedProcessDocumentsCardTab = "Temporary Certificate";
        [ObservableProperty] private ObservableCollection<AppDocument> _filteredProcessCardDocuments = new();


        // Stage Visibility Flags
        public bool IsTempCertificateUploadVisible => Record?.CurrentStep == 1;
        public bool IsLetterUploadVisible => Record?.CurrentStep == 2;
        public bool IsCertificateUploadVisible => Record?.CurrentStep == 3;

        public Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

        protected override void InitializeSteps()
        {
            var stepDefinitions = new List<(string Name, string? Icon)>
            {
                ("Temporary Certificate", null),
                ("Letter", null),
                ("Certificate", "fa-solid fa-check")
            };

            SetupSteps(stepDefinitions);
        }

        public override void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Trade Mark Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete the Trade Mark record for '{CompanyName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (Record != null)
                {
                    await Services.DataService.Instance.DeleteAuditRecordsAsync("Trade Marks", new[] { Record });
                    NavigateBack?.Invoke();
                }
            };
            IsConfirmDialogVisible = true;
        }

        protected override void UpdateStepStates()
        {
            base.UpdateStepStates();
            UpdateFilteredProcessDocuments();
            UpdateFilteredProcessCardDocuments();
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

        public void UpdateFilteredProcessDocuments()
        {
            OnPropertyChanged(nameof(IsTempCertificateUploadVisible));
            OnPropertyChanged(nameof(IsLetterUploadVisible));
            OnPropertyChanged(nameof(IsCertificateUploadVisible));

            string category = "Temporary Certificate";
            if (Record?.CurrentStep == 2) category = "Letter";
            else if (Record?.CurrentStep == 3) category = "Certificate";

            var filtered = AllProcessDocuments.Where(d => d.Category == category).ToList();
            FilteredProcessDocuments.Clear();
            foreach (var doc in filtered)
            {
                FilteredProcessDocuments.Add(doc);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task UploadProcessDocument(string documentType)
        {
            if (RequestFilePicker == null) return;
            var paths = await RequestFilePicker();
            if (paths == null || paths.Length == 0) return;

            string category = "Temporary Certificate";
            if (Record?.CurrentStep == 2) category = "Letter";
            else if (Record?.CurrentStep == 3) category = "Certificate";

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
                    UpdateFilteredProcessCardDocuments();
                    await System.Threading.Tasks.Task.CompletedTask;
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

        [RelayCommand]
        private void PreviewProcessDocument(AppDocument doc)
        {
            if (doc != null && !string.IsNullOrWhiteSpace(doc.ImagePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.ImagePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Error previewing document: {ex.Message}");
                }
            }
        }
    }
}