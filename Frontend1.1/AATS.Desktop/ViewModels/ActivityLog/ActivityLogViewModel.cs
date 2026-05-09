using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.ActivityLog;

public partial class ActivityLogViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ActivityLogEntry> _filteredLogs = new();
    
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedModule = "All Modules";
    [ObservableProperty] private string _selectedAction = "All Actions";
    [ObservableProperty] private string _selectedBranch = "All Branches";

    // Pagination State
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

    private List<ActivityLogEntry> _allFilteredSource = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpecificDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsMonthVisible))]
    [NotifyPropertyChangedFor(nameof(IsYearVisible))]
    [NotifyPropertyChangedFor(nameof(IsPeriodVisible))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedDateFilter = "All Dates";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SpecificDateDisplay))] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _specificDate;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MonthDisplay))] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _monthDate;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedMonth;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedYear;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PeriodDisplay))] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _periodStartDate;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PeriodDisplay))] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _periodEndDate;

    public bool IsSpecificDateVisible => SelectedDateFilter == "Specific Date";
    public bool IsMonthVisible => SelectedDateFilter == "This Month";
    public bool IsYearVisible => SelectedDateFilter == "This Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Specific Period";

    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    public ObservableCollection<string> Months { get; } = new() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public ObservableCollection<string> Years { get; } = new() { "2024", "2025", "2026", "2027", "2028" };

    public string SpecificDateDisplay => SpecificDate.HasValue ? SpecificDate.Value.ToString("dd/MM/yyyy") : "Pick a date";
    public string MonthDisplay => MonthDate.HasValue ? MonthDate.Value.ToString("MMMM yyyy") : "Pick month";
    
    public string PeriodDisplay => PeriodStartDate.HasValue && PeriodEndDate.HasValue 
        ? $"{PeriodStartDate:dd/MM/yyyy} - {PeriodEndDate:dd/MM/yyyy}" 
        : "Pick a range";

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchText) || 
                                   SelectedDateFilter != "All Dates" || 
                                   SelectedModule != "All Modules" || 
                                   SelectedAction != "All Actions" || 
                                   SelectedBranch != "All Branches";

    public string DateFilterSummary
    {
        get
        {
            return SelectedDateFilter switch
            {
                "All Dates" => "All Time",
                "Today" => "Today",
                "This Week" => "This Week",
                "This Month" => MonthDate.HasValue ? MonthDate.Value.ToString("MMMM yyyy") : "This Month",
                "This Year" => !string.IsNullOrEmpty(SelectedYear) ? SelectedYear : "This Year",
                "Specific Date" => SpecificDate.HasValue ? SpecificDate.Value.ToString("dd MMM yyyy") : "Pick Date",
                "Specific Period" => (PeriodStartDate.HasValue && PeriodEndDate.HasValue) 
                    ? $"{PeriodStartDate:dd MMM} - {PeriodEndDate:dd MMM yyyy}" 
                    : "Select Range",
                _ => "Date Filter"
            };
        }
    }

    [ObservableProperty] private bool _isGuideVisible;
    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;
    public string GuideLinkText => "Learn more about Activity Logs";

    public ActivityLogViewModel()
    {
        ApplyFilters();
        LogService.Instance.Logs.CollectionChanged += (s, e) => ApplyFilters();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDateFilter = "All Dates";
        SelectedModule = "All Modules";
        SelectedAction = "All Actions";
        SelectedBranch = "All Branches";
        SpecificDate = null;
        MonthDate = null;
        SelectedMonth = null;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    partial void OnSelectedDateFilterChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }
    partial void OnSpecificDateChanged(DateTime? value) { OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }
    
    partial void OnMonthDateChanged(DateTime? value)
    {
        if (value.HasValue)
        {
            SelectedMonth = Months[value.Value.Month - 1];
            SelectedYear = value.Value.Year.ToString();
        }
        OnPropertyChanged(nameof(MonthDisplay));
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters();
    }
    
    partial void OnSelectedMonthChanged(string? value) { OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }
    partial void OnSelectedYearChanged(string? value) { OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }
    partial void OnPeriodStartDateChanged(DateTime? value) { OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }
    partial void OnPeriodEndDateChanged(DateTime? value) { OnPropertyChanged(nameof(DateFilterSummary)); ApplyFilters(); }


    [RelayCommand]
    private void ApplyFilters()
    {
        var raw = LogService.Instance.Logs.AsEnumerable();

        // 1. Explicitly filter out Access/Login logs (unless specifically requested, but here we hide them entirely)
        raw = raw.Where(l => l.Action != "Login" && l.Action != "Auth" && l.Action != "View");

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            raw = raw.Where(l => l.Details.ToLower().Contains(search) || 
                                 l.User.ToLower().Contains(search) || 
                                 l.Module.ToLower().Contains(search));
        }

        if (SelectedModule != "All Modules")
            raw = raw.Where(l => l.Module == SelectedModule);

        if (SelectedAction != "All Actions")
            raw = raw.Where(l => l.Action == SelectedAction);

        if (SelectedBranch != "All Branches")
            raw = raw.Where(l => l.Branch == SelectedBranch);

        // Advanced Date Filtering Logic
        DateTime now = DateTime.Now;
        if (SelectedDateFilter == "Today")
        {
            raw = raw.Where(l => l.Timestamp.Date == now.Date);
        }
        else if (SelectedDateFilter == "This Week")
        {
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            raw = raw.Where(l => l.Timestamp.Date >= startOfWeek.Date && l.Timestamp.Date <= now.Date);
        }
        else if (SelectedDateFilter == "This Month")
        {
            int targetMonth = MonthDate?.Month ?? now.Month;
            int targetYear = MonthDate?.Year ?? now.Year;
            raw = raw.Where(l => l.Timestamp.Month == targetMonth && l.Timestamp.Year == targetYear);
        }
        else if (SelectedDateFilter == "This Year")
        {
            int year = !string.IsNullOrEmpty(SelectedYear) ? int.Parse(SelectedYear) : now.Year;
            raw = raw.Where(l => l.Timestamp.Year == year);
        }
        else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
        {
            raw = raw.Where(l => l.Timestamp.Date == SpecificDate.Value.Date);
        }
        else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
        {
            raw = raw.Where(l => l.Timestamp.Date >= PeriodStartDate.Value.Date && l.Timestamp.Date <= PeriodEndDate.Value.Date);
        }

        _allFilteredSource = raw.OrderByDescending(l => l.Timestamp).ToList();
        
        // Ensure UI updates happen on correct thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() => {
            CurrentPage = 1;
            UpdatePagination();
        });
    }

    private void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(_allFilteredSource.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;

        FilteredLogs.Clear();
        var pageRecords = _allFilteredSource.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var log in pageRecords)
        {
            FilteredLogs.Add(log);
        }
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

    [RelayCommand] private void UpdateModule(string module) { SelectedModule = module; OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    [RelayCommand] private void UpdateAction(string action) { SelectedAction = action; OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    [RelayCommand] private void UpdateDateFilter(string date) { SelectedDateFilter = date; ApplyFilters(); }
    [RelayCommand] private void UpdateBranch(string branch) { SelectedBranch = branch; OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
}
