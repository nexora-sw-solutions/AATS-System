using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
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
    public ObservableCollection<string> CategoryFilters { get; } = new() { "All Categories", "SME", "Corporate" };
    public ObservableCollection<string> StatusFilters { get; } = new() { "All Statuses", "Active", "Inactive" };
    
    [ObservableProperty] private string _selectedCategoryFilter = "All Categories";
    [ObservableProperty] private string _selectedStatusFilter = "All Statuses";

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
    [ObservableProperty] private ClientRecord? _selectedClient;
    [ObservableProperty] private bool _isDetailVisible = false;

    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private string _guideLinkText = "Learn more about Clients";

    // Add New Client Form State
    [ObservableProperty] private bool _isAddClientVisible = false;
    [ObservableProperty] private string _newClientName = string.Empty;
    [ObservableProperty] private string _newClientEmail = string.Empty;
    [ObservableProperty] private string _newClientPhone = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Confirmation Flags
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private bool _isDiscardConfirmVisible;
    [ObservableProperty] private bool _isSaveConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;
    private bool _isBulkDelete;
    private ClientRecord? _clientToDelete;

    public Action? NavigateToAddClient { get; set; }

    public ClientsViewModel()
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var clients = await DataService.Instance.GetClientsAsync();

        foreach (var existing in _allClients)
        {
            existing.PropertyChanged -= OnRecordPropertyChanged;
        }

        _allClients.Clear();
        foreach (var client in clients)
        {
            client.PropertyChanged += OnRecordPropertyChanged;
            _allClients.Add(client);
        }
        ApplyFilter();
    }


    private void OnRecordPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClientRecord.IsSelected)) UpdateSelectionStatus();
    }

    [RelayCommand] private void Search() => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedCategoryFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var results = _allClients.AsEnumerable();

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

        if (SelectedStatusFilter != "All Statuses")
            results = results.Where(c => c.Status == SelectedStatusFilter);

        _filteredSource = results.ToList();
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
        SelectedClient = client;
        IsDetailVisible = true;
    }

    [RelayCommand]
    private void CloseClientDetail()
    {
        IsDetailVisible = false;
        SelectedClient = null;
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
        NewClientName = string.Empty;
        NewClientEmail = string.Empty;
        NewClientPhone = string.Empty;
        ErrorMessage = string.Empty;
        IsAddClientVisible = true;
    }

    [RelayCommand]
    private void CloseAddClient()
    {
        IsDiscardConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        IsAddClientVisible = false;
    }

    [RelayCommand]
    private void CancelDiscard()
    {
        IsDiscardConfirmVisible = false;
    }

    [RelayCommand]
    private void CancelSaveClient()
    {
        IsSaveConfirmVisible = false;
    }

    [RelayCommand]
    private void SaveNewClient()
    {
        if (!ValidationHelper.IsValidName(NewClientName))
        {
            ErrorMessage = "Client Name must be at least 3 characters.";
            return;
        }
        if (!ValidationHelper.IsValidEmail(NewClientEmail))
        {
            ErrorMessage = "Please enter a valid email address.";
            return;
        }
        if (!ValidationHelper.IsValidPhone(NewClientPhone))
        {
            ErrorMessage = "Phone number must contain at least 10 digits.";
            return;
        }

        ErrorMessage = string.Empty;
        IsSaveConfirmVisible = true;
    }

    [RelayCommand]
    private async Task ConfirmSaveClient()
    {
        var newClient = new ClientRecord
        {
            Name = NewClientName,
            Email = NewClientEmail,
            Phone = NewClientPhone,
            Category = "SME",
            Status = "Active",
            TotalRevenue = 0,
            DueAmount = 0
        };

        await DataService.Instance.AddClientAsync(newClient);
        LogService.Instance.AddLog("Create", "Clients", newClient.Branch ?? "Central", $"Registered new client: {newClient.Name}");
        await LoadDataAsync();
        IsSaveConfirmVisible = false;
        IsAddClientVisible = false;
    }

    [RelayCommand]
    private async Task SaveClientDetails()
    {
        if (SelectedClient == null) return;

        await DataService.Instance.UpdateClientAsync(SelectedClient);
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
    private async Task ConfirmDelete()
    {
        IsDeleteConfirmVisible = false;
        if (_isBulkDelete)
        {
            var selected = _allClients.Where(c => c.IsSelected).ToList();
            await DataService.Instance.DeleteClientsAsync(selected);
        }
        else if (_clientToDelete != null)
        {
            string clientName = _clientToDelete.Name ?? "Unknown";
            await DataService.Instance.DeleteClientsAsync(new[] { _clientToDelete });
            LogService.Instance.AddLog("Delete", "Clients", _clientToDelete.Branch ?? "N/A", $"Deleted client: {clientName}");
            CloseClientDetail();
        }
        _clientToDelete = null;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmVisible = false;
        _clientToDelete = null;
    }
}
