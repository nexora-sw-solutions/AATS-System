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
    public ObservableCollection<string> RoleFilters { get; } = new() { "All Roles", "Admin", "Audit and Assurance", "Secretarial and Advisory", "Tax Filing", "All" };
    public ObservableCollection<string> StatusFilters { get; } = new() { "All Statuses", "Active Only", "Deleted / Trash", "Active", "Inactive" };
    [ObservableProperty] private ObservableCollection<string> _branchFilters = new() { "All Branches" };
    [ObservableProperty] private ObservableCollection<Branch> _branches = new();
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedRoleFilter = "All Roles";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedBranchFilter = "All Branches";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasActiveFilters))] private string _selectedStatusFilter = "All Statuses";

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
    [ObservableProperty] private Branch? _selectedNewMemberBranch;
    
    [ObservableProperty] private string _newMemberPassword = string.Empty;
    [ObservableProperty] private string _newMemberConfirmPassword = string.Empty;
    [ObservableProperty] private char _passwordChar = '•';
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private char _confirmPasswordChar = '•';
    [ObservableProperty] private bool _isConfirmPasswordVisible;
    // Validation errors are inherited from ViewModelBase

    // Confirmation Flags
    [ObservableProperty] private bool _isDeleteConfirmVisible;
    [ObservableProperty] private bool _isDiscardConfirmVisible;
    [ObservableProperty] private string _deleteConfirmMessage = string.Empty;
    private TeamMember? _memberToDelete;

    public ObservableCollection<string> AvailableRoles => new(RoleFilters.Where(r => r != "All Roles"));
    public ObservableCollection<Branch> AvailableBranches => Branches;
    [ObservableProperty] private ObservableCollection<TeamMember> _deletedTeamMembers = new();
    [ObservableProperty] private bool _isTrashExpanded;

    public TeamViewModel()
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var branchesTask = DataService.Instance.GetBranchesAsync();
            var membersTask = DataService.Instance.GetTeamMembersAsync();
            var deletedMembersTask = DataService.Instance.GetDeletedTeamMembersAsync();

            await Task.WhenAll(branchesTask, membersTask, deletedMembersTask);

            var branches = await branchesTask;
            Branches.Clear();
            BranchFilters.Clear();
            BranchFilters.Add("All Branches");
            foreach (var b in branches)
            {
                Branches.Add(b);
                BranchFilters.Add(b.Name);
            }

            SelectedBranchFilter = "All Branches";

            var members = await membersTask;
            _allMembers.Clear();
            _allMembers.AddRange(members);

            var deleted = await deletedMembersTask;
            DeletedTeamMembers.Clear();
            foreach (var d in deleted)
            {
                DeletedTeamMembers.Add(d);
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading team data: {ex.Message}");
        }
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

        if (SelectedStatusFilter == "Active Only")
            results = results.Where(m => !m.IsDeleted);
        else if (SelectedStatusFilter == "Deleted / Trash")
            results = results.Where(m => m.IsDeleted);
        else if (SelectedStatusFilter != "All Statuses")
            results = results.Where(m => m.Status == SelectedStatusFilter);

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
        SelectedNewMemberBranch = Branches.FirstOrDefault(b => b.Name == member.Branch || b.Id == member.BranchId);

        NewMemberPassword = string.Empty;
        NewMemberConfirmPassword = string.Empty;
        
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
        SelectedNewMemberBranch = null;
        NewMemberPassword = string.Empty;
        NewMemberConfirmPassword = string.Empty;
        
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
    private async System.Threading.Tasks.Task SaveNewMember()
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

        if (NewMemberRole == "Select Role" || SelectedNewMemberBranch == null)
        {
            FormErrorMessage = "Please select a Role and Branch.";
            HasFormError = true;
            return;
        }

        if (IsEditMode)
        {
            if (!string.IsNullOrWhiteSpace(NewMemberPassword))
            {if (NewMemberPassword != NewMemberConfirmPassword)
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
                SelectedMember.Branch = SelectedNewMemberBranch?.Name;
                SelectedMember.BranchId = SelectedNewMemberBranch?.Id ?? Guid.Empty;
                SelectedMember.Role = NewMemberRole;
                
                // Only send password if a new one was entered
                SelectedMember.Password = !string.IsNullOrWhiteSpace(NewMemberPassword) ? NewMemberPassword : null;
                
                try 
                {
                    await DataService.Instance.UpdateTeamMemberAsync(SelectedMember);
                    LogService.Instance.AddLog("Update", "Team", SelectedMember.Branch ?? "Central", $"Updated profile for member: {NewMemberUsername}");
                }
                catch (Exception ex)
                {
                    FormErrorMessage = "Error updating member: " + ex.Message;
                    HasFormError = true;
                    return;
                }
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
                Branch = SelectedNewMemberBranch?.Name,
                BranchId = SelectedNewMemberBranch?.Id ?? Guid.Empty,
                Role = NewMemberRole, 
                Password = NewMemberPassword,
                CreatedAt = DateTime.Now 
            };
            _allMembers.Insert(0, newMember);
            
            try 
            {
                await DataService.Instance.AddTeamMemberAsync(newMember);
                LogService.Instance.AddLog("Create", "Team", newMember.Branch ?? "Central", $"Created new member account: {NewMemberUsername}");
            }
            catch (Exception ex)
            {
                FormErrorMessage = "Error creating member: " + ex.Message;
                HasFormError = true;
                _allMembers.Remove(newMember); // Rollback local change
                return;
            }
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

            IsAllSelected = false;
            SelectedRecordCount = 0;
            HasSelectedRecords = false;

            ApplyFilter();
            _ = LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RestoreTeamMember(TeamMember? member)
    {
        if (member == null || string.IsNullOrEmpty(member.Id)) return;
        bool success = await DataService.Instance.RestoreTeamMemberAsync(member.Id);
        if (success)
        {
            LogService.Instance.AddLog("Restore", "Team", member.Branch ?? "Central", $"Restored soft-deleted member: {member.Username}");
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task PermanentlyDeleteTeamMember(TeamMember? member)
    {
        if (member == null || string.IsNullOrEmpty(member.Id)) return;
        bool success = await DataService.Instance.PermanentlyDeleteTeamMemberAsync(member.Id);
        if (success)
        {
            LogService.Instance.AddLog("Purge", "Team", member.Branch ?? "Central", $"Permanently purged member: {member.Username}");
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmVisible = false;
        _memberToDelete = null;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
        PasswordChar = IsPasswordVisible ? (char)0 : '•';
    }

    [RelayCommand]
    private void ToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        ConfirmPasswordChar = IsConfirmPasswordVisible ? (char)0 : '•';
    }}
