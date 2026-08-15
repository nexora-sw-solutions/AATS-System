using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels.AuditAndAccounts
{
    public partial class AuditAssuranceDetailViewModel : DetailViewModelBase
    {
        public AuditAssuranceDetailViewModel(AuditRecord record) : base(record)
        {
            record.Type = "Audit & Assurance";
            InitializeSteps();
            
            FilterPart1Files();
            FilterPart2Files();
            
            if (Record?.SourceDocuments != null)
            {
                var processTabs = new[] { "Bookkeep", "Draft", "Finalize", "Return" };
                foreach (var doc in Record.SourceDocuments)
                {
                    if (doc.Description != null && System.Linq.Enumerable.Contains(processTabs, doc.Description))
                    {
                        _allProcessDocuments.Add(new AppDocument
                        {
                            FileName = doc.FileName ?? string.Empty,
                            FileSize = "Unknown",
                            Category = doc.Description,
                            Type = doc.Description,
                            ImagePath = doc.Url ?? string.Empty,
                            IsExisting = true
                        });
                    }
                }
            }
            FilterProcessFiles();

        }

        protected override void InitializeSteps()
        {
            var stepDefinitions = new List<(string Name, string? Icon)>
            {
                ("Bookkeep", null),
                ("Draft Account", null),
                ("Finalize", null),
                ("Handover", null),
                ("Submit", "fa-solid fa-check"),
                ("Return", "fa-solid fa-circle-info")
            };

            SetupSteps(stepDefinitions);
        }

        public System.Func<System.Threading.Tasks.Task<string[]?>>? RequestFilePicker { get; set; }

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _selectedProcessTab = "Bookkeep";

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<AppDocument> _filteredProcessDocuments = new();

        private System.Collections.ObjectModel.ObservableCollection<AppDocument> _allProcessDocuments = new();

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void SelectProcessTab(string tabName)
        {
            SelectedProcessTab = tabName;
            FilterProcessFiles();
        }

        private void FilterProcessFiles()
        {
            var filtered = System.Linq.Enumerable.Where(_allProcessDocuments, d => d.Category == SelectedProcessTab);
            FilteredProcessDocuments = new System.Collections.ObjectModel.ObservableCollection<AppDocument>(filtered);
        }

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isGlobalEditVisible;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private AppDocument? _editingDocument;

        private AppDocument? _originalEditingSource;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _selectedPart1Tab = "BR";
        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<AppDocument> _filteredPart1Files = new();

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _selectedPart2Tab = "Bank Statement";
        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<AppDocument> _filteredPart2Files = new();

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void SelectPart1Tab(string tabName)
        {
            SelectedPart1Tab = tabName;
            FilterPart1Files();
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void SelectPart2Tab(string tabName)
        {
            SelectedPart2Tab = tabName;
            FilterPart2Files();
        }

        private void FilterPart1Files()
        {
            if (Record?.SourceDocuments != null)
            {
                var filtered = System.Linq.Enumerable.Where(Record.SourceDocuments, d => d.Description == SelectedPart1Tab);
                var appDocs = System.Linq.Enumerable.Select(filtered, d => new AppDocument
                {
                    FileName = d.FileName ?? string.Empty,
                    FileSize = "Unknown",
                    Category = d.Description ?? string.Empty,
                    ImagePath = d.Url ?? string.Empty,
                    IsExisting = true
                });
                FilteredPart1Files = new System.Collections.ObjectModel.ObservableCollection<AppDocument>(appDocs);
            }
        }

        private void FilterPart2Files()
        {
            if (Record?.SourceDocuments != null)
            {
                var filtered = System.Linq.Enumerable.Where(Record.SourceDocuments, d => d.Description == SelectedPart2Tab);
                var appDocs = System.Linq.Enumerable.Select(filtered, d => new AppDocument
                {
                    FileName = d.FileName ?? string.Empty,
                    FileSize = "Unknown",
                    Category = d.Description ?? string.Empty,
                    ImagePath = d.Url ?? string.Empty,
                    IsExisting = true
                });
                FilteredPart2Files = new System.Collections.ObjectModel.ObservableCollection<AppDocument>(appDocs);
            }
        }

        public bool IsBookkeepUploadVisible => Record?.CurrentStep == 0;
        public bool IsDraftUploadVisible => Record?.CurrentStep == 1;
        public bool IsFinalizeUploadVisible => Record?.CurrentStep == 2;
        public bool IsReturnUploadVisible => Record?.CurrentStep == 5;

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async System.Threading.Tasks.Task UploadProcessDocument(string stage)
        {
            if (RequestFilePicker == null) return;
            var paths = await RequestFilePicker();
            if (paths == null || paths.Length == 0) return;

            foreach (var path in paths)
            {
                var doc = new AppDocument
                {
                    FileName = System.IO.Path.GetFileName(path),
                    ImagePath = path,
                    Category = SelectedProcessTab,
                    Type = SelectedProcessTab,
                    FileSize = (new System.IO.FileInfo(path).Length / 1024).ToString() + " KB",
                    IsExisting = false
                };
                _allProcessDocuments.Add(doc);
            }
            FilterProcessFiles();
            SyncToRecordSourceDocuments();
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void RemoveProcessDocument(AppDocument doc)
        {
            if (doc != null)
            {
                var count = System.Linq.Enumerable.Count(_allProcessDocuments, d => d.Category == doc.Category);
                if (count <= 1)
                {
                    ShowConfirmDialog("Cannot delete the final remaining document in this tab.", () => System.Threading.Tasks.Task.CompletedTask);
                    return;
                }
                
                ShowConfirmDialog($"Are you sure you want to delete {doc.FileName}?", () =>
                {
                    _allProcessDocuments.Remove(doc);
                    FilterProcessFiles();
                    SyncToRecordSourceDocuments();
                    return System.Threading.Tasks.Task.CompletedTask;
                });
            }
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void EditProcessDocument(AppDocument doc)
        {
            if (doc != null)
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

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void SaveGlobalEdit()
        {
            if (EditingDocument != null && _originalEditingSource != null)
            {
                _originalEditingSource.FileName = EditingDocument.FileName;
                
                FilterProcessFiles();
                SyncToRecordSourceDocuments();
            }
            IsGlobalEditVisible = false;
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void CloseGlobalEdit()
        {
            IsGlobalEditVisible = false;
            EditingDocument = null;
            _originalEditingSource = null;
        }

        private void SyncToRecordSourceDocuments()
        {
            if (Record?.SourceDocuments != null)
            {
                var processTabs = new[] { "Bookkeep", "Draft", "Finalize", "Return" };
                
                // Remove all existing process documents from the record
                var toRemove = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(Record.SourceDocuments, d => d.Description != null && System.Linq.Enumerable.Contains(processTabs, d.Description)));
                foreach (var doc in toRemove) Record.SourceDocuments.Remove(doc);

                // Add current ones
                foreach (var doc in _allProcessDocuments)
                {
                    Record.SourceDocuments.Add(new SourceDocument
                    {
                        FileName = doc.FileName,
                        Url = doc.ImagePath,
                        Description = doc.Category
                    });
                }
            }
        }

        private void ShowConfirmDialog(string message, System.Func<System.Threading.Tasks.Task> confirmAction)
        {
             ConfirmDialogTitle = "Confirmation";
             ConfirmDialogMessage = message;
             ConfirmActionDelegate = confirmAction;
             IsConfirmDialogVisible = true;
        }

        protected override void UpdateStepStates()
        {
            base.UpdateStepStates();
            OnPropertyChanged(nameof(IsBookkeepUploadVisible));
            OnPropertyChanged(nameof(IsDraftUploadVisible));
            OnPropertyChanged(nameof(IsFinalizeUploadVisible));
            OnPropertyChanged(nameof(IsReturnUploadVisible));
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async System.Threading.Tasks.Task PreviewDocument(object parameter)
        {
            if (parameter is AppDocument doc && !string.IsNullOrEmpty(doc.ImagePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(doc.ImagePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to open document: {ex.Message}");
                }
            }
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public async System.Threading.Tasks.Task DownloadDocument(AppDocument doc)
        {
            if (doc != null && !string.IsNullOrEmpty(doc.ImagePath))
            {
                try
                {
                    string target = doc.ImagePath;
                    if (target.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) || 
                        target.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var fileName = doc.FileName ?? "downloaded_file";
                        await AATS.Desktop.Services.ApiService.Instance.DownloadDocumentAsync(target, fileName);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to download document: {ex.Message}");
                }
            }
        }

        public override string Category => "Audit & Assurance";
    }
}
