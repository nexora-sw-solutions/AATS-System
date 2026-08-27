using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels
{
    public partial class OutstandingBalancesViewModel : ViewModelBase
    {
        private bool _isInitializing;
        private OutstandingBalanceRecord? _previousRecord;
        private List<OutstandingBalanceRecord> _allRecords = new();

        [ObservableProperty] private ObservableCollection<OutstandingBalanceRecord> _filteredRecords = new();
        [ObservableProperty] private string _searchText = string.Empty;

        // Filters
        public ObservableCollection<string> ClientFilters { get; } = new() { "All Clients" };
        public ObservableCollection<string> StatusFilters { get; } = new() { "All Statuses", "Partial", "Unpaid", "Pending Cheque", "Bounced Cheque" };
        public ObservableCollection<string> ServiceFilters { get; } = new() { "All Services", "Audit & Assurance", "Company Registration", "Forensic Audit", "SSCL (Tax Filing)" };

        [ObservableProperty] private string _selectedClientFilter = "All Clients";
        [ObservableProperty] private string _selectedStatusFilter = "All Statuses";
        [ObservableProperty] private string _selectedServiceFilter = "All Services";

        // Date Filters
        public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string _selectedDateFilter = "All Dates";
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _specificDate;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _monthDate = DateTime.Now;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedYear;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _periodStartDate;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _periodEndDate;

        public ObservableCollection<string> Years { get; } = new() { "2024", "2025", "2026", "2027", "2028" };

        public string DateFilterSummary
        {
            get
            {
                if (SelectedDateFilter == "All Dates") return "All Time";
                if (SelectedDateFilter == "Today") return "Today";
                if (SelectedDateFilter == "This Week") return "This Week";
                if (SelectedDateFilter == "This Month") return MonthDate?.ToString("MMMM yyyy") ?? "This Month";
                if (SelectedDateFilter == "This Year") return SelectedYear ?? "This Year";
                if (SelectedDateFilter == "Specific Date") return SpecificDate?.ToString("dd MMM yyyy") ?? "Pick Date";
                if (SelectedDateFilter == "Specific Period")
                {
                    if (PeriodStartDate.HasValue && PeriodEndDate.HasValue)
                        return $"{PeriodStartDate:dd MMM} - {PeriodEndDate:dd MMM}";
                    return "Pick Range";
                }
                return SelectedDateFilter;
            }
        }

        public bool HasActiveFilters =>
            SelectedDateFilter != "All Dates" ||
            SelectedClientFilter != "All Clients" ||
            SelectedStatusFilter != "All Statuses" ||
            SelectedServiceFilter != "All Services" ||
            !string.IsNullOrWhiteSpace(SearchText);

        // Pagination State (matches standard application pattern)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PaginationDisplay))]
        [NotifyCanExecuteChangedFor(nameof(PrevPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PaginationDisplay))]
        [NotifyCanExecuteChangedFor(nameof(PrevPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int _totalPages = 1;

        public int PageSize { get; } = 10;
        public string PaginationDisplay => $"Page {CurrentPage} of {Math.Max(1, TotalPages)}";
        public bool CanGoToPrevPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        private List<OutstandingBalanceRecord> _filteredSource = new();

        // Summary Statistics (Calculated dynamically from loaded records)
        public decimal TotalOutstanding => _allRecords.Sum(r => r.OutstandingAmount);
        public decimal TotalPendingCheques => _allRecords.Where(r => r.PaymentStatus == "Pending Cheque").Sum(r => r.OutstandingAmount);
        public decimal TotalPartialPayments => _allRecords.Where(r => r.PaymentStatus == "Partial").Sum(r => r.AmountPaid);
        public decimal TotalOverdueAmount => _allRecords.Sum(r => r.OutstandingAmount);
        public int OutstandingClientsCount => _allRecords.Select(r => r.ClientId).Distinct().Count();

        // Selected Record Details for Sliding Drawer
        [ObservableProperty] private OutstandingBalanceRecord? _selectedRecord;
        [ObservableProperty] private bool _isDrawerOpen;

        public OutstandingBalancesViewModel()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _isInitializing = true;

                SelectedClientFilter = "All Clients";
                SelectedStatusFilter = "All Statuses";
                SelectedServiceFilter = "All Services";
                SelectedDateFilter = "All Dates";
                SearchText = string.Empty;

                var records = await DataService.Instance.GetOutstandingBalancesAsync();
                _allRecords = records ?? new List<OutstandingBalanceRecord>();

                // Build client filters from loaded records
                ClientFilters.Clear();
                ClientFilters.Add("All Clients");
                foreach (var name in _allRecords.Select(r => r.ClientName).Distinct().OrderBy(n => n))
                {
                    if (!string.IsNullOrEmpty(name))
                        ClientFilters.Add(name);
                }

                SelectedClientFilter = "All Clients";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading outstanding balances data: {ex.Message}");
            }
            finally
            {
                _isInitializing = false;
                ApplyFilter();
                NotifyStats();
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            _isInitializing = true;
            SelectedDateFilter = "All Dates";
            SelectedClientFilter = "All Clients";
            SelectedStatusFilter = "All Statuses";
            SelectedServiceFilter = "All Services";
            SearchText = string.Empty;
            SpecificDate = null;
            MonthDate = DateTime.Now;
            SelectedYear = null;
            PeriodStartDate = null;
            PeriodEndDate = null;
            _isInitializing = false;
            ApplyFilter();
        }

        [RelayCommand]
        private void CloseDrawer()
        {
            IsDrawerOpen = false;
            SelectedRecord = null;
        }

        [RelayCommand]
        private void OpenRecordDetail(OutstandingBalanceRecord record)
        {
            SelectedRecord = record;
        }

        partial void OnSelectedRecordChanged(OutstandingBalanceRecord? value)
        {
            if (_previousRecord != null)
            {
                _previousRecord.IsSelected = false;
            }

            if (value != null)
            {
                value.IsSelected = true;
                IsDrawerOpen = true;
            }

            _previousRecord = value;
        }

        // Trigger filters when properties change
        partial void OnSearchTextChanged(string value) => TryApplyFilter();
        partial void OnSelectedClientFilterChanged(string value)
        {
            if (string.IsNullOrEmpty(value) && !_isInitializing)
            {
                SelectedClientFilter = "All Clients";
                return;
            }
            TryApplyFilter();
        }
        partial void OnSelectedStatusFilterChanged(string value) => TryApplyFilter();
        partial void OnSelectedServiceFilterChanged(string value) => TryApplyFilter();
        partial void OnSelectedDateFilterChanged(string value) => TryApplyFilter();
        partial void OnSpecificDateChanged(DateTime? value) => TryApplyFilter();
        partial void OnMonthDateChanged(DateTime? value) => TryApplyFilter();
        partial void OnSelectedYearChanged(string? value) => TryApplyFilter();
        partial void OnPeriodStartDateChanged(DateTime? value) => TryApplyFilter();
        partial void OnPeriodEndDateChanged(DateTime? value) => TryApplyFilter();

        private void TryApplyFilter()
        {
            if (!_isInitializing)
                ApplyFilter();
        }

        private void ApplyFilter()
        {
            var results = _allRecords.AsEnumerable();

            // Date filtering
            if (SelectedDateFilter != "All Dates")
            {
                var now = DateTime.Now;
                if (SelectedDateFilter == "Today")
                    results = results.Where(r => r.DueDate.Date == now.Date);
                else if (SelectedDateFilter == "This Week")
                {
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    results = results.Where(r => r.DueDate.Date >= startOfWeek.Date);
                }
                else if (SelectedDateFilter == "This Month" && MonthDate.HasValue)
                    results = results.Where(r => r.DueDate.Month == MonthDate.Value.Month && r.DueDate.Year == MonthDate.Value.Year);
                else if (SelectedDateFilter == "This Year" && !string.IsNullOrEmpty(SelectedYear))
                {
                    if (int.TryParse(SelectedYear, out int year))
                        results = results.Where(r => r.DueDate.Year == year);
                }
                else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
                    results = results.Where(r => r.DueDate.Date == SpecificDate.Value.Date);
                else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
                    results = results.Where(r => r.DueDate.Date >= PeriodStartDate.Value.Date && r.DueDate.Date <= PeriodEndDate.Value.Date);
            }

            // Client filter
            if (!string.IsNullOrEmpty(SelectedClientFilter) && SelectedClientFilter != "All Clients")
                results = results.Where(r => r.ClientName == SelectedClientFilter);

            // Status filter
            if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "All Statuses")
                results = results.Where(r => string.Equals(r.PaymentStatus, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));

            // Service filter
            if (!string.IsNullOrEmpty(SelectedServiceFilter) && SelectedServiceFilter != "All Services")
                results = results.Where(r => r.ServiceModule == SelectedServiceFilter);

            // Search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                results = results.Where(r =>
                    (r.ClientId?.ToLower().Contains(lower) ?? false) ||
                    (r.ClientName?.ToLower().Contains(lower) ?? false) ||
                    (r.ServiceModule?.ToLower().Contains(lower) ?? false) ||
                    (r.InvoiceNumber?.ToLower().Contains(lower) ?? false) ||
                    (r.PaymentStatus?.ToLower().Contains(lower) ?? false)
                );
            }

            _filteredSource = results.ToList();
            CurrentPage = 1;
            UpdatePagination();
            OnPropertyChanged(nameof(HasActiveFilters));
        }

        private void UpdatePagination()
        {
            TotalPages = (int)Math.Ceiling(_filteredSource.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            FilteredRecords.Clear();
            var pageRecords = _filteredSource.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            foreach (var r in pageRecords)
                FilteredRecords.Add(r);
        }

        [RelayCommand(CanExecute = nameof(CanGoToPrevPage))]
        private void PrevPage()
        {
            CurrentPage--;
            UpdatePagination();
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private void NextPage()
        {
            CurrentPage++;
            UpdatePagination();
        }

        private void NotifyStats()
        {
            OnPropertyChanged(nameof(TotalOutstanding));
            OnPropertyChanged(nameof(TotalPendingCheques));
            OnPropertyChanged(nameof(TotalPartialPayments));
            OnPropertyChanged(nameof(TotalOverdueAmount));
            OnPropertyChanged(nameof(OutstandingClientsCount));
        }
    }
}
