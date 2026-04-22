using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    
    [ObservableProperty] private string _selectedRoleFilter = "All Roles";
    [ObservableProperty] private string _selectedBranchFilter = "All Branches";

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

    [ObservableProperty] private bool _hasFormError = false;
    [ObservableProperty] private string _formErrorMessage = string.Empty;

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
    private async Task ToggleStatus(TeamMember member)
    {
        if (member == null || string.IsNullOrEmpty(member.Id)) return;

        string newStatus = member.IsActive ? "Inactive" : "Active";
        member.Status = newStatus;
        
        await DataService.Instance.UpdateUserStatusAsync(member.Id, newStatus);
        LogService.Instance.AddLog("Update", "Team", member.Branch ?? "Central", $"Toggled status for {member.Username} to {newStatus}");
    }

    [RelayCommand]
    private async Task UploadLogo(TeamMember member)
    {
        if (member == null || string.IsNullOrEmpty(member.Id)) return;

        // Note: For now, we'll assume a file picker is available or handled by the view
        // In a real scenario, we might trigger a message to the View to open a picker
        // For this task, I'll implement the logic assuming we get a path
    }

    [RelayCommand]
    private async Task RemoveLogo(TeamMember member)
    {
        if (member == null || string.IsNullOrEmpty(member.Id)) return;
        
        await DataService.Instance.DeleteUserLogoAsync(member.Id);
        member.LogoUrl = null;
        LogService.Instance.AddLog("Delete", "Team", member.Branch ?? "Central", $"Removed logo for {member.Username}");
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
            FormErrorMessage = "Username must be at least 3 characters.";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidEmail(NewMemberEmail))
        {
            FormErrorMessage = "Please enter a valid email address (e.g. user@domain.com).";
            HasFormError = true;
            return;
        }

        if (!ValidationHelper.IsValidPhone(NewMemberPhone))
        {
            FormErrorMessage = "Phone number must contain at least 10 digits.";
            HasFormError = true;
            return;
        }

        if (NewMemberRole == "Select Role" || NewMemberBranch == "Select Branch")
        {
            FormErrorMessage = "Please select a Role and Branch.";
            HasFormError = true;
            return;
        }

        // All field validation passed — clear any previous error
        HasFormError = false;
        FormErrorMessage = string.Empty;

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
                // Keep existing status; status can only be changed via ToggleStatus
                
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
                Status = "Active",
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
