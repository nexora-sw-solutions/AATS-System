using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace AATS.Desktop.ViewModels.TaxFiling
{
    public partial class SSCLDetailViewModel : ViewModelBase
    {
        [ObservableProperty] private TaxRecord _record;
        [ObservableProperty] private bool _isEditMode;

        // Display Properties
        public string Code => Record?.Code ?? "N/A";
        public string PaymentStatus => Record?.Status ?? "N/A";
        public string DisplayClientId => Record?.ClientCode ?? "N/A";
        public string DisplayClientName => Record?.ClientName ?? "N/A";
        public string DisplaySsclNo => Record?.TIN ?? "N/A";
        public string DisplayStatus => Record?.Status ?? "N/A";
        public string DisplayAssessmentYear => "2024/2025";
        public string DisplayCurrentPeriod => Record?.TaxPeriod ?? "N/A";
        public string DisplayAuditorNotes => Record?.Notes ?? "No notes available.";

        public string ClientCategory => Record?.ClientCategory ?? string.Empty;
        public string ClientCategoryColor => Record?.ClientCategoryColor ?? "Transparent";
        public bool HasClientCategory => Record?.HasClientCategory ?? false;

        // Edit Properties
        [ObservableProperty] private string _editClientId = string.Empty;
        [ObservableProperty] private string _editClientName = string.Empty;
        [ObservableProperty] private string _editSsclNo = string.Empty;
        [ObservableProperty] private string _editStatus = string.Empty;
        [ObservableProperty] private string _editAssessmentYear = string.Empty;
        [ObservableProperty] private string _editCurrentPeriod = string.Empty;
        [ObservableProperty] private string _editAuditorNotes = string.Empty;

        public ObservableCollection<string> DrawerStatusFilters { get; } = new() { "Paid", "Pending", "IRD pending", "IRD Paid" };

        [ObservableProperty] private ObservableCollection<string> _previewDocuments = new();
        [ObservableProperty] private bool _isDeleteConfirmVisible;

        public Action? GoBack { get; set; }

        public SSCLDetailViewModel(TaxRecord record)
        {
            Record = record;
            if (record != null)
            {
                if (record.SourceDocuments != null)
                {
                    foreach (var doc in record.SourceDocuments)
                    {
                        if (!string.IsNullOrEmpty(doc.Url)) PreviewDocuments.Add(doc.Url);
                        else if (!string.IsNullOrEmpty(doc.FileName)) PreviewDocuments.Add(doc.FileName);
                    }
                }
                
                // Fallback for demo records
                if (PreviewDocuments.Count == 0 && record.Code != null)
                {
                    PreviewDocuments.Add($"Payment_Slip_{record.Code ?? "1"}.pdf");
                }
            }
        }

        [RelayCommand]
        private void PreviewDocument(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error previewing document: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void DownloadReport()
        {
            try {
                RecordReportService.Instance.DownloadReportAsync(Record, "SSCL", MainViewModel.Instance?.CurrentUser?.Username ?? "System");
            } catch (Exception ex) {
                NotificationService.Instance.AddNotification("Error", $"Could not download report: {ex.Message}");
            }
        }

        [RelayCommand]
        private void PrintReport()
        {
            try {
                RecordReportService.Instance.PrintReportAsync(Record, "SSCL", MainViewModel.Instance?.CurrentUser?.Username ?? "System");
            } catch (Exception ex) {
                NotificationService.Instance.AddNotification("Error", $"Could not print report: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EnterEditMode()
        {
            if (MainViewModel.Instance != null)
            {
                MainViewModel.Instance.NavigateToSSCLEditRecord(Record);
            }
        }

        [RelayCommand]
        private void Submit()
        {
            Record.ClientCode = EditClientId;
            Record.ClientName = EditClientName;
            Record.TIN = EditSsclNo;
            Record.Status = EditStatus;
            Record.TaxPeriod = EditCurrentPeriod;
            Record.Notes = EditAuditorNotes;

            OnPropertyChanged(nameof(DisplayClientId));
            OnPropertyChanged(nameof(DisplayClientName));
            OnPropertyChanged(nameof(DisplaySsclNo));
            OnPropertyChanged(nameof(DisplayStatus));
            OnPropertyChanged(nameof(DisplayCurrentPeriod));
            OnPropertyChanged(nameof(DisplayAuditorNotes));
            
            IsEditMode = false;
        }

        [RelayCommand]
        private void Clear()
        {
            EditClientId = string.Empty;
            EditClientName = string.Empty;
            EditSsclNo = string.Empty;
            EditStatus = "Pending";
            EditAssessmentYear = string.Empty;
            EditCurrentPeriod = string.Empty;
            EditAuditorNotes = string.Empty;
        }

        [RelayCommand]
        private void RequestDeleteRecord()
        {
            IsDeleteConfirmVisible = true;
        }

        [RelayCommand]
        private void CancelDelete()
        {
            IsDeleteConfirmVisible = false;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            IsDeleteConfirmVisible = false;
            GoBack?.Invoke();
        }

        [ObservableProperty] private bool _isGuideVisible;

        [RelayCommand]
        private void OpenGuide()
        {
            IsGuideVisible = true;
        }

        [RelayCommand]
        private void CloseGuide()
        {
            IsGuideVisible = false;
        }
    }
}

