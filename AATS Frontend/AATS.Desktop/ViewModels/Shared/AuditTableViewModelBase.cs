using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Views.Reports;
using AATS.Desktop.ViewModels.Reports;

namespace AATS.Desktop.ViewModels.Shared;

public abstract partial class AuditTableViewModelBase : ViewModelBase
{
    // Page Customization Properties
    public abstract string PageTitle { get; }
    public abstract string SearchPlaceholder { get; }
    public abstract string StatusHeader { get; }
    public virtual string ProcessHeader => "Process";
    public virtual string ClientHeader => "Client";
    public virtual string CompanyHeader => "Company Name";

    public abstract string GuideLinkText { get; }
    public virtual string GuideDescription => "Master the tools for tracking and managing records.";
    public virtual string GuideFeature1Title => "Advanced Filtering";
    public virtual string GuideFeature1Text => "Filter by Specific Date, Month, Year, or Period. Combine with other filters for precision.";
    public virtual string GuideFeature2Title => "Bulk Actions";
    public virtual string GuideFeature2Text => "Select multiple records using the checkboxes to perform bulk deletion and management tasks.";
    public virtual string GuideFeature3Title => "Dynamic Records";
    public virtual string GuideFeature3Text => "Quickly jump to client details or edit records directly from the table using the action menus.";
    public virtual string GuideFeature4Title => "Real-time Search";
    public virtual string GuideFeature4Text => "Instantly find any client by name or ID using the global search bar updated in real-time.";
    public virtual string GuideProTip => "Use the \"Period\" filter to generate custom reports for any date range required by your firm.";

    // Component Visibility Configuration
    public virtual bool IsProcessVisible => true;
    public virtual bool IsStatusVisible => true;
    public virtual bool IsNoOfStaffsVisible => false;
    public virtual bool IsCountryVisible => false;
    public virtual bool IsBranchVisible => true;
    public virtual bool IsCompanyVisible => false; // Hidden by default for standard audit pages
    public virtual bool IsAssignmentVisible => false; // Hidden by default for standard audit pages

    public virtual bool IsStatusFilterVisible => IsStatusVisible;
    public virtual bool IsProcessFilterVisible => IsProcessVisible;
    
    [ObservableProperty]
    private ObservableCollection<string> _processFilters = new() { "All Process" };

    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    public ObservableCollection<string> StatusFilters { get; } = new() { "All Status", "Paid", "Unpaid", "Partial" };
    public ObservableCollection<string> BranchFilters { get; } = new() { "All Branch", "South", "West", "Central", "Northeast" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpecificDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsMonthVisible))]
    [NotifyPropertyChangedFor(nameof(IsYearVisible))]
    [NotifyPropertyChangedFor(nameof(IsPeriodVisible))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedDateFilter = "All Dates";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedStatusFilter = "All Status";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedProcessFilter = "All Process";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedBranchFilter = "All Branch";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isGuideVisible = false;

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

    [RelayCommand]
    public async Task PrintReport()
    {
        if (SelectedRecord == null) return;
        try {
            await RecordReportService.Instance.PrintReportAsync(SelectedRecord, PageTitle, MainViewModel.Instance?.CurrentUser?.Username ?? "System");
        } catch (Exception ex) {
            NotificationService.Instance.AddNotification("Error", $"Could not print report: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task DownloadReport()
    {
        if (SelectedRecord == null) return;
        try {
            await RecordReportService.Instance.DownloadReportAsync(SelectedRecord, PageTitle, MainViewModel.Instance?.CurrentUser?.Username ?? "System");
        } catch (Exception ex) {
            NotificationService.Instance.AddNotification("Error", $"Could not download report: {ex.Message}");
        }
    }

    // --- Drawer State ---
    [ObservableProperty] private bool _isDrawerOpen;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private AuditRecord? _selectedRecord;

    // Editable fields (used in edit mode)
    [ObservableProperty] private string _editClientName = string.Empty;
    [ObservableProperty] private string _editStatus = string.Empty;
    [ObservableProperty] private string _editAssessmentYear = string.Empty;
    [ObservableProperty] private string _editCurrentPeriod = string.Empty;
    [ObservableProperty] private string _editAuditorNotes = string.Empty;
    [ObservableProperty] private string _editAssignment = string.Empty;
    [ObservableProperty] private string _editBranch = string.Empty;
    [ObservableProperty] private string _editCountry = string.Empty;
    [ObservableProperty] private string _editCompany = string.Empty;
    [ObservableProperty] private int _editNoOfStaffs;
    [ObservableProperty] private string _editTin = string.Empty;
    [ObservableProperty] private string _editDirectorId = string.Empty;

    // Display fields (read-only mode)
    public string DisplayClientName => SelectedRecord?.ClientName ?? "N/A";
    public string DisplayId => SelectedRecord?.Code ?? SelectedRecord?.ClientCode ?? "N/A";
    public string DisplayStatus => SelectedRecord?.PaymentStatus ?? "UNKNOWN";
    public string DisplayAssessmentYear => SelectedRecord?.Period ?? "N/A";
    public string DisplayCurrentPeriod => SelectedRecord?.Branch ?? string.Empty;
    public string DisplayAuditorNotes => SelectedRecord?.Notes ?? string.Empty;
    public string DisplayAssignment => SelectedRecord?.Assignment ?? "N/A";
    public string DisplayBranch => SelectedRecord?.Branch ?? "N/A";
    public string DisplayCountry => SelectedRecord?.Country ?? "N/A";
    public string DisplayCompany => SelectedRecord?.Company ?? "N/A";
    public string DisplayNoOfStaffs => SelectedRecord?.NoOfStaffs.ToString() ?? "0";
    public virtual string DisplayDirectorId => SelectedRecord?.DirectorID ?? "N/A";
    public virtual string DisplayTin => SelectedRecord?.TIN ?? "N/A";

    [RelayCommand]
    public virtual void CloseDrawer()
    {
        if (IsEditMode)
        {
            IsDiscardConfirmVisible = true;
            return;
        }
        IsDrawerOpen = false;
        IsEditMode = false;
        SelectedRecord = null;
    }

    [RelayCommand]
    public virtual void EnterEditMode()
    {
        if (SelectedRecord == null) return;

        Action proceed = () =>
        {
            EditClientName = SelectedRecord.ClientName ?? string.Empty;
            EditStatus = SelectedRecord.PaymentStatus ?? "UNKNOWN";
            EditAssessmentYear = SelectedRecord.Period ?? string.Empty;
            EditCurrentPeriod = SelectedRecord.Branch ?? string.Empty;
            EditAuditorNotes = SelectedRecord.Notes ?? string.Empty;
            EditAssignment = SelectedRecord.Assignment ?? string.Empty;
            EditBranch = SelectedRecord.Branch ?? string.Empty;
            EditCountry = SelectedRecord.Country ?? string.Empty;
            EditCompany = SelectedRecord.Company ?? string.Empty;
            EditNoOfStaffs = SelectedRecord.NoOfStaffs;
            EditTin = SelectedRecord.TIN ?? string.Empty;
            EditDirectorId = SelectedRecord.DirectorID ?? string.Empty;
            IsEditMode = true;
        };

        if (MainViewModel.Instance != null)
        {
            MainViewModel.Instance.ExecuteAuthorizedAction(proceed);
        }
        else
        {
            proceed();
        }
    }

    [RelayCommand]
    public virtual void CancelEdit()
    {
        IsDiscardConfirmVisible = true;
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
    public virtual async Task SaveEdit()
    {
        if (SelectedRecord == null) return;
        SelectedRecord.ClientName = EditClientName;
        SelectedRecord.PaymentStatus = EditStatus;
        SelectedRecord.Period = EditAssessmentYear;
        SelectedRecord.Branch = EditCurrentPeriod;
        SelectedRecord.Notes = EditAuditorNotes;
        SelectedRecord.Assignment = EditAssignment;
        SelectedRecord.Country = EditCountry;
        SelectedRecord.Company = EditCompany;
        SelectedRecord.NoOfStaffs = EditNoOfStaffs;
        SelectedRecord.TIN = EditTin;
        SelectedRecord.DirectorID = EditDirectorId;
        
        await DataService.Instance.UpdateAuditRecordAsync(PageTitle, SelectedRecord);
        
        IsEditMode = false;
        RefreshDisplayProperties();
    }

    [ObservableProperty] private bool _isDrawerDeleteConfirmVisible;
    [ObservableProperty] private bool _isDiscardConfirmVisible;

    [RelayCommand]
    public virtual void RequestDeleteDrawerRecord()
    {
        IsDrawerDeleteConfirmVisible = true;
    }

    [RelayCommand]
    public virtual async Task ConfirmDrawerDelete()
    {
        IsDrawerDeleteConfirmVisible = false;
        if (SelectedRecord != null)
        {
            await DataService.Instance.DeleteAuditRecordsAsync(PageTitle, new[] { SelectedRecord });
            _allRecords.Remove(SelectedRecord);
            ApplyFilters();
            CalculateSelected();
            CloseDrawer();
        }
    }

    [RelayCommand]
    public virtual void CancelDrawerDelete()
    {
        IsDrawerDeleteConfirmVisible = false;
    }

    protected void RefreshDisplayProperties()
    {
        OnPropertyChanged(nameof(DisplayClientName));
        OnPropertyChanged(nameof(DisplayId));
        OnPropertyChanged(nameof(DisplayStatus));
        OnPropertyChanged(nameof(DisplayAssessmentYear));
        OnPropertyChanged(nameof(DisplayCurrentPeriod));
        OnPropertyChanged(nameof(DisplayAuditorNotes));
        OnPropertyChanged(nameof(DisplayAssignment));
        OnPropertyChanged(nameof(DisplayBranch));
        OnPropertyChanged(nameof(DisplayCountry));
        OnPropertyChanged(nameof(DisplayCompany));
        OnPropertyChanged(nameof(DisplayNoOfStaffs));
        OnPropertyChanged(nameof(DisplayDirectorId));
        OnPropertyChanged(nameof(DisplayTin));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpecificDateDisplay))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _specificDate;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthDisplay))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _monthDate;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedMonth;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedYear;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _periodStartDate;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _periodEndDate;

    public string MonthDisplay => MonthDate.HasValue ? MonthDate.Value.ToString("MMMM yyyy") : "Pick month";
    
    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchText) || 
                                   SelectedDateFilter != "All Dates" || 
                                   SelectedStatusFilter != "All Status" || 
                                   SelectedProcessFilter != "All Process" ||
                                   SelectedBranchFilter != "All Branch";

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

    public string PeriodDisplay => PeriodStartDate.HasValue && PeriodEndDate.HasValue 
        ? $"{PeriodStartDate:dd/MM/yyyy} - {PeriodEndDate:dd/MM/yyyy}" 
        : "Pick a date range";

    public string SpecificDateDisplay => SpecificDate.HasValue 
        ? SpecificDate.Value.ToString("dd/MM/yyyy") 
        : "Pick a date";

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
    public void CalculateSelected()
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
    // Bulk Delete Confirmation
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;

    [RelayCommand]
    public void DeleteSelected()
    {
        var count = _allRecords.Count(r => r.IsSelected);
        if (count == 0) return;
        DeleteConfirmMessage = $"Are you sure you want to delete {count} selected record{(count > 1 ? "s" : "")}? This action cannot be undone.";
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    public async Task ConfirmDeleteSelected()
    {
        IsDeleteConfirmVisible = false;
        var toDelete = _allRecords.Where(r => r.IsSelected).ToList();
        if (!toDelete.Any() && SelectedRecord != null)
        {
            toDelete.Add(SelectedRecord);
        }
        
        await DataService.Instance.DeleteAuditRecordsAsync(PageTitle, toDelete);
        
        SelectedRecordCount = 0;
        IsAllSelected = false;

        await LoadDataAsync();
    }

    [RelayCommand]
    public void CancelDeleteSelected()
    {
        IsDeleteConfirmVisible = false;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var record in _allRecords)
            record.IsSelected = false;
        CalculateSelected();
    }

    public bool IsSpecificDateVisible => SelectedDateFilter == "Specific Date";
    public bool IsMonthVisible => SelectedDateFilter == "This Month";
    public bool IsYearVisible => SelectedDateFilter == "This Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Specific Period";

    public ObservableCollection<string> Months { get; } = new() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public ObservableCollection<string> Years { get; } = new() { "2024", "2025", "2026", "2027", "2028" };



    [ObservableProperty] private ObservableCollection<AuditRecord> _deletedRecords = new();
    [ObservableProperty] private bool _isTrashExpanded;

    public AuditTableViewModelBase()
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var records = await DataService.Instance.GetAuditRecordsAsync(PageTitle);
        var clients = await DataService.Instance.GetClientsAsync();
        var deletedRecords = await DataService.Instance.GetDeletedAuditRecordsAsync(PageTitle);

        foreach (var r in records)
        {
            var client = clients.FirstOrDefault(c => c.ClientCode == r.ClientCode);
            if (client != null) { r.ClientStatus = client.Status; r.ClientCategory = client.Status; }
        }
        _allRecords.Clear();
        _allRecords.AddRange(records);

        DeletedRecords.Clear();
        foreach (var d in deletedRecords)
        {
            DeletedRecords.Add(d);
        }

        ApplyFilters();
    }

    [ObservableProperty] private bool _isRestoreConfirmVisible;
    [ObservableProperty] private string _restoreConfirmMessage = string.Empty;
    private AuditRecord? _recordToRestore;

    [ObservableProperty] private bool _isPurgeConfirmVisible;
    [ObservableProperty] private string _purgeConfirmMessage = string.Empty;
    private AuditRecord? _recordToPurge;

    [RelayCommand]
    public void RestoreAuditRecord(AuditRecord? record)
    {
        if (record == null || string.IsNullOrEmpty(record.ID)) return;
        _recordToRestore = record;
        RestoreConfirmMessage = $"Are you sure you want to restore record '{record.ClientName ?? record.Code}'? This record will be moved back to active records.";
        IsRestoreConfirmVisible = true;
    }

    [RelayCommand]
    public async Task ConfirmRestoreAuditRecord()
    {
        IsRestoreConfirmVisible = false;
        if (_recordToRestore != null && !string.IsNullOrEmpty(_recordToRestore.ID))
        {
            bool success = await DataService.Instance.RestoreAuditRecordAsync(PageTitle, _recordToRestore.ID);
            if (success)
            {
                LogService.Instance.AddLog("Restore", PageTitle, _recordToRestore.Branch ?? "Central", $"Restored soft-deleted audit record: {_recordToRestore.Code}");
                await LoadDataAsync();
            }
        }
        _recordToRestore = null;
    }

    [RelayCommand]
    public void CancelRestoreAuditRecord()
    {
        IsRestoreConfirmVisible = false;
        _recordToRestore = null;
    }

    [RelayCommand]
    public void PermanentlyDeleteAuditRecord(AuditRecord? record)
    {
        if (record == null || string.IsNullOrEmpty(record.ID)) return;
        _recordToPurge = record;
        PurgeConfirmMessage = $"Are you sure you want to permanently delete record '{record.ClientName ?? record.Code}'? This action cannot be undone and will erase all record data.";
        IsPurgeConfirmVisible = true;
    }

    [RelayCommand]
    public async Task ConfirmPurgeAuditRecord()
    {
        IsPurgeConfirmVisible = false;
        if (_recordToPurge != null && !string.IsNullOrEmpty(_recordToPurge.ID))
        {
            bool success = await DataService.Instance.PermanentlyDeleteAuditRecordAsync(PageTitle, _recordToPurge.ID);
            if (success)
            {
                LogService.Instance.AddLog("Purge", PageTitle, _recordToPurge.Branch ?? "Central", $"Permanently purged audit record: {_recordToPurge.Code}");
                await LoadDataAsync();
            }
        }
        _recordToPurge = null;
    }

    [RelayCommand]
    public void CancelPurgeAuditRecord()
    {
        IsPurgeConfirmVisible = false;
        _recordToPurge = null;
    }

    public ObservableCollection<AuditRecord> Records { get; } = new();

    protected readonly List<AuditRecord> _allRecords = new();
    protected readonly List<AuditRecord> _filteredRecords = new();

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

    protected void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(_filteredRecords.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        Records.Clear();
        var pageRecords = _filteredRecords.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var r in pageRecords)
        {
            Records.Add(r);
        }
    }

    protected void ApplyFilters()
    {
        _filteredRecords.Clear();
        foreach (var record in _allRecords)
        {
            Console.WriteLine($"[DEBUG] Filtering Record: {record.Code} - {record.ClientName}");
            if (!string.IsNullOrWhiteSpace(SearchText) && 
                !(record.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) && 
                !(record.ClientCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                Console.WriteLine($"[DEBUG] Skipped by Search: {SearchText}");
                continue;
            }

            if (SelectedStatusFilter != "All Status" && record.PaymentStatus != SelectedStatusFilter)
            {
                Console.WriteLine($"[DEBUG] Skipped by Status: {record.PaymentStatus} vs {SelectedStatusFilter}");
                continue;
            }

            if (SelectedProcessFilter != "All Process" && !(record.Process?.Equals(SelectedProcessFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                Console.WriteLine($"[DEBUG] Skipped by Process: {record.Process} vs {SelectedProcessFilter}");
                continue;
            }

            if (SelectedBranchFilter != "All Branch" && record.Branch != SelectedBranchFilter)
            {
                Console.WriteLine($"[DEBUG] Skipped by Branch: {record.Branch} vs {SelectedBranchFilter}");
                continue;
            }

            DateTime now = DateTime.Now;
            if (SelectedDateFilter == "Today")
            {
                if (record.Date.Date != now.Date) { Console.WriteLine("[DEBUG] Skipped by Date (Today)"); continue; }
            }
            else if (SelectedDateFilter == "This Week")
            {
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                if (record.Date.Date < startOfWeek.Date || record.Date.Date > now.Date) { Console.WriteLine("[DEBUG] Skipped by Date (Week)"); continue; }
            }
            else if (SelectedDateFilter == "This Month")
            {
                int targetMonth = MonthDate?.Month ?? now.Month;
                int targetYear = MonthDate?.Year ?? now.Year;
                if (record.Date.Month != targetMonth || record.Date.Year != targetYear) { Console.WriteLine("[DEBUG] Skipped by Date (Month)"); continue; }
            }
            else if (SelectedDateFilter == "This Year")
            {
                int year = !string.IsNullOrEmpty(SelectedYear) ? int.Parse(SelectedYear) : now.Year;
                if (record.Date.Year != year) { Console.WriteLine("[DEBUG] Skipped by Date (Year)"); continue; }
            }
            else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
            {
                if (record.Date.Date != SpecificDate.Value.Date) { Console.WriteLine("[DEBUG] Skipped by Date (Specific)"); continue; }
            }
            else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
            {
                if (record.Date.Date < PeriodStartDate.Value.Date || record.Date.Date > PeriodEndDate.Value.Date) { Console.WriteLine("[DEBUG] Skipped by Date (Period)"); continue; }
            }

            _filteredRecords.Add(record);
            Console.WriteLine("[DEBUG] Record Passed Filters");
        }
        
        var sorted = _filteredRecords.OrderBy(r => r.ClientCode).ToList();
        _filteredRecords.Clear();
        _filteredRecords.AddRange(sorted);
        
        CurrentPage = 1;
        UpdatePagination();
    }

    // Handlers
    partial void OnSearchTextChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    partial void OnSelectedStatusFilterChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    partial void OnSelectedProcessFilterChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
    partial void OnSelectedBranchFilterChanged(string value) { OnPropertyChanged(nameof(HasActiveFilters)); ApplyFilters(); }
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

    // The Navigation Hook to be handled uniquely by subclasses (or MainViewModel) depending on app routing architecture.
    public Action? NavigateToAddRecord { get; set; }
    public Action<AuditRecord>? NavigateToDetail { get; set; }

    [RelayCommand]
    public virtual void AddRecord()
    {
        NavigateToAddRecord?.Invoke();
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDateFilter = "All Dates";
        SelectedStatusFilter = "All Status";
        SelectedProcessFilter = "All Process";
        SelectedBranchFilter = "All Branch";
        SpecificDate = null;
        MonthDate = DateTime.Now;
        SelectedMonth = null;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
        ApplyFilters();
    }

    [RelayCommand]
    public virtual void Details(AuditRecord record)
    {
        if (NavigateToDetail != null)
        {
            NavigateToDetail.Invoke(record);
        }
        else
        {
            // Fallback for modules that haven't implemented full-page detail yet
            SelectedRecord = record;
            IsEditMode = false;
            IsDrawerOpen = true;
            RefreshDisplayProperties();
        }
    }
}
