using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.Models;
using AATS.Desktop.Services;

namespace AATS.Desktop.ViewModels.Shared;

public abstract partial class AuditTableViewModelBase : ViewModelBase
{
    // Page Customization Properties
    public abstract string PageTitle { get; }
    public abstract string SearchPlaceholder { get; }
    public abstract string StatusHeader { get; }
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

    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Specific Date", "Month", "Year", "Period" };
    public ObservableCollection<string> StatusFilters { get; } = new() { "All Status", "Paid", "Unpaid", "Partial" };
    public ObservableCollection<string> BranchFilters { get; } = new() { "All Branch", "South", "West", "Central", "Northeast" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpecificDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsMonthVisible))]
    [NotifyPropertyChangedFor(nameof(IsYearVisible))]
    [NotifyPropertyChangedFor(nameof(IsPeriodVisible))]
    [NotifyPropertyChangedFor(nameof(IsClearDateFilterVisible))]
    private string _selectedDateFilter = "All Dates";

    [ObservableProperty] private string _selectedStatusFilter = "All Status";
    [ObservableProperty] private string _selectedProcessFilter = "All Process";
    [ObservableProperty] private string _selectedBranchFilter = "All Branch";
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isGuideVisible = false;

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

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
    public string DisplayId => SelectedRecord?.ID ?? "N/A";
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
        IsDrawerOpen = false;
        IsEditMode = false;
        SelectedRecord = null;
    }

    [RelayCommand]
    public virtual void EnterEditMode()
    {
        if (SelectedRecord == null) return;
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
    private DateTime? _specificDate;
    [ObservableProperty] private string? _selectedMonth;
    [ObservableProperty] private string? _selectedYear;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodStartDate;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodEndDate;

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
        
        await DataService.Instance.DeleteAuditRecordsAsync(PageTitle, toDelete);
        
        foreach (var d in toDelete)
            _allRecords.Remove(d);
            
        ApplyFilters();
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
    public bool IsMonthVisible => SelectedDateFilter == "Month";
    public bool IsYearVisible => SelectedDateFilter == "Month" || SelectedDateFilter == "Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Period";
    public bool IsClearDateFilterVisible => SelectedDateFilter != "All Dates";

    public ObservableCollection<string> Months { get; } = new() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public ObservableCollection<string> Years { get; } = new() { "2024", "2025", "2026", "2027", "2028" };

    [RelayCommand]
    private void ClearDateFilter()
    {
        SelectedDateFilter = "All Dates";
        SpecificDate = null;
        SelectedMonth = null;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
    }

    public AuditTableViewModelBase()
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var records = await DataService.Instance.GetAuditRecordsAsync(PageTitle);
        _allRecords.Clear();
        _allRecords.AddRange(records);
        ApplyFilters();
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
            if (!string.IsNullOrWhiteSpace(SearchText) && 
                !(record.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) && 
                !(record.ID?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            if (SelectedStatusFilter != "All Status" && record.PaymentStatus != SelectedStatusFilter)
                continue;

            if (SelectedProcessFilter != "All Process" && !(record.Process?.Equals(SelectedProcessFilter, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            if (SelectedBranchFilter != "All Branch" && record.Branch != SelectedBranchFilter)
                continue;

            if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
            {
                if (record.Date.Date != SpecificDate.Value.Date) continue;
            }
            else if (SelectedDateFilter == "Month" && !string.IsNullOrEmpty(SelectedMonth) && !string.IsNullOrEmpty(SelectedYear))
            {
                int monthIndex = Months.IndexOf(SelectedMonth) + 1;
                if (record.Date.Month != monthIndex || record.Date.Year.ToString() != SelectedYear) continue;
            }
            else if (SelectedDateFilter == "Year" && !string.IsNullOrEmpty(SelectedYear))
            {
                if (record.Date.Year.ToString() != SelectedYear) continue;
            }
            else if (SelectedDateFilter == "Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
            {
                if (record.Date.Date < PeriodStartDate.Value.Date || record.Date.Date > PeriodEndDate.Value.Date) continue;
            }

            _filteredRecords.Add(record);
        }
        
        CurrentPage = 1;
        UpdatePagination();
    }

    // Handlers
    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedProcessFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedBranchFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedDateFilterChanged(string value) => ApplyFilters();
    partial void OnSpecificDateChanged(DateTime? value) => ApplyFilters();
    partial void OnSelectedMonthChanged(string? value) => ApplyFilters();
    partial void OnSelectedYearChanged(string? value) => ApplyFilters();
    partial void OnPeriodStartDateChanged(DateTime? value) => ApplyFilters();
    partial void OnPeriodEndDateChanged(DateTime? value) => ApplyFilters();

    // The Navigation Hook to be handled uniquely by subclasses (or MainViewModel) depending on app routing architecture.
    public Action? NavigateToAddRecord { get; set; }
    public Action<AuditRecord>? NavigateToDetail { get; set; }

    [RelayCommand]
    public virtual void AddRecord()
    {
        NavigateToAddRecord?.Invoke();
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
