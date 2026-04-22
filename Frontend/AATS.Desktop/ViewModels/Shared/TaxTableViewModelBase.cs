using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AATS.Desktop.ViewModels.Shared;

public abstract partial class TaxTableViewModelBase : ViewModelBase
{
    public TaxTableViewModelBase()
    {
        _ = LoadDataAsync();
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
    public string DisplayId => SelectedRecord?.ID ?? "N/A";
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
    [ObservableProperty] private string? _clientName;
    [ObservableProperty] private string? _directorId; // This is the value for TaxIdLabel
    [ObservableProperty] private string _errorMessage = string.Empty;
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
    [ObservableProperty] private string? _selectedYear;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodStartDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodDisplay))]
    private DateTime? _periodEndDate;

    [ObservableProperty] private string _selectedStatusFilter = "All Status";
    [ObservableProperty] private string _selectedBranchFilter = "All Branch";

    // Date filter visibility
    public bool IsSpecificDateVisible => SelectedDateFilter == "Specific Date";
    public bool IsMonthVisible => SelectedDateFilter == "Month";
    public bool IsYearVisible => SelectedDateFilter == "Month" || SelectedDateFilter == "Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Period";
    public bool IsClearDateFilterVisible => SelectedDateFilter != "All Dates";

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
    private void ClearDateFilter()
    {
        SelectedDateFilter = "All Dates";
        SpecificDate = null;
        SelectedMonth = null;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
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
    public ObservableCollection<string> Years { get; } = new() { "2023", "2024", "2025", "2026", "2027", "2028" };
    public List<string> DurationUnitOptions { get; } = new() { "Days", "Months", "Years" };

    protected void ApplyFilter()
    {
        _filteredRecords.Clear();

        foreach (var record in _allRecords)
        {
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                !(record.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
                !(record.DINNo?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) &&
                !(record.ID?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                continue;

            if (SelectedStatusFilter != "All Status" && 
                !(record.Status?.Equals(SelectedStatusFilter, StringComparison.OrdinalIgnoreCase) ?? false))
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
    partial void OnSelectedMonthChanged(string? value) => ApplyFilter();
    partial void OnSelectedYearChanged(string? value) => ApplyFilter();
    partial void OnPeriodStartDateChanged(DateTime? value) => ApplyFilter();
    partial void OnPeriodEndDateChanged(DateTime? value) => ApplyFilter();

    [RelayCommand]
    protected virtual async Task Submit()
    {
        if (!ValidationHelper.IsValidName(ClientName))
        {
            ErrorMessage = "Client Name must be at least 3 characters.";
            return;
        }
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            ErrorMessage = "Client ID (TIN/VAT) is required.";
            return;
        }

        ErrorMessage = string.Empty;

        var record = new TaxRecord
        {
            ID = ClientId,
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
        ErrorMessage = string.Empty;
        Duration = 1;
        PaymentStatus = "Pending";
    }
}
