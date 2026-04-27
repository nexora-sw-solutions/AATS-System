using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AATS.Desktop.Helpers;
using AATS.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.ViewModels.Nexora;
using AATS.Desktop.Services;
using System.Threading.Tasks;

namespace AATS.Desktop.ViewModels.Team;

public partial class TeamViewModel : ViewModelBase
{
    private readonly List<TeamMember> _allMembers = new();
    private List<TeamMember> _filteredSource = new();
    
    [ObservableProperty] private ObservableCollection<TeamMember> _filteredMembers = new();
    [ObservableProperty] private string _searchText = string.Empty;

    // Filter Collections
    public ObservableCollection<string> RoleFilters { get; } = new() { "All Roles", "Admin", "Staff" };
    public ObservableCollection<string> BranchFilters { get; } = new() { "All Branches", "Central", "South", "West", "Northeast" };
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedRoleFilter = "All Roles";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedBranchFilter = "All Branches";

    // Date Filtering Properties
    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedDateFilter = "All Dates";

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

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchText) || 
                                   SelectedDateFilter != "All Dates" || 
                                   SelectedRoleFilter != "All Roles" || 
                                   SelectedBranchFilter != "All Branches";

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

    // Selection Tracking
    [ObservableProperty] private int _selectedRecordCount = 0;
    [ObservableProperty] private bool _hasSelectedRecords = false;
    [ObservableProperty] private bool _isAllSelected = false;

    // Detail Panel
    [ObservableProperty] private TeamMember? _selectedMember;
    [ObservableProperty] private bool _isDetailVisible = false;

    public Action? NavigateToAddMember { get; set; }

    // Add Member Modal State
    [ObservableProperty] private bool _isAddMemberModalOpen = false;
    [ObservableProperty] private bool _isEditMode = false;
    
    // Instruction Guide State
    [ObservableProperty] private bool _isGuideVisible = false;
    [ObservableProperty] private string _guideLinkText = "Learn more about Team";
    
    [ObservableProperty] private string _modalTitle = "Add New Member";
    [ObservableProperty] private string _modalDescription = "Create a new access profile for your team.";
    [ObservableProperty] private string _saveButtonText = "Save Member";

    [ObservableProperty] private string _newMemberUsername = string.Empty;
    [ObservableProperty] private string _newMemberEmail = string.Empty;
    [ObservableProperty] private string _newMemberPhone = string.Empty;
    [ObservableProperty] private string _newMemberRole = "Select Role";
    [ObservableProperty] private string _newMemberBranch = "Select Branch";
    
    [ObservableProperty] private string _newMemberPassword = string.Empty;
    [ObservableProperty] private string _newMemberConfirmPassword = string.Empty;
    [ObservableProperty] private string _editCurrentPassword = string.Empty;

    // Validation errors are inherited from ViewModelBase

    // Confirmation Flags
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private bool _isDiscardConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;
    private TeamMember? _memberToDelete;

    public ObservableCollection<string> AvailableRoles => new(RoleFilters.Where(r => r != "All Roles"));
    public ObservableCollection<string> AvailableBranches => new(BranchFilters.Where(b => b != "All Branches"));
    public TeamViewModel()
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var members = await DataService.Instance.GetTeamMembersAsync();
        _allMembers.Clear();
        _allMembers.AddRange(members);
        ApplyFilter();
    }


    [RelayCommand] private void Search() => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedRoleFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedBranchFilterChanged(string value) => ApplyFilter();
    partial void OnSelectedDateFilterChanged(string value) => ApplyFilter();
    partial void OnSpecificDateChanged(DateTime? value) => ApplyFilter();
    partial void OnMonthDateChanged(DateTime? value) => ApplyFilter();
    partial void OnSelectedYearChanged(string? value) => ApplyFilter();
    partial void OnPeriodStartDateChanged(DateTime? value) => ApplyFilter();
    partial void OnPeriodEndDateChanged(DateTime? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var results = _allMembers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var lowerFilter = SearchText.ToLower();
            results = results.Where(m => 
                (m.Username?.ToLower().Contains(lowerFilter) ?? false) ||
                (m.Email?.ToLower().Contains(lowerFilter) ?? false)
            );
        }

        if (SelectedRoleFilter != "All Roles")
            results = results.Where(m => m.Role == SelectedRoleFilter);

        if (SelectedBranchFilter != "All Branches")
            results = results.Where(m => m.Branch == SelectedBranchFilter);

        DateTime now = DateTime.Now;
        if (SelectedDateFilter == "Today")
        {
            results = results.Where(m => m.CreatedAt.Date == now.Date);
        }
        else if (SelectedDateFilter == "This Week")
        {
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            results = results.Where(m => m.CreatedAt.Date >= startOfWeek.Date && m.CreatedAt.Date <= now.Date);
        }
        else if (SelectedDateFilter == "This Month")
        {
            int targetMonth = MonthDate?.Month ?? now.Month;
            int targetYear = MonthDate?.Year ?? now.Year;
            results = results.Where(m => m.CreatedAt.Month == targetMonth && m.CreatedAt.Year == targetYear);
        }
        else if (SelectedDateFilter == "This Year")
        {
            int year = !string.IsNullOrEmpty(SelectedYear) ? int.Parse(SelectedYear) : now.Year;
            results = results.Where(m => m.CreatedAt.Year == year);
        }
        else if (SelectedDateFilter == "Specific Date" && SpecificDate.HasValue)
        {
            results = results.Where(m => m.CreatedAt.Date == SpecificDate.Value.Date);
        }
        else if (SelectedDateFilter == "Specific Period" && PeriodStartDate.HasValue && PeriodEndDate.HasValue)
        {
            results = results.Where(m => m.CreatedAt.Date >= PeriodStartDate.Value.Date && m.CreatedAt.Date <= PeriodEndDate.Value.Date);
        }

        _filteredSource = results.ToList();
        CurrentPage = 1;
        UpdatePagination();
    }

    private void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(_filteredSource.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        FilteredMembers.Clear();
        var pageRecords = _filteredSource.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        foreach (var r in pageRecords)
        {
            FilteredMembers.Add(r);
        }
        UpdateSelectionStatus();
    }

    private void UpdateSelectionStatus()
    {
        SelectedRecordCount = FilteredMembers.Count(m => m.IsSelected);
        HasSelectedRecords = SelectedRecordCount > 0;
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

    [RelayCommand]
    private void OpenEditMember(TeamMember member)
    {
        SelectedMember = member;

        IsEditMode = true;
        ModalTitle = "Edit Member Profile";
        ModalDescription = "Update the details for this team member.";
        SaveButtonText = "Update Member";

        // Pre-populate
        NewMemberUsername = member.Username ?? "";
        NewMemberEmail = member.Email ?? "";
        NewMemberPhone = member.Phone ?? "";
        NewMemberRole = member.Role ?? "Select Role";
        NewMemberBranch = member.Branch ?? "Select Branch";

        NewMemberPassword = string.Empty;
        NewMemberConfirmPassword = string.Empty;
        EditCurrentPassword = string.Empty;
        
        HasFormError = false;
        FormErrorMessage = string.Empty;

        IsAddMemberModalOpen = true;
    }

    [RelayCommand]
    private void CloseMemberDetail()
    {
        IsDetailVisible = false;
        SelectedMember = null;
    }

    [RelayCommand]
    private void OpenGuide()
    {
        IsGuideVisible = true;
    }

    [RelayCommand]
    private void CloseGuide()
    {
        IsGuideVisible = false;
    }

    [RelayCommand] 
    private void AddNewMember() 
    {
        IsEditMode = false;
        ModalTitle = "Add New Member";
        ModalDescription = "Create a new access profile for your team.";
        SaveButtonText = "Save Member";

        HasFormError = false;
        FormErrorMessage = string.Empty;
        NewMemberUsername = string.Empty;
        NewMemberEmail = string.Empty;
        NewMemberPhone = string.Empty;
        NewMemberRole = "Select Role";
        NewMemberBranch = "Select Branch";
        NewMemberPassword = string.Empty;
        NewMemberConfirmPassword = string.Empty;
        EditCurrentPassword = string.Empty;
        
        SelectedMember = null;
        IsAddMemberModalOpen = true;
    }

    [RelayCommand]
    private void CloseAddMemberModal()
    {
        IsDiscardConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmDiscard()
    {
        IsDiscardConfirmVisible = false;
        IsAddMemberModalOpen = false;
    }

    [RelayCommand]
    private void CancelDiscard()
    {
        IsDiscardConfirmVisible = false;
    }

    [RelayCommand]
    private void SaveNewMember()
    {
        if (!ValidationHelper.IsValidName(NewMemberUsername))
        {
            FormErrorMessage = "Please enter a valid username (at least 2 characters).";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidEmail(NewMemberEmail))
        {
            FormErrorMessage = "Please enter a valid email address.";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidPhone(NewMemberPhone))
        {
            FormErrorMessage = "Please enter a valid phone number (at least 9 digits).";
            HasFormError = true;
            return;
        }

        if (NewMemberRole == "Select Role" || NewMemberBranch == "Select Branch")
        {
            FormErrorMessage = "Please select a Role and Branch.";
            HasFormError = true;
            return;
        }

        if (IsEditMode)
        {
            if (!string.IsNullOrWhiteSpace(NewMemberPassword))
            {
                if (string.IsNullOrWhiteSpace(EditCurrentPassword))
                {
                    FormErrorMessage = "Please enter Current Password to update the password.";
                    HasFormError = true;
                    return;
                }
                if (NewMemberPassword != NewMemberConfirmPassword)
                {
                    FormErrorMessage = "Passwords do not match.";
                    HasFormError = true;
                    return;
                }
            }
            
            // Execute Update
            if (SelectedMember != null)
            {
                SelectedMember.Username = NewMemberUsername;
                SelectedMember.Email = NewMemberEmail;
                SelectedMember.Phone = NewMemberPhone;
                SelectedMember.Branch = NewMemberBranch;
                SelectedMember.Role = NewMemberRole;
                
                _ = DataService.Instance.UpdateTeamMemberAsync(SelectedMember);

                LogService.Instance.AddLog("Update", "Team", NewMemberBranch, $"Updated profile for member: {NewMemberUsername}");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(NewMemberPassword) || NewMemberPassword != NewMemberConfirmPassword)
            {
                FormErrorMessage = string.IsNullOrWhiteSpace(NewMemberPassword) ? "Please enter a Password." : "Passwords do not match.";
                HasFormError = true;
                return;
            }

            // Successfully validated - Create member
            var newMember = new TeamMember 
            { 
                Id = $"TM-{_allMembers.Count + 100:D3}", // Unique ID generation
                Username = NewMemberUsername, 
                Email = NewMemberEmail, 
                Phone = NewMemberPhone, 
                Branch = NewMemberBranch, 
                Role = NewMemberRole, 
                CreatedAt = DateTime.Now 
            };
            _allMembers.Insert(0, newMember);
            
            _ = DataService.Instance.AddTeamMemberAsync(newMember);

            LogService.Instance.AddLog("Create", "Team", NewMemberBranch, $"Created new member account: {NewMemberUsername}");
        }

        IsAddMemberModalOpen = false;
        ApplyFilter(); // Refresh table
    }
    [RelayCommand]
    private void ToggleAllSelection()
    {
        foreach (var member in FilteredMembers) member.IsSelected = IsAllSelected;
        UpdateSelectionStatus();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var member in FilteredMembers) member.IsSelected = false;
        IsAllSelected = false;
        UpdateSelectionStatus();
    }

    [RelayCommand]
    private void ExportTeam()
    {
        var count = FilteredMembers.Count;
        LogService.Instance.AddLog("Export", "Team", SelectedBranchFilter == "All Branches" ? "Central" : SelectedBranchFilter, $"Exported team list ({count} members) to Excel.");
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDateFilter = "All Dates";
        SelectedRoleFilter = "All Roles";
        SelectedBranchFilter = "All Branches";
        SpecificDate = null;
        MonthDate = DateTime.Now;
        SelectedYear = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
        ApplyFilter();
    }

    [RelayCommand]
    private void PrintTeam()
    {
        var count = FilteredMembers.Count;
        LogService.Instance.AddLog("Print", "Team", SelectedBranchFilter == "All Branches" ? "Central" : SelectedBranchFilter, $"Generated print report for team members ({count} records).");
    }

    [RelayCommand] private void DeleteSelected() { }

    [RelayCommand]
    private void DeleteMember(TeamMember member)
    {
        if (member == null) return;
        _memberToDelete = member;
        DeleteConfirmMessage = $"Are you sure you want to delete member '{member.Username}'? This action cannot be undone.";
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        IsDeleteConfirmVisible = false;
        if (_memberToDelete != null && _allMembers.Contains(_memberToDelete))
        {
            string deletedName = _memberToDelete.Username ?? "Unknown";
            string branch = _memberToDelete.Branch ?? "Central";
            
            _allMembers.Remove(_memberToDelete);
            _ = DataService.Instance.DeleteTeamMembersAsync(new[] { _memberToDelete });
            
            LogService.Instance.AddLog("Delete", "Team", branch, $"Removed member: {deletedName}");
            _memberToDelete = null;
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmVisible = false;
        _memberToDelete = null;
    }
}
