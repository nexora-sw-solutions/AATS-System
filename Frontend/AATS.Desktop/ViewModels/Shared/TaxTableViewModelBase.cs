using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AATS.Desktop.Views.Reports;
using AATS.Desktop.ViewModels.Reports;

namespace AATS.Desktop.ViewModels.Shared;

public abstract partial class TaxTableViewModelBase : ViewModelBase
{
    public TaxTableViewModelBase()
    {
        _ = LoadDataAsync();
        _ = LoadClientCodesAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var records = await DataService.Instance.GetTaxRecordsAsync(PageTitle);
        _allRecords.Clear();
        _allRecords.AddRange(records);
        ApplyFilter();
    }
    // Page Customization Properties
    public abstract string PageTitle { get; }
    public abstract string SearchPlaceholder { get; }

    // --- Drawer State ---
    [ObservableProperty] private bool _isDrawerOpen;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private TaxRecord? _selectedRecord;

    // --- Guide State ---
    [ObservableProperty] private bool _isGuideVisible;
    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    public abstract string GuideLinkText { get; }
    public abstract string GuideDescription { get; }
    public abstract string GuideFeature1Title { get; }
    public abstract string GuideFeature1Text { get; }
    public abstract string GuideFeature2Title { get; }
    public abstract string GuideFeature2Text { get; }
    public abstract string GuideFeature3Title { get; }
    public abstract string GuideFeature3Text { get; }
    public abstract string GuideFeature4Title { get; }
    public abstract string GuideFeature4Text { get; }
    public abstract string GuideProTip { get; }

    // Editable fields (used in edit mode)
    [ObservableProperty] private string _editClientName = string.Empty;
    [ObservableProperty] private string _editDirectorId = string.Empty;
    [ObservableProperty] private string _editStatus = string.Empty;
    [ObservableProperty] private string _editAssessmentYear = string.Empty;
    [ObservableProperty] private string _editCurrentPeriod = string.Empty;
    [ObservableProperty] private string _editAuditorNotes = string.Empty;

    // Display fields (read-only mode)
    public string DisplayClientName => SelectedRecord?.ClientName ?? "N/A";
    public string DisplayId => SelectedRecord?.Code ?? "N/A";
    public string DisplayStatus => SelectedRecord?.Status ?? "UNKNOWN";
    public string DisplayAssessmentYear => "2024/2025"; // Default as per mockups
    public string DisplayCurrentPeriod => SelectedRecord?.TaxPeriod ?? string.Empty;
    public string DisplayAuditorNotes => SelectedRecord?.Notes ?? string.Empty;
    public string DisplayDirectorId => SelectedRecord?.DINNo ?? "N/A";
    public string DisplayTin => "N/A";

    [RelayCommand]
    protected virtual void CloseDrawer()
    {
        IsDrawerOpen = false;
        IsEditMode = false;
        SelectedRecord = null;
    }

    [RelayCommand]
    public void OpenReport()
    {
        if (SelectedRecord == null) return;
        var reportVm = new ReportViewModel(SelectedRecord, PageTitle);
        var reportWindow = new ReportView { DataContext = reportVm };
        reportWindow.Show();
    }

    [RelayCommand]
    public async Task DownloadReport()
    {
        if (SelectedRecord == null) return;
        var reportVm = new ReportViewModel(SelectedRecord, PageTitle);
        await reportVm.PrintCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    protected virtual void EnterEditMode()
    {
        if (SelectedRecord == null) return;
        EditClientName = SelectedRecord.ClientName ?? string.Empty;
        EditDirectorId = SelectedRecord.DINNo ?? string.Empty;
        EditStatus = SelectedRecord.Status ?? "UNKNOWN";
        EditAssessmentYear = "2024/2025";
        EditCurrentPeriod = SelectedRecord.TaxPeriod ?? string.Empty;
        EditAuditorNotes = SelectedRecord.Notes ?? string.Empty;
        IsEditMode = true;
    }

    [ObservableProperty] private bool _isDrawerDeleteConfirmVisible;
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;
    [ObservableProperty] private bool _isDiscardConfirmVisible;

    [RelayCommand]
    protected virtual void RequestDeleteDrawerRecord()
    {
        IsDrawerDeleteConfirmVisible = true;
    }

    [RelayCommand]
    protected virtual async Task ConfirmDrawerDelete()
    {
        IsDrawerDeleteConfirmVisible = false;
        if (SelectedRecord != null)
        {
            await DataService.Instance.DeleteTaxRecordsAsync(PageTitle, new[] { SelectedRecord });
            _allRecords.Remove(SelectedRecord);
            ApplyFilter();
            CloseDrawer();
        }
    }

    [RelayCommand]
    protected virtual void CancelDrawerDelete()
    {
        IsDrawerDeleteConfirmVisible = false;
    }

    [RelayCommand]
    protected virtual void CancelEdit()
    {
        IsDiscardConfirmVisible = true;
    }

    [RelayCommand]
    protected virtual async Task SaveEdit()
    {
        if (SelectedRecord == null) return;
        SelectedRecord.ClientName = EditClientName;
        SelectedRecord.DINNo = EditDirectorId;
        SelectedRecord.Status = EditStatus;
        SelectedRecord.TaxPeriod = EditCurrentPeriod;
        SelectedRecord.Notes = EditAuditorNotes;

        await DataService.Instance.UpdateTaxRecordAsync(PageTitle, SelectedRecord);

        IsEditMode = false;
        RefreshDisplayProperties();
    }

    [RelayCommand]
    public void ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        IsEditMode = false;
    }

    [RelayCommand]
    public void CancelDiscard()
    {
        IsDiscardConfirmVisible = false;
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var count = _allRecords.Count(r => r.IsSelected);
        if (count == 0) return;
        
        DeleteConfirmMessage = count == 1 
            ? "Are you sure you want to delete this record? This action cannot be undone."
            : $"Are you sure you want to delete {count} selected records? This action cannot be undone.";
            
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteSelected()
    {
        IsDeleteConfirmVisible = false;
        var toDelete = _allRecords.Where(r => r.IsSelected).ToList();
        if (toDelete.Count == 0) return;

        await DataService.Instance.DeleteTaxRecordsAsync(PageTitle, toDelete);

        foreach (var d in toDelete)
            _allRecords.Remove(d);
            
        SelectedRecordCount = 0;
        IsAllSelected = false;
        ApplyFilter();
    }

    [RelayCommand]
    private void CancelDeleteSelected()
    {
        IsDeleteConfirmVisible = false;
    }

    protected void RefreshDisplayProperties()
    {
        OnPropertyChanged(nameof(DisplayClientName));
        OnPropertyChanged(nameof(DisplayId));
        OnPropertyChanged(nameof(DisplayStatus));
        OnPropertyChanged(nameof(DisplayAssessmentYear));
        OnPropertyChanged(nameof(DisplayCurrentPeriod));
        OnPropertyChanged(nameof(DisplayAuditorNotes));
        OnPropertyChanged(nameof(DisplayDirectorId));
        OnPropertyChanged(nameof(DisplayTin));
    }

    public Action<TaxRecord>? NavigateToDetail { get; set; }

    [RelayCommand]
    protected virtual void Details(TaxRecord record)
    {
        if (NavigateToDetail != null)
        {
            NavigateToDetail.Invoke(record);
        }
        else
        {
            SelectedRecord = record;
            IsEditMode = false;
            IsDrawerOpen = true;
            RefreshDisplayProperties();
        }
    }

    // Localized field properties for ID types (e.g. DIN, TIN, VAT No)
    public abstract string TaxIdLabel { get; }
    public abstract string TaxIdPlaceholder { get; }
    public abstract string TaxIdHeader { get; }

    // Form properties
    [ObservableProperty] private string? _clientId;
    partial void OnClientIdChanged(string? value) => FilterClientCodes(value);

    public override void SelectClientCode(ClientRecord client)
    {
        ClientId = client.ClientCode ?? string.Empty;
        ClientName = client.Name ?? string.Empty;
        IsClientCodeDropdownOpen = false;
    }
    [ObservableProperty] private string? _clientName;
    [ObservableProperty] private string? _directorId; // This is the value for TaxIdLabel
    [ObservableProperty] private int _duration = 1;
    [ObservableProperty] private string _durationUnit = "Months";
    [ObservableProperty] private string _paymentStatus = "Pending";
    
    public bool IsPaidStatus
    {
        get => PaymentStatus == "Paid";
        set { if (value) PaymentStatus = "Paid"; }
    }
    public bool IsPendingStatus
    {
        get => PaymentStatus == "Pending";
        set { if (value) PaymentStatus = "Pending"; }
    }
    public bool IsIRDPaidStatus
    {
        get => PaymentStatus == "IRD Paid";
        set { if (value) PaymentStatus = "IRD Paid"; }
    }

    partial void OnPaymentStatusChanged(string value)
    {
        OnPropertyChanged(nameof(IsPaidStatus));
        OnPropertyChanged(nameof(IsPendingStatus));
        OnPropertyChanged(nameof(IsIRDPaidStatus));
    }

    // Search & Filter
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpecificDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsMonthVisible))]
    [NotifyPropertyChangedFor(nameof(IsYearVisible))]
    [NotifyPropertyChangedFor(nameof(IsPeriodVisible))]
    [NotifyPropertyChangedFor(nameof(IsClearDateFilterVisible))]
    private string _selectedDateFilter = "All Dates";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpecificDateDisplay))]
    private DateTime? _specificDate;

    [ObservableProperty] private string? _selectedMonth;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedYear;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodStartDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodEndDate;

    [ObservableProperty] private string _selectedStatusFilter = "All Status";
    [ObservableProperty] private string _selectedBranchFilter = "All Branch";

    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    public ObservableCollection<string> Years { get; } = new() { "2023", "2024", "2025", "2026", "2027", "2028" };

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchText) || 
                                   SelectedDateFilter != "All Dates" || 
                                   SelectedStatusFilter != "All Status" || 
                                   SelectedBranchFilter != "All Branch";

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

    [ObservableProperty] private DateTime? _monthDate = DateTime.Now;

    // Date filter visibility (Legacy compatibility, but preferably use styles)
    public bool IsSpecificDateVisible => SelectedDateFilter == "Specific Period";
    public bool IsMonthVisible => SelectedDateFilter == "This Month";
    public bool IsYearVisible => SelectedDateFilter == "This Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Specific Period";
    public bool IsClearDateFilterVisible => HasActiveFilters;

    public string PeriodDisplay => PeriodStartDate.HasValue && PeriodEndDate.HasValue 
        ? $"{PeriodStartDate:dd/MM/yyyy} - {PeriodEndDate:dd/MM/yyyy}" 
        : "Pick a date range";

    public string SpecificDateDisplay => SpecificDate.HasValue 
        ? SpecificDate.Value.ToString("dd/MM/yyyy") 
        : "Pick a date";

    // Selection
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedRecords))]
    private int _selectedRecordCount = 0;

    public bool HasSelectedRecords => SelectedRecordCount > 0;

    [ObservableProperty] private bool _isAllSelected;

    partial void OnIsAllSelectedChanged(bool value)
    {
        foreach (var record in _filteredRecords)
            record.IsSelected = value;
        CalculateSelected();
    }

    [RelayCommand]
    private void CalculateSelected()
    {
        SelectedRecordCount = _allRecords.Count(r => r.IsSelected);
        if (_filteredRecords.Count > 0)
        {
            var expectedAllSelected = _filteredRecords.All(r => r.IsSelected);
            if (IsAllSelected != expectedAllSelected)
            {
                IsAllSelected = expectedAllSelected;
                OnPropertyChanged(nameof(IsAllSelected));
            }
        }
    }



    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var record in _allRecords)
            record.IsSelected = false;
        CalculateSelected();
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDateFilter = "All Dates";
        SelectedStatusFilter = "All Status";
        SelectedBranchFilter = "All Branch";
        SpecificDate = null;
        MonthDate = DateTime.Now;
        SelectedMonth = null;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
        ApplyFilter();
    }

    // Pagination
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationDisplay))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaginationDisplay))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalPages = 1;

    protected const int PageSize = 10;
    
    public string PaginationDisplay => $"Page {CurrentPage} of {Math.Max(1, TotalPages)}";
    public string PageSetText => $"Page {CurrentPage} of {Math.Max(1, TotalPages)}";
    public bool CanGoToPrevPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;

    protected List<TaxRecord> _allRecords = new();
    protected List<TaxRecord> _filteredRecords = new();
    public ObservableCollection<TaxRecord> DisplayedRecords { get; } = new();

    // Filter Options
    public ObservableCollection<string> DateFilterOptions { get; } = new() { "All Dates", "Specific Date", "Month", "Year", "Period" };
    public ObservableCollection<string> StatusFilterOptions { get; } = new() { "All Status", "Paid", "Pending", "IRD pending" };
    public List<string> DrawerStatusFilters { get; } = new() { "Paid", "Pending", "IRD Paid" };
    public ObservableCollection<string> BranchFilterOptions { get; } = new() { "All Branch", "South", "West", "Central", "Northeast" };
    public ObservableCollection<string> Months { get; } = new() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    public List<string> DurationUnitOptions { get; } = new() { "Days", "Months", "Years" };

    protected void ApplyFilter()
    {
        _filteredRecords.Clear();

        foreach (var record in _allRecords)
        {
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                !(record.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
                !(record.DINNo?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
                !(record.ClientCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            if (SelectedStatusFilter != "All Status" && 
                !(record.Status?.Equals(SelectedStatusFilter, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            if (SelectedBranchFilter != "All Branch" && record.Branch != SelectedBranchFilter)
                continue;

            DateTime now = DateTime.Now;
            if (SelectedDateFilter == "Today")
            {
                if (record.Date.Date != now.Date) continue;
            }
            else if (SelectedDateFilter == "This Week")
            {
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                if (record.Date.Date < startOfWeek.Date || record.Date.Date > now.Date) continue;
            }
            else if (SelectedDateFilter == "This Month")
            {
                int targetMonth = MonthDate?.Month ?? now.Month;
                int targetYear = MonthDate?.Year ?? now.Year;
                if (record.Date.Month != targetMonth || record.Date.Year != targetYear) continue;
            }
            else if (SelectedDateFilter == "This Year")
            {
                int year = !string.IsNullOrEmpty(SelectedYear) ? int.Parse(SelectedYear) : now.Year;
                if (record.Date.Year != year) continue;
            }
            else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
            {
                if (record.Date.Date != SpecificDate.Value.Date) continue;
            }
            else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
            {
                if (record.Date.Date < PeriodStartDate.Value.Date || record.Date.Date > PeriodEndDate.Value.Date) continue;
            }

            _filteredRecords.Add(record);
        }

        CurrentPage = 1;
        UpdatePagination();
    }

    protected void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(_filteredRecords.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        DisplayedRecords.Clear();
        var pageRecords = _filteredRecords.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var r in pageRecords)
        {
            DisplayedRecords.Add(r);
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void NextPage()
    {
        CurrentPage++;
        UpdatePagination();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPrevPage))]
    private void PreviousPage()
    {
        CurrentPage--;
        UpdatePagination();
    }

    // Filter change handlers
    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedDateFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedBranchFilterChanged(string value) => ApplyFilter();
    partial void OnSpecificDateChanged(DateTime? value) => ApplyFilter();
    partial void OnMonthDateChanged(DateTime? value)
    {
        if (value.HasValue)
        {
            SelectedMonth = Months[value.Value.Month - 1];
            SelectedYear = value.Value.Year.ToString();
        }
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilter();
    }
    partial void OnSelectedMonthChanged(string? value) => ApplyFilter();
    partial void OnSelectedYearChanged(string? value) => ApplyFilter();
    partial void OnPeriodStartDateChanged(DateTime? value) => ApplyFilter();
    partial void OnPeriodEndDateChanged(DateTime? value) => ApplyFilter();

    [RelayCommand]
    protected virtual async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(ClientId) && string.IsNullOrWhiteSpace(ClientName)) return;

        var record = new TaxRecord
        {
            ClientCode = ClientId,
            ClientName = ClientName,
            ClientNameSub = ClientId,
            DINNo = DirectorId,
            TaxPeriod = $"{Duration} {DurationUnit}",
            Status = PaymentStatus == "IRD Paid" ? "IRD pending" : PaymentStatus,
            Branch = "South",
            Date = DateTime.Now
        };

        await DataService.Instance.AddTaxRecordAsync(PageTitle, record);
        _allRecords.Add(record);
        
        Clear();
        ApplyFilter();
    }

    [RelayCommand]
    protected virtual void Clear()
    {
        ClientId = string.Empty;
        ClientName = string.Empty;
        DirectorId = string.Empty;
        Duration = 1;
        PaymentStatus = "Pending";
    }
}
