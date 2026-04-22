using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;

namespace AATS.Desktop.ViewModels
{
    public class ProcessStep : ObservableObject
    {
        private int _number;
        private string _name = string.Empty;
        private bool _isActive;
        private bool _isClickable;
        private bool _isIconStep;
        private string _iconValue = string.Empty;

        private bool _isLast;
 
        public int Number { get => _number; set => SetProperty(ref _number, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        public bool IsClickable { get => _isClickable; set => SetProperty(ref _isClickable, value); }
        public bool IsIconStep { get => _isIconStep; set => SetProperty(ref _isIconStep, value); }
        public string IconValue { get => _iconValue; set => SetProperty(ref _iconValue, value); }
        public bool IsLast { get => _isLast; set => SetProperty(ref _isLast, value); }
    }

    public abstract partial class DetailViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        private AuditRecord? _record;

        [ObservableProperty]
        private ObservableCollection<ProcessStep> _steps = new();

        [ObservableProperty]
        private bool _isConfirmDialogVisible;

        [ObservableProperty]
        private string _confirmDialogTitle = string.Empty;

        [ObservableProperty]
        private string _confirmDialogMessage = string.Empty;

        [ObservableProperty]
        private bool _isGuideVisible;

        protected Func<System.Threading.Tasks.Task>? ConfirmActionDelegate;
        private int _pendingStep;

        public Action? NavigateBack { get; set; }
        public Action<AuditRecord>? NavigateToEditRecord { get; set; }
        public Func<System.Threading.Tasks.Task>? DeleteRecordAction { get; set; }

        public virtual string GuideTitle => "Client Details Guide";
        public virtual string GuideDescription => "Review the overall progress and specific information for this client record.";
        public virtual string GuideProTip => "Use the process timeline to update the status of the clearance or registration.";

        public DetailViewModelBase(AuditRecord record)
        {
            Record = record;
        }

        // Common record properties
        public string ClientName => Record?.ClientName ?? "N/A";
        public string CompanyName => Record?.Company ?? "N/A";
        public string ID => Record?.ID ?? "N/A";
        public string IDDisplay => Record?.ID ?? "N/A";
        public string Date => Record?.Date.ToString("yyyy-MM-dd") ?? "N/A";
        public string DateDisplay => Record?.Date.ToString("dd/MM/yyyy") ?? "N/A";
        public string BranchDisplay => Record?.Branch ?? "N/A";
        public string StatusDisplay => Record?.PaymentStatus ?? "N/A";
        public string AssignmentDisplay => Record?.Assignment ?? "N/A";
        public string PaymentOption => Record?.PaymentOption ?? "N/A";
        public string PaymentStatus => Record?.PaymentStatus ?? "N/A";
        public string Assignment => Record?.Assignment ?? "N/A";
        public string Period => Record?.Period ?? "N/A";
        public ObservableCollection<SourceDocument> SourceDocuments => new(Record?.SourceDocuments ?? new List<SourceDocument>());

        protected abstract void InitializeSteps();

        protected void SetupSteps(IEnumerable<(string Name, string? Icon)> stepDefs)
        {
            Steps.Clear();
            int count = 1;
            foreach (var (name, icon) in stepDefs)
            {
                Steps.Add(new ProcessStep
                {
                    Number = count,
                    Name = name,
                    IsIconStep = !string.IsNullOrEmpty(icon),
                    IconValue = icon ?? string.Empty
                });
                count++;
            }
            UpdateStepStates();
            
            // Mark the last step for UI line visibility
            if (Steps.Count > 0)
            {
                Steps[Steps.Count - 1].IsLast = true;
            }
        }

        protected virtual void UpdateStepStates()
        {
            if (Record == null) return;

            for (int i = 0; i < Steps.Count; i++)
            {
                int stepNum = i + 1;
                Steps[i].IsActive = stepNum <= Record.CurrentStep;
                
                // Clickable logic: 
                // 1. Next step (Forward Sequential)
                // 2. Any previous step (Backward correction)
                Steps[i].IsClickable = (stepNum == Record.CurrentStep + 1) || (stepNum < Record.CurrentStep);
            }
        }

        [RelayCommand]
        public void StepClick(ProcessStep step)
        {
            if (!step.IsClickable) return;

            _pendingStep = step.Number;
            ConfirmDialogTitle = step.Number > Record!.CurrentStep ? "Advance Process?" : "Move Back Process?";
            ConfirmDialogMessage = $"Are you sure you want to change the status to '{step.Name}'?";
            ConfirmActionDelegate = () =>
            {
                ApplyStepChange();
                return System.Threading.Tasks.Task.CompletedTask;
            };
            IsConfirmDialogVisible = true;
        }

        private void ApplyStepChange()
        {
            if (Record != null)
            {
                Record.CurrentStep = _pendingStep;
                // You could also sync Record.Process here if needed
                UpdateStepStates();
                Refresh();
            }
        }

        [RelayCommand]
        public virtual async System.Threading.Tasks.Task ConfirmAction()
        {
            IsConfirmDialogVisible = false;
            if (ConfirmActionDelegate != null)
            {
                await ConfirmActionDelegate.Invoke();
            }
            ConfirmActionDelegate = null;
        }

        [RelayCommand]
        public virtual void CancelAction()
        {
            IsConfirmDialogVisible = false;
            ConfirmActionDelegate = null;
            _pendingStep = 0;
        }

        [RelayCommand] public void EditRecord() => OnEditRecord();
        [RelayCommand] public void DeleteRecord() => OnDeleteRecord();
        [RelayCommand] public void DownloadRecord() => OnDownloadRecord();
        [RelayCommand] public void PrintRecord() => OnPrintRecord();

        public virtual void OnEditRecord()
        {
            if (Record != null)
                NavigateToEditRecord?.Invoke(Record);
        }

        public virtual void OnDeleteRecord()
        {
            ConfirmDialogTitle = "Delete Record?";
            ConfirmDialogMessage = $"Are you sure you want to delete record '{ID}' for '{ClientName}'? This action cannot be undone.";
            ConfirmActionDelegate = async () =>
            {
                if (DeleteRecordAction != null)
                {
                    await DeleteRecordAction.Invoke();
                }
                NavigateBack?.Invoke();
            };
            IsConfirmDialogVisible = true;
        }

        protected virtual void OnDownloadRecord()
        {
            // Placeholder simulation
            ConfirmDialogTitle = "Download Interface";
            ConfirmDialogMessage = "Simulating PDF generation and download for this client record...";
            ConfirmActionDelegate = null; // Just show message
            IsConfirmDialogVisible = true;
        }

        protected virtual void OnPrintRecord()
        {
            // Placeholder simulation
            ConfirmDialogTitle = "Print Interface";
            ConfirmDialogMessage = "Opening system print dialog simulation for the client report...";
            ConfirmActionDelegate = null; // Just show message
            IsConfirmDialogVisible = true;
        }

        [RelayCommand]
        private void OpenGuide() => IsGuideVisible = true;

        [RelayCommand]
        private void CloseGuide() => IsGuideVisible = false;

        public virtual void Refresh()
        {
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(ClientName));
            OnPropertyChanged(nameof(Assignment));
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(PaymentOption));
            OnPropertyChanged(nameof(PaymentStatus));
            OnPropertyChanged(nameof(Period));
            OnPropertyChanged(nameof(SourceDocuments));
        }
    }
}
