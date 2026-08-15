using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AATS.Desktop.ViewModels.Clients;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly List<ClientRecord> _allClients = new();
    
    [ObservableProperty] private ObservableCollection<ClientRecord> _filteredClients = new();
    [ObservableProperty] private string _searchText = string.Empty;

    // Filter Collections (Standardized design sync)
    public ObservableCollection<string> CategoryFilters { get; } = new() { "All Categories", "Active", "Black Listed", "Suspended" };
    public List<string> Categories { get; } = new() { "Active", "Black Listed", "Suspended" };
    public ObservableCollection<string> StatusFilters { get; } = new() { "All Statuses", "Active", "Inactive" };
    public List<string> Statuses { get; } = new() { "Active", "Inactive" };
    
    [ObservableProperty] private string _selectedCategoryFilter = "All Categories";
    [ObservableProperty] private string _selectedStatusFilter = "All Statuses";
    [ObservableProperty] private ObservableCollection<string> _branchFilters = new() { "All Branches" };
    [ObservableProperty] private string _selectedBranchFilter = "All Branches";
    public ObservableCollection<Branch> AvailableBranches { get; } = new();

    // Date Filters (Standardized design sync)
    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    public ObservableCollection<string> Years { get; } = new() { "2023", "2024", "2025", "2026", "2027", "2028" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedDateFilter = "All Dates";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _specificDate;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private DateTime? _monthDate = DateTime.Now;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DateFilterSummary))] private string? _selectedYear;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _periodStartDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    private DateTime? _periodEndDate;

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

    public bool HasActiveFilters => SelectedDateFilter != "All Dates" || SelectedCategoryFilter != "All Categories" || SelectedStatusFilter != "All Statuses" || !string.IsNullOrWhiteSpace(SearchText);

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedDateFilter = "All Dates";
        SelectedCategoryFilter = "All Categories";
        SelectedStatusFilter = "All Statuses";
        SearchText = string.Empty;
        ApplyFilter();
    }

    // Pagination State (Standardized design sync)
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

    private List<ClientRecord> _filteredSource = new();

    // Selection Tracking
    [ObservableProperty] private int _selectedRecordCount = 0;
    [ObservableProperty] private bool _hasSelectedRecords = false;
    [ObservableProperty] private bool _isAllSelected = false;

    // Sidebar Detail
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(DisplayClientName))]
    [NotifyPropertyChangedFor(nameof(DisplayId))]
    [NotifyPropertyChangedFor(nameof(DisplayStatus))]
    private ClientRecord? _selectedClient;
    [ObservableProperty] private bool _isDetailVisible = false;
    [ObservableProperty] private bool _isEditMode = false;

    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private string _guideLinkText = "Learn more about Clients";

    // Editable fields for Edit Mode
    [ObservableProperty] private string _editClientName = string.Empty;
    [ObservableProperty] private string _editStatus = string.Empty;
    [ObservableProperty] private string _editEmail = string.Empty;
    [ObservableProperty] private string _editPhone = string.Empty;
    [ObservableProperty] private string _editAssessmentYear = string.Empty;
    [ObservableProperty] private string _editCurrentPeriod = string.Empty;
    [ObservableProperty] private string _editAuditorNotes = string.Empty;
    [ObservableProperty] private string _editDirectorId = string.Empty;
    [ObservableProperty] private string _editTin = string.Empty;
    [ObservableProperty] private string _editCategory = string.Empty;

    // Display fields (read-only mode)
    public string DisplayClientName => SelectedClient?.Name ?? "N/A";
    public string DisplayId => !string.IsNullOrEmpty(SelectedClient?.ClientCode) ? SelectedClient.ClientCode : (SelectedClient != null ? "CL-" + SelectedClient.Id?.Substring(0, 5).ToUpper() : "N/A");
    public string DisplayStatus => SelectedClient?.Status ?? "UNKNOWN";
    public string DisplayAssessmentYear => SelectedClient?.Category ?? "N/A"; // Using Category as placeholder if needed, or adjust
    public string DisplayCurrentPeriod => SelectedClient?.Phone ?? string.Empty;
    public string DisplayAuditorNotes => SelectedClient?.Email ?? string.Empty; // Just placeholders for now
    public string DisplayDirectorId => "DIR-8829"; // Placeholder
    public string DisplayTin => "TIN-9921-X"; // Placeholder

    // Confirmation Flags
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private bool _isDiscardConfirmVisible;
    [ObservableProperty] private bool _isSaveConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;
    private bool _isBulkDelete;
    private ClientRecord? _clientToDelete;

    public Action? NavigateToAddClient { get; set; }
    public Action<ClientRecord>? NavigateToDetail { get; set; }

    public ClientsViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var branchesTask = DataService.Instance.GetBranchesAsync();
            var clientsTask = DataService.Instance.GetClientsAsync();

            await Task.WhenAll(branchesTask, clientsTask);

            var branches = await branchesTask;
            AvailableBranches.Clear();
            BranchFilters.Clear();
            BranchFilters.Add("All Branches");
            foreach (var b in branches)
            {
                AvailableBranches.Add(b);
                BranchFilters.Add(b.Name);
            }
            SelectedBranchFilter = "All Branches";

            var clients = await clientsTask;
            _allClients.Clear();
            foreach (var client in clients)
            {
                client.PropertyChanged += OnRecordPropertyChanged;
                _allClients.Add(client);
            }
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading clients data: {ex.Message}");
        }
    }


    private void OnRecordPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClientRecord.IsSelected)) UpdateSelectionStatus();
    }

    [RelayCommand] private void Search() => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedCategoryFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilter();

    partial void OnSelectedDateFilterChanged(string value) => ApplyFilter();
    partial void OnSpecificDateChanged(DateTime? value) => ApplyFilter();
    partial void OnMonthDateChanged(DateTime? value) => ApplyFilter();
    partial void OnSelectedYearChanged(string? value) => ApplyFilter();
    partial void OnPeriodStartDateChanged(DateTime? value) => ApplyFilter();
    partial void OnPeriodEndDateChanged(DateTime? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var results = _allClients.AsEnumerable();

        // Date Filtering logic
        if (SelectedDateFilter != "All Dates")
        {
            var now = DateTime.Now;
            if (SelectedDateFilter == "Today")
            {
                results = results.Where(c => c.Date.Date == now.Date);
            }
            else if (SelectedDateFilter == "This Week")
            {
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                results = results.Where(c => c.Date.Date >= startOfWeek.Date);
            }
            else if (SelectedDateFilter == "This Month" && MonthDate.HasValue)
            {
                results = results.Where(c => c.Date.Month == MonthDate.Value.Month && c.Date.Year == MonthDate.Value.Year);
            }
            else if (SelectedDateFilter == "This Year" && !string.IsNullOrEmpty(SelectedYear))
            {
                if (int.TryParse(SelectedYear, out int year))
                    results = results.Where(c => c.Date.Year == year);
            }
            else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
            {
                results = results.Where(c => c.Date.Date == SpecificDate.Value.Date);
            }
            else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
            {
                results = results.Where(c => c.Date.Date >= PeriodStartDate.Value.Date && c.Date.Date <= PeriodEndDate.Value.Date);
            }
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var lowerFilter = SearchText.ToLower();
            results = results.Where(c => 
                (c.Name?.ToLower().Contains(lowerFilter) ?? false) ||
                (c.Email?.ToLower().Contains(lowerFilter) ?? false) ||
                (c.Id?.ToLower().Contains(lowerFilter) ?? false)
            );
        }

        if (SelectedCategoryFilter != "All Categories")
            results = results.Where(c => c.Category == SelectedCategoryFilter);

        if (SelectedBranchFilter != "All Branches")
            results = results.Where(c => c.Branch == SelectedBranchFilter);

        if (SelectedStatusFilter != "All Statuses")
            results = results.Where(c => c.Status == SelectedStatusFilter);

        _filteredSource = results.OrderBy(c => c.ClientCode).ToList();
        CurrentPage = 1;
        UpdatePagination();
    }

    private void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(_filteredSource.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        FilteredClients.Clear();
        var pageRecords = _filteredSource.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var r in pageRecords)
        {
            FilteredClients.Add(r);
        }
        UpdateSelectionStatus();
    }

    private void UpdateSelectionStatus()
    {
        SelectedRecordCount = FilteredClients.Count(c => c.IsSelected);
        HasSelectedRecords = SelectedRecordCount > 0;
    }

    [RelayCommand] private void AddNewClient() => NavigateToAddClient?.Invoke();

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var client in FilteredClients) client.IsSelected = false;
        IsAllSelected = false;
        UpdateSelectionStatus();
    }

    [RelayCommand]
    private void ToggleAllSelection()
    {
        foreach (var client in FilteredClients) client.IsSelected = IsAllSelected;
        UpdateSelectionStatus();
    }

    [RelayCommand]
    private void OpenClientDetail(ClientRecord client)
    {
        if (client != null)
        {
            NavigateToDetail?.Invoke(client);
        }
    }

    [RelayCommand]
    private void CloseClientDetail()
    {
        IsDetailVisible = false;
        IsEditMode = false;
        SelectedClient = null;
    }

    [RelayCommand]
    private void EnterEditMode()
    {
        if (SelectedClient == null) return;

        Action proceed = () =>
        {
            EditClientName = SelectedClient.Name ?? string.Empty;
            EditStatus = SelectedClient.Status ?? "UNKNOWN";
            EditEmail = SelectedClient.Email ?? string.Empty;
            EditPhone = SelectedClient.Phone ?? string.Empty;
            EditCategory = SelectedClient.Category ?? "Loyal";
            // Map other fields as needed
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
    private void CancelEdit()
    {
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task SaveEdit()
    {
        if (SelectedClient == null) return;
        
        HasFormError = false;

        if (!ValidationHelper.IsValidName(EditClientName))
        {
            FormErrorMessage = "Please enter a valid client name.";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidEmail(EditEmail))
        {
            FormErrorMessage = "Please enter a valid email address.";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidPhone(EditPhone))
        {
            FormErrorMessage = "Please enter a valid phone number.";
            HasFormError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(EditCategory))
        {
            FormErrorMessage = "Please enter or select a category.";
            HasFormError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(EditStatus))
        {
            FormErrorMessage = "Please select a status.";
            HasFormError = true;
            return;
        }

        SelectedClient.Name = EditClientName;
        SelectedClient.Status = EditStatus;
        SelectedClient.Email = EditEmail;
        SelectedClient.Phone = EditPhone;
        SelectedClient.Category = EditCategory;
        
        try
        {
            await DataService.Instance.UpdateClientAsync(SelectedClient);
            
            // Notify UI about changes in Display properties
            OnPropertyChanged(nameof(DisplayClientName));
            OnPropertyChanged(nameof(DisplayStatus));
            OnPropertyChanged(nameof(DisplayAuditorNotes)); // Placeholder for email
            OnPropertyChanged(nameof(DisplayCurrentPeriod)); // Placeholder for phone
            
            IsEditMode = false;
            
            LogService.Instance.AddLog("Update", "Clients", SelectedClient.Branch ?? "Central", $"Updated details for client: {SelectedClient.Name}");
        }
        catch (Exception ex)
        {
            FormErrorMessage = $"Error updating client: {ex.Message}";
            HasFormError = true;
        }
    }

    [RelayCommand] private void OpenGuide() => IsGuideVisible = true;
    [RelayCommand] private void CloseGuide() => IsGuideVisible = false;

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

    // Add New Client Logic
    [RelayCommand]
    private void OpenAddClient()
    {
        NavigateToAddClient?.Invoke();
    }

    [RelayCommand]
    private void SaveClientDetails()
    {
        if (SelectedClient == null) return;
        
        LogService.Instance.AddLog("Update", "Clients", SelectedClient.Branch ?? "Central", $"Updated details for client: {SelectedClient.Name}");
        CloseClientDetail();
    }

    [RelayCommand]
    private void ExportClients()
    {
        var count = HasSelectedRecords ? SelectedRecordCount : FilteredClients.Count;
        string target = HasSelectedRecords ? "selected records" : "current list";
        LogService.Instance.AddLog("Export", "Clients", "Central", $"Exported {count} {target} to Excel.");
    }

    [RelayCommand]
    private void PrintClients()
    {
        var count = HasSelectedRecords ? SelectedRecordCount : FilteredClients.Count;
        string target = HasSelectedRecords ? "selected records" : "current list";
        LogService.Instance.AddLog("Print", "Clients", "Central", $"Generated print report for {count} {target}.");
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var count = FilteredClients.Count(c => c.IsSelected);
        if (count == 0) return;
        _isBulkDelete = true;
        DeleteConfirmMessage = $"Are you sure you want to delete {count} selected client(s)? This action cannot be undone.";
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void DeleteCurrentClient()
    {
        if (SelectedClient == null) return;
        _isBulkDelete = false;
        _clientToDelete = SelectedClient;
        DeleteConfirmMessage = $"Are you sure you want to delete client '{SelectedClient.Name}'? This action cannot be undone.";
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        IsDeleteConfirmVisible = false;
        if (_isBulkDelete)
        {
            var selected = _allClients.Where(c => c.IsSelected).ToList();
            foreach (var client in selected) _allClients.Remove(client);
            _ = DataService.Instance.DeleteClientsAsync(selected);
        }
        else if (_clientToDelete != null)
        {
            string clientName = _clientToDelete.Name ?? "Unknown";
            _allClients.Remove(_clientToDelete);
            
            _ = DataService.Instance.DeleteClientsAsync(new[] { _clientToDelete });

            LogService.Instance.AddLog("Delete", "Clients", _clientToDelete.Branch ?? "N/A", $"Deleted client: {clientName}");
            CloseClientDetail();
        }
        _clientToDelete = null;

        IsAllSelected = false;
        SelectedRecordCount = 0;
        HasSelectedRecords = false;

        ApplyFilter();
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmVisible = false;
        _clientToDelete = null;
    }
}
