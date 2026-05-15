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
        }

        protected override void OnRecordLoaded(AuditRecord? value)
        {
            LoadFromRecord();
            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(TotalStaff));
        }

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
    }
}