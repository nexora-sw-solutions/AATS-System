using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AATS.Desktop.ViewModels.AuditAndAccounts;
using AATS.Desktop.ViewModels.TaxFiling;
using AATS.Desktop.ViewModels.SecretarialAdvisory;
using AATS.Desktop.ViewModels.Clients;
using AATS.Desktop.ViewModels.Team;
using AATS.Desktop.ViewModels.ActivityLog;
using AATS.Desktop.ViewModels.Nexora;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using AATS.Desktop.Data;

namespace AATS.Desktop.ViewModels;

public record NavigationState(ViewModelBase ViewModel, string Main, string Sub, string ActivePage);



public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty] 
    private string _breadcrumbMain = "Home";

    [ObservableProperty]
    private string _breadcrumbSub = "";

    [ObservableProperty]
    private string _breadcrumbLeaf = "";

    public bool IsBreadcrumbSubClickable => !string.IsNullOrEmpty(BreadcrumbLeaf);
    public bool IsBreadcrumbSubOnlyVisible => !string.IsNullOrEmpty(BreadcrumbSub) && string.IsNullOrEmpty(BreadcrumbLeaf);

    partial void OnBreadcrumbSubChanged(string value)
    {
        OnPropertyChanged(nameof(IsBreadcrumbSubClickable));
        OnPropertyChanged(nameof(IsBreadcrumbSubOnlyVisible));
    }

    partial void OnBreadcrumbLeafChanged(string value)
    {
        OnPropertyChanged(nameof(IsBreadcrumbSubClickable));
        OnPropertyChanged(nameof(IsBreadcrumbSubOnlyVisible));
    }

    [ObservableProperty]
    private bool _isAccountsExpanded = true;

    [ObservableProperty]
    private bool _isTaxFilingExpanded;

    [ObservableProperty]
    private bool _isSecretarialExpanded;

    [ObservableProperty]
    private string _activePage = "Dashboard";

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    private TeamMember _currentUser = null!;
    [ObservableProperty] private bool _isSignOutConfirmVisible;
    [ObservableProperty] private bool _isEditProfileVisible;

    // Edit Profile Form Properties
    [ObservableProperty] private string _editUsername = "";
    [ObservableProperty] private string _editEmail = "";
    [ObservableProperty] private string _editPhone = "";
    [ObservableProperty] private string _currentPassword = "";
    [ObservableProperty] private string _newPassword = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private bool _hasEditProfileError;
    [ObservableProperty] private string _editProfileErrorMessage = "";
    
    // Notifications
    [ObservableProperty] private int _notificationUnreadCount;
    public bool IsAdmin => CurrentUser?.Role?.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
    public ObservableCollection<AppNotification> RecentNotifications => NotificationService.Instance.Notifications;
    
    // Global App Search
    [ObservableProperty] private string _appSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<ModuleSearchItem> _filteredModules = new();
    [ObservableProperty] private bool _isSearchDropdownOpen;
    [ObservableProperty] private ModuleSearchItem? _selectedSearchItem;
    private List<ModuleSearchItem> _allModules = new();

    private readonly Stack<NavigationState> _backStack = new();
    private readonly Stack<NavigationState> _forwardStack = new();
    private bool _isNavigatingHistory;

    // Theme
    public bool IsDarkMode => ThemeService.Instance.IsDarkMode;

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.ToggleTheme();
        OnPropertyChanged(nameof(IsDarkMode));
    }



    public MainViewModel()
    {
        _currentUser = null!; // Start null to ensure RBAC is secure by default
        
        NotificationUnreadCount = NotificationService.Instance.UnreadCount;
        NotificationService.Instance.OnNotificationAdded += (s, e) => NotificationUnreadCount = NotificationService.Instance.UnreadCount;
        NotificationService.Instance.OnNotificationUpdated += (s, e) => NotificationUnreadCount = NotificationService.Instance.UnreadCount;

        // Ensure modals are hidden on initialization
        IsSignOutConfirmVisible = false;
        IsEditProfileVisible = false;

        _ = LoadProfileAsync();
        NavigateTo(new DashboardViewModel(), "Home");
        InitializeSearchItems();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            await Task.Delay(500); // Small delay to ensure API token is definitely set
            Console.WriteLine("[DEBUG] Fetching profile...");
            var response = await ApiService.Instance.GetAsync<ApiResponse<TeamMember>>("/api/v1/auth/profile");
            if (response?.Success == true && response.Data != null)
            {
                Console.WriteLine($"[DEBUG] Profile loaded. Role: {response.Data.Role}");
                CurrentUser = response.Data;
            }
            else
            {
                Console.WriteLine($"[DEBUG] Profile fetch failed or empty. Success: {response?.Success}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error loading profile: {ex.Message}");
        }
    }

    private void InitializeSearchItems()
    {
        _allModules = new List<ModuleSearchItem>
        {
            new() { Name = "Dashboard", Route = "/home", Category = "Main", Command = NavigateToDashboardCommand },
            new() { Name = "Audit & Assurance", Route = "/accounts/audit", Category = "Accounts & Audit", Command = NavigateToAuditAssuranceCommand },
            new() { Name = "Internal Audit", Route = "/accounts/internal", Category = "Accounts & Audit", Command = NavigateToInternalAuditCommand },
            new() { Name = "Forensic Audit", Route = "/accounts/forensic", Category = "Accounts & Audit", Command = NavigateToForensicAuditCommand },
            new() { Name = "Management Account", Route = "/accounts/management", Category = "Accounts & Audit", Command = NavigateToManagementAccountCommand },
            new() { Name = "Tax Account", Route = "/accounts/tax", Category = "Accounts & Audit", Command = NavigateToTaxAccountCommand },
            new() { Name = "Internal Control Systems & Outsourcing", Route = "/accounts/control", Category = "Accounts & Audit", Command = NavigateToInternalControlCommand },
            new() { Name = "Audit Others", Route = "/accounts/others", Category = "Accounts & Audit", Command = NavigateToAuditOthersCommand },
            new() { Name = "CIT (Corporate)", Route = "/tax/cit", Category = "Tax Filing", Command = NavigateToCITCommand },
            new() { Name = "IIT (Individual)", Route = "/tax/iit", Category = "Tax Filing", Command = NavigateToIITCommand },
            new() { Name = "VAT", Route = "/tax/vat", Category = "Tax Filing", Command = NavigateToVATCommand },
            new() { Name = "SSCL", Route = "/tax/sscl", Category = "Tax Filing", Command = NavigateToSSCLCommand },
            new() { Name = "WHT", Route = "/tax/wht", Category = "Tax Filing", Command = NavigateToWHTCommand },
            new() { Name = "Tax Others", Route = "/tax/others", Category = "Tax Filing", Command = NavigateToTaxOthersCommand },
            new() { Name = "Company Registration", Route = "/secretarial/company", Category = "Secretarial & Advisory", Command = NavigateToCompanyRegistrationCommand },
            new() { Name = "EPF / ETF", Route = "/secretarial/epf-etf", Category = "Secretarial & Advisory", Command = NavigateToEPFETFCommand },
            new() { Name = "Trade License", Route = "/secretarial/trade-license", Category = "Secretarial & Advisory", Command = NavigateToTradeLicenseCommand },
            new() { Name = "Trade Mark", Route = "/secretarial/trade-mark", Category = "Secretarial & Advisory", Command = NavigateToTradeMarkCommand },
            new() { Name = "Import and Export Clearance", Route = "/secretarial/import-export", Category = "Secretarial & Advisory", Command = NavigateToImportExportCommand },
            new() { Name = "BOI", Route = "/secretarial/boi", Category = "Secretarial & Advisory", Command = NavigateToBOICommand },
            new() { Name = "HR and Management Consulting", Route = "/secretarial/hr", Category = "Secretarial & Advisory", Command = NavigateToHRConsultingCommand },
            new() { Name = "Business Plan and Asset Valuation Consulting", Route = "/secretarial/business-plan", Category = "Secretarial & Advisory", Command = NavigateToBusinessPlanCommand },
            new() { Name = "Secretarial Others", Route = "/secretarial/others", Category = "Secretarial & Advisory", Command = NavigateToSecretarialOthersCommand },
            new() { Name = "Clients", Route = "/clients", Category = "Main", Command = NavigateToClientsCommand },
            new() { Name = "Team", Route = "/team", Category = "Main", Command = NavigateToTeamCommand, IsAdminOnly = true },
            new() { Name = "Activity Log", Route = "/activity-log", Category = "Main", Command = NavigateToActivityLogCommand, IsAdminOnly = true },
            new() { Name = "Nexora", Route = "/nexora", Category = "Main", Command = NavigateToNexoraCommand }
        };
    }

    partial void OnAppSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredModules.Clear();
            IsSearchDropdownOpen = false;
            return;
        }

        var results = _allModules
            .Where(m => (!m.IsAdminOnly || IsAdmin) && 
                        (m.Name.Contains(value, StringComparison.OrdinalIgnoreCase) || 
                         m.Category.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        FilteredModules = new ObservableCollection<ModuleSearchItem>(results);
        IsSearchDropdownOpen = FilteredModules.Any();
    }


    [RelayCommand]
    private void NavigateToModule(ModuleSearchItem item)
    {
        if (item?.Command != null)
        {
            item.Command.Execute(null);
            AppSearchText = string.Empty;
            IsSearchDropdownOpen = false;
        }
    }

    [RelayCommand]
    private void HandleSearchEnter()
    {
        // If there's an exact match or only one result, navigate to it
        var exactMatch = _allModules.FirstOrDefault(m => m.Name.Equals(AppSearchText, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
        {
            NavigateToModule(exactMatch);
        }
        else if (FilteredModules.Count == 1)
        {
            NavigateToModule(FilteredModules[0]);
        }
    }

    [RelayCommand]
    private void NavigateToBreadcrumbMain()
    {
        if (string.IsNullOrEmpty(BreadcrumbMain)) return;
        
        // Special case for Home
        if (BreadcrumbMain == "Home")
        {
            NavigateToDashboard();
            return;
        }

        // Try to find a module with this name
        var module = _allModules.FirstOrDefault(m => m.Name.Equals(BreadcrumbMain, StringComparison.OrdinalIgnoreCase));
        if (module?.Command != null)
        {
            module.Command.Execute(null);
            return;
        }

        // Try to find the first module in this category
        var firstInCategory = _allModules.FirstOrDefault(m => m.Category.Equals(BreadcrumbMain, StringComparison.OrdinalIgnoreCase));
        if (firstInCategory?.Command != null)
        {
            firstInCategory.Command.Execute(null);
        }
    }

    [RelayCommand]
    private void NavigateToBreadcrumbSub()
    {
        if (string.IsNullOrEmpty(BreadcrumbSub)) return;

        // Try to find a module with this name
        var module = _allModules.FirstOrDefault(m => m.Name.Equals(BreadcrumbSub, StringComparison.OrdinalIgnoreCase));
        if (module?.Command != null)
        {
            module.Command.Execute(null);
        }
    }

    partial void OnIsAccountsExpandedChanged(bool value)
    {
        if (value)
        {
            IsTaxFilingExpanded = false;
            IsSecretarialExpanded = false;
        }
    }

    partial void OnIsTaxFilingExpandedChanged(bool value)
    {
        if (value)
        {
            IsAccountsExpanded = false;
            IsSecretarialExpanded = false;
        }
    }

    partial void OnIsSecretarialExpandedChanged(bool value)
    {
        if (value)
        {
            IsAccountsExpanded = false;
            IsTaxFilingExpanded = false;
        }
    }

    // Navigation Commands
    private void NavigateTo(ViewModelBase viewModel, string main, string sub = "", string leaf = "", string activePage = "")
    {
        string finalActivePage = string.IsNullOrEmpty(activePage) ? (string.IsNullOrEmpty(leaf) ? (string.IsNullOrEmpty(sub) ? main : sub) : leaf) : activePage;

        if (!_isNavigatingHistory && CurrentView != null)
        {
            _backStack.Push(new NavigationState(CurrentView, BreadcrumbMain, BreadcrumbSub, ActivePage));
            _forwardStack.Clear();
            UpdateCommandStates();
        }

        CurrentView = viewModel;
        BreadcrumbMain = main;
        BreadcrumbSub = sub;
        BreadcrumbLeaf = leaf;
        ActivePage = finalActivePage;
    }


    private void UpdateCommandStates()
    {
        GoBackCommand.NotifyCanExecuteChanged();
        GoForwardCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        if (_backStack.Count == 0) return;

        _isNavigatingHistory = true;
        _forwardStack.Push(new NavigationState(CurrentView!, BreadcrumbMain, BreadcrumbSub, ActivePage));
        
        var state = _backStack.Pop();
        NavigateTo(state.ViewModel, state.Main, state.Sub, state.ActivePage);
        
        _isNavigatingHistory = false;
        UpdateCommandStates();

    }

    private bool CanGoBack() => _backStack.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        if (_forwardStack.Count == 0) return;

        _isNavigatingHistory = true;
        _backStack.Push(new NavigationState(CurrentView!, BreadcrumbMain, BreadcrumbSub, ActivePage));

        var state = _forwardStack.Pop();
        NavigateTo(state.ViewModel, state.Main, state.Sub, state.ActivePage);

        _isNavigatingHistory = false;
        UpdateCommandStates();

    }

    private bool CanGoForward() => _forwardStack.Count > 0;


    [RelayCommand]
    private void NavigateToDashboard()
    {
        var vm = new DashboardViewModel
        {
            NavigateToAuditAssurance = NavigateToAuditAssurance,
            NavigateToCompanyRegistration = NavigateToCompanyRegistration,
            NavigateToTeam = NavigateToTeam,
            NavigateToCIT = NavigateToCIT,
            NavigateToClients = NavigateToClients
        };
        NavigateTo(vm, "Home", "", leaf: "Dashboard", activePage: "Dashboard");
    }

    
    // Audit & Accounts
    [RelayCommand]
    private void NavigateToAuditAssurance()
    {
        var vm = new AuditAssuranceViewModel();
        vm.NavigateToAddRecord = NavigateToAuditAssuranceAddRecord;
        vm.NavigateToDetail = NavigateToAuditAssuranceDetail;
        NavigateTo(vm, "Audit & Accounts", "Audit & Assurance", activePage: "AuditAssurance");
    }


    [RelayCommand]
    private void NavigateToAuditAssuranceDetail(AuditRecord record)
    {
        var vm = new AuditAssuranceDetailViewModel(record);
        vm.NavigateBack = NavigateToAuditAssurance;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Audit & Assurance", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new AuditAssuranceAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Audit & Assurance", "Edit Record", "AuditAssurance");
        };
        NavigateTo(vm, "Audit & Accounts", "Audit & Assurance", "Client Details", "AuditAssurance");
    }

    [RelayCommand]
    private void NavigateToAuditAssuranceAddRecord()
    {
        var vm = CurrentView as AuditAssuranceViewModel ?? new AuditAssuranceViewModel();
        vm.NavigateToAddRecord = NavigateToAuditAssuranceAddRecord;
        vm.NavigateToDetail = NavigateToAuditAssuranceDetail;

        var addVm = new AuditAssuranceAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Audit & Assurance", "Add Record", "AuditAssurance");
    }

    [RelayCommand]
    private void NavigateToInternalAudit()
    {
        var vm = new InternalAuditViewModel();
        vm.NavigateToAddRecord = NavigateToInternalAuditAddRecord;
        vm.NavigateToDetail = NavigateToInternalAuditDetail;
        NavigateTo(vm, "Audit & Accounts", "Internal Audit", activePage: "InternalAudit");
    }

    [RelayCommand]
    private void NavigateToInternalAuditAddRecord()
    {
        var vm = CurrentView as InternalAuditViewModel ?? new InternalAuditViewModel();
        vm.NavigateToAddRecord = NavigateToInternalAuditAddRecord;
        vm.NavigateToDetail = NavigateToInternalAuditDetail;

        var addVm = new InternalAuditAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Internal Audit", "Add Record", "InternalAudit");
    }

    [RelayCommand]
    private void NavigateToInternalAuditDetail(AuditRecord record)
    {
        var vm = new InternalAuditDetailViewModel(record);
        vm.NavigateBack = NavigateToInternalAudit;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Internal Audit", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new InternalAuditAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Internal Audit", "Edit Record", "InternalAudit");
        };
        NavigateTo(vm, "Audit & Accounts", "Internal Audit", "Client Details", "InternalAudit");
    }

    [RelayCommand]
    private void NavigateToForensicAudit()
    {
        var vm = new ForensicAuditViewModel();
        vm.NavigateToAddRecord = NavigateToForensicAuditAddRecord;
        vm.NavigateToDetail = NavigateToForensicAuditDetail;
        NavigateTo(vm, "Audit & Accounts", "Forensic Audit", activePage: "ForensicAudit");
    }

    [RelayCommand]
    private void NavigateToForensicAuditAddRecord()
    {
        var vm = CurrentView as ForensicAuditViewModel ?? new ForensicAuditViewModel();
        vm.NavigateToAddRecord = NavigateToForensicAuditAddRecord;
        vm.NavigateToDetail = NavigateToForensicAuditDetail;

        var addVm = new ForensicAuditAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Forensic Audit", "Add Record", "ForensicAudit");
    }

    [RelayCommand]
    private void NavigateToForensicAuditDetail(AuditRecord record)
    {
        var vm = new ForensicAuditDetailViewModel(record);
        vm.NavigateBack = NavigateToForensicAudit;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Forensic Audit", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new ForensicAuditAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Forensic Audit", "Edit Record", "ForensicAudit");
        };
        NavigateTo(vm, "Audit & Accounts", "Forensic Audit", "Client Details", "ForensicAudit");
    }

    [RelayCommand]
    private void NavigateToManagementAccount()
    {
        var vm = new ManagementAccountViewModel();
        vm.NavigateToAddRecord = NavigateToManagementAccountAddRecord;
        vm.NavigateToDetail = NavigateToManagementAccountDetail;
        NavigateTo(vm, "Audit & Accounts", "Management Accountings", activePage: "ManagementAccount");
    }

    [RelayCommand]
    private void NavigateToManagementAccountAddRecord()
    {
        var vm = CurrentView as ManagementAccountViewModel ?? new ManagementAccountViewModel();
        vm.NavigateToAddRecord = NavigateToManagementAccountAddRecord;
        vm.NavigateToDetail = NavigateToManagementAccountDetail;

        var addVm = new ManagementAccountAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Management Accountings", "Add Record", "ManagementAccount");
    }

    [RelayCommand]
    private void NavigateToManagementAccountDetail(AuditRecord record)
    {
        var vm = new ManagementAccountDetailViewModel(record);
        vm.NavigateBack = NavigateToManagementAccount;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Management Accountings", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new ManagementAccountAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Management Accountings", "Edit Record", "ManagementAccount");
        };
        NavigateTo(vm, "Audit & Accounts", "Management Accountings", "Client Details", "ManagementAccount");
    }


    [RelayCommand]
    private void NavigateToTaxAccount()
    {
        var vm = new TaxAccountViewModel();
        vm.NavigateToAddRecord = NavigateToTaxAccountAddRecord;
        vm.NavigateToDetail = NavigateToTaxAccountDetail;
        NavigateTo(vm, "Audit & Accounts", "Tax Accountings", activePage: "TaxAccount");
    }

    [RelayCommand]
    private void NavigateToTaxAccountAddRecord()
    {
        var vm = CurrentView as TaxAccountViewModel ?? new TaxAccountViewModel();
        vm.NavigateToAddRecord = NavigateToTaxAccountAddRecord;
        vm.NavigateToDetail = NavigateToTaxAccountDetail;

        var addVm = new TaxAccountAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Tax Accountings", "Add Record", "TaxAccount");
    }

    [RelayCommand]
    private void NavigateToTaxAccountDetail(AuditRecord record)
    {
        var vm = new TaxAccountDetailViewModel(record);
        vm.NavigateBack = NavigateToTaxAccount;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Tax Accountings", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new TaxAccountAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Tax Accountings", "Edit Record", "TaxAccount");
        };
        NavigateTo(vm, "Audit & Accounts", "Tax Accountings", "Client Details", "TaxAccount");
    }

    [RelayCommand]
    private void NavigateToInternalControl()
    {
        var vm = new InternalControlViewModel();
        vm.NavigateToAddRecord = NavigateToInternalControlAddRecord;
        vm.NavigateToDetail = NavigateToInternalControlDetail;
        NavigateTo(vm, "Audit & Accounts", "Internal Control Systems & Outsourcing", activePage: "InternalControl");
    }

    [RelayCommand]
    private void NavigateToInternalControlAddRecord()
    {
        var vm = CurrentView as InternalControlViewModel ?? new InternalControlViewModel();
        vm.NavigateToAddRecord = NavigateToInternalControlAddRecord;
        vm.NavigateToDetail = NavigateToInternalControlDetail;

        var addVm = new InternalControlAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Internal Control Systems & Outsourcing", "Add Record", "InternalControl");
    }

    [RelayCommand]
    private void NavigateToInternalControlDetail(AuditRecord record)
    {
        var vm = new InternalControlDetailViewModel(record);
        vm.NavigateBack = NavigateToInternalControl;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Internal Control Systems & Outsourcing", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var addVm = new InternalControlAddRecordViewModel(r);
            addVm.GoBack = async () => 
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addVm, "Audit & Accounts", "Internal Control Systems & Outsourcing", "Edit Record", "InternalControl");
        };
        NavigateTo(vm, "Audit & Accounts", "Internal Control Systems & Outsourcing", "Client Details", "InternalControl");
    }


    [RelayCommand]
    private void NavigateToAuditOthers()
    {
        var vm = new AuditOthersViewModel();
        vm.NavigateToAddRecord = NavigateToAuditOthersAddRecord;
        NavigateTo(vm, "Audit & Accounts", "Audit Others", activePage: "AuditOthers");
    }

    [RelayCommand]
    private void NavigateToAuditOthersAddRecord()
    {
        var vm = CurrentView as AuditOthersViewModel ?? new AuditOthersViewModel();
        vm.NavigateToAddRecord = NavigateToAuditOthersAddRecord;

        var addVm = new AuditOthersAddRecordViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Audit & Accounts", "Audit Others", "Add Record", "AuditOthers");
    }


    // Tax Filing
    [RelayCommand]
    private void NavigateToCIT()
    {
        var vm = new CITViewModel();
        // Temporarily hook to a placeholder or stay null until TaxDetailViewModel is ready
        // vm.NavigateToDetail = ... 
        NavigateTo(vm, "Tax Filing", "CIT");
    }

    [RelayCommand]
    private void NavigateToIIT()
    {
        var vm = new IITViewModel();
        NavigateTo(vm, "Tax Filing", "IIT");
    }

    [RelayCommand]
    private void NavigateToVAT()
    {
        var vm = new VATViewModel();
        NavigateTo(vm, "Tax Filing", "VAT");
    }

    [RelayCommand]
    private void NavigateToSSCL()
    {
        var vm = new SSCLViewModel();
        NavigateTo(vm, "Tax Filing", "SSCL");
    }

    [RelayCommand]
    private void NavigateToWHT()
    {
        var vm = new WHTViewModel();
        NavigateTo(vm, "Tax Filing", "WHT");
    }

    [RelayCommand]
    private void NavigateToTaxOthers()
    {
        var vm = new TaxOthersViewModel();
        NavigateTo(vm, "Tax Filing", "Others", activePage: "TaxOthers");
    }


    [RelayCommand]
    private void NavigateToCompanyRegistration()
    {
        var vm = new CompanyRegistrationViewModel();
        vm.NavigateToAddRecord = NavigateToCompanyRegistrationAddRecord;
        vm.NavigateToDetail = NavigateToCompanyRegistrationDetail;
        NavigateTo(vm, "Secretarial & Advisory", "Company Registration", activePage: "CompanyRegistration");
    }

    [RelayCommand]
    private void NavigateToCompanyRegistrationDetail(AuditRecord record)
    {
        var vm = new CompanyRegistrationDetailViewModel(record);
        vm.NavigateBack = NavigateToCompanyRegistration;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Company Registration", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddCompanyRegistrationViewModel(r);
            editVm.GoBack = async () =>
            {
                await vm.LoadFullRecordAsync();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "Company Registration", "Edit Record", "CompanyRegistration");
        };
        NavigateTo(vm, "Secretarial & Advisory", "Company Registration", "Client Details", "CompanyRegistration");
    }

    [RelayCommand]
    private void NavigateToCompanyRegistrationAddRecord()
    {
        var vm = CurrentView as CompanyRegistrationViewModel ?? new CompanyRegistrationViewModel();
        vm.NavigateToAddRecord = NavigateToCompanyRegistrationAddRecord;

        var addVm = new AddCompanyRegistrationViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Company Registration", "Add Record", "CompanyRegistration");
    }

    [RelayCommand]
    private void NavigateToEPFETF()
    {
        var vm = new EPFETFViewModel();
        vm.NavigateToAddRecord = NavigateToEPFETFAddRecord;
        vm.NavigateToDetail = NavigateToEPFETFDetail;
        NavigateTo(vm, "Secretarial & Advisory", "EPF / ETF", activePage: "EPFETF");
    }

    [RelayCommand]
    private void NavigateToEPFETFDetail(AuditRecord record)
    {
        var vm = new EPFETFDetailViewModel(record);
        vm.NavigateBack = NavigateToEPFETF;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("EPF / ETF", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddEPFETFViewModel(r);
            editVm.GoBack = async () =>
            {
                await vm.LoadFullRecordAsync();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "EPF / ETF", "Edit Record", "EPFETF");
        };
        vm.NavigateToAddStaff = (r) =>
        {
            var addStaffVm = new AddEPFETFStaffViewModel(r);
            addStaffVm.GoBack = async () =>
            {
                await vm.LoadFullRecordAsync();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addStaffVm, "Secretarial & Advisory", "EPF / ETF", "Add Staff", "EPFETF");
        };
        vm.NavigateToStaffDetail = (parent, member) =>
        {
            NavigateToEPFETFStaffDetail(parent, member);
        };
        NavigateTo(vm, "Secretarial & Advisory", "EPF / ETF", "Client Details", "EPFETF");
    }

    [RelayCommand]
    private void NavigateToEPFETFAddStaff(AuditRecord parentRecord)
    {
        var addStaffVm = new AddEPFETFStaffViewModel(parentRecord);
        addStaffVm.GoBack = async () => 
        {
            GoBackCommand.Execute(null);
        };
        NavigateTo(addStaffVm, "Secretarial & Advisory", "EPF / ETF", "Add Staff", "EPFETF");
    }

    public void NavigateToEPFETFStaffDetail(AuditRecord parent, StaffMember member)
    {
        var vm = new StaffDetailViewModel(parent, member);
        vm.NavigateBack = () => NavigateToEPFETFDetail(parent);
        vm.NavigateToEditStaff = (p, m) =>
        {
            var addStaffVm = new AddEPFETFStaffViewModel(p, m);
            addStaffVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(addStaffVm, "Secretarial & Advisory", "EPF / ETF", "Edit Staff", "EPFETF");
        };
        NavigateTo(vm, "Secretarial & Advisory", "EPF / ETF", "Staff Details", "EPFETF");
    }

    [RelayCommand]
    private void NavigateToEPFETFAddRecord()
    {
        var vm = CurrentView as EPFETFViewModel ?? new EPFETFViewModel();
        vm.NavigateToAddRecord = NavigateToEPFETFAddRecord;

        var addVm = new AddEPFETFViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "EPF / ETF", "Add Record", "EPFETF");
    }

    [RelayCommand]
    private void NavigateToTradeLicense()
    {
        var vm = new TradeLicenseViewModel();
        vm.NavigateToAddRecord = NavigateToTradeLicenseAddRecord;
        vm.NavigateToDetail = NavigateToTradeLicenseDetail;
        NavigateTo(vm, "Secretarial & Advisory", "Trade License", activePage: "TradeLicense");
    }

    [RelayCommand]
    private void NavigateToTradeLicenseAddRecord()
    {
        var vm = CurrentView as TradeLicenseViewModel ?? new TradeLicenseViewModel();
        vm.NavigateToAddRecord = NavigateToTradeLicenseAddRecord;

        var addVm = new AddTradeLicenseViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Trade License", "Add Record", "TradeLicense");
    }

    [RelayCommand]
    private void NavigateToTradeLicenseDetail(AuditRecord record)
    {
        var vm = new TradeLicenseDetailViewModel(record);
        vm.NavigateBack = NavigateToTradeLicense;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Trade License", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddTradeLicenseViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "Trade License", "Edit Record", "TradeLicense");
        };
        NavigateTo(vm, "Secretarial & Advisory", "Trade License", "Client Details", "TradeLicense");
    }

    [RelayCommand]
    private void NavigateToTradeMark()
    {
        var vm = new TradeMarkViewModel();
        vm.NavigateToAddRecord = NavigateToTradeMarkAddRecord;
        vm.NavigateToDetail = NavigateToTradeMarkDetail; // Set the navigation hook
        NavigateTo(vm, "Secretarial & Advisory", "Trade Mark", activePage: "TradeMark");
    }

    [RelayCommand]
    private void NavigateToTradeMarkDetail(AuditRecord record)
    {
        var vm = new TradeMarkDetailViewModel(record);
        vm.NavigateBack = NavigateToTradeMark;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Trade Mark", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddTradeMarkViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "Trade Mark", "Edit Record", "TradeMark");
        };
        NavigateTo(vm, "Secretarial & Advisory", "Trade Mark", "Client Details", "TradeMark");
    }

    [RelayCommand]
    private void NavigateToTradeMarkAddRecord()
    {
        var vm = CurrentView as TradeMarkViewModel ?? new TradeMarkViewModel();
        vm.NavigateToAddRecord = NavigateToTradeMarkAddRecord;

        var addVm = new AddTradeMarkViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Trade Mark", "Add Record", "TradeMark");
    }

    [RelayCommand]
    private void NavigateToImportExport()
    {
        var vm = new ImportExportViewModel();
        vm.NavigateToAddRecord = NavigateToImportExportAddRecord;
        vm.NavigateToDetail = NavigateToImportExportDetail;
        NavigateTo(vm, "Secretarial & Advisory", "Import and Export Clearance", activePage: "ImportExport");
    }

    [RelayCommand]
    private void NavigateToImportExportDetail(AuditRecord record)
    {
        var vm = new ImportExportDetailViewModel(record);
        vm.NavigateBack = NavigateToImportExport;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Import and Export Clearance", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddImportExportViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "Import and Export Clearance", "Edit Record", "ImportExport");
        };
        NavigateTo(vm, "Secretarial & Advisory", "Import and Export Clearance", "Client Details", "ImportExport");
    }

    [RelayCommand]
    private void NavigateToImportExportAddRecord()
    {
        var vm = CurrentView as ImportExportViewModel ?? new ImportExportViewModel();
        vm.NavigateToAddRecord = NavigateToImportExportAddRecord;

        var addVm = new AddImportExportViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Import and Export Clearance", "Add Record", "ImportExport");
    }

    [RelayCommand]
    private void NavigateToBOI()
    {
        var vm = new BOIViewModel();
        vm.NavigateToAddRecord = NavigateToBOIAddRecord;
        vm.NavigateToDetail = NavigateToBOIDetail;
        NavigateTo(vm, "Secretarial & Advisory", "BOI", activePage: "BOI");
    }

    [RelayCommand]
    private void NavigateToBOIDetail(AuditRecord record)
    {
        var vm = new BOIDetailViewModel(record);
        vm.NavigateBack = NavigateToBOI;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("BOI", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddBOIViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "BOI", "Edit Record", "BOI");
        };
        NavigateTo(vm, "Secretarial & Advisory", "BOI", "Client Details", "BOI");
    }

    [RelayCommand]
    private void NavigateToBOIAddRecord()
    {
        var vm = CurrentView as BOIViewModel ?? new BOIViewModel();
        vm.NavigateToAddRecord = NavigateToBOIAddRecord;

        var addVm = new AddBOIViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "BOI", "Add Record", "BOI");
    }

    [RelayCommand]
    private void NavigateToHRConsulting()
    {
        var vm = new HRConsultingViewModel();
        vm.NavigateToAddRecord = NavigateToHRConsultingAddRecord;
        vm.NavigateToDetail = NavigateToHRConsultingDetail;
        NavigateTo(vm, "Secretarial & Advisory", "HR and Management Consulting", activePage: "HRConsulting");
    }

    [RelayCommand]
    private void NavigateToHRConsultingDetail(AuditRecord record)
    {
        var vm = new HRConsultingDetailViewModel(record);
        vm.NavigateBack = NavigateToHRConsulting;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("HR and Management Consulting", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddHRConsultingViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "HR and Management Consulting", "Edit Record", "HRConsulting");
        };
        NavigateTo(vm, "Secretarial & Advisory", "HR and Management Consulting", "Client Details", "HRConsulting");
    }

    [RelayCommand]
    private void NavigateToHRConsultingAddRecord()
    {
        var vm = CurrentView as HRConsultingViewModel ?? new HRConsultingViewModel();
        vm.NavigateToAddRecord = NavigateToHRConsultingAddRecord;

        var addVm = new AddHRConsultingViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "HR and Management Consulting", "Add Record", "HRConsulting");
    }

    [RelayCommand]
    private void NavigateToBusinessPlan()
    {
        var vm = new BusinessPlanViewModel();
        vm.NavigateToAddRecord = NavigateToBusinessPlanAddRecord;
        vm.NavigateToDetail = NavigateToBusinessPlanDetail;
        NavigateTo(vm, "Secretarial & Advisory", "Business Plan and Asset Valuation Consulting", activePage: "BusinessPlan");
    }

    [RelayCommand]
    private void NavigateToBusinessPlanDetail(AuditRecord record)
    {
        var vm = new BusinessPlanDetailViewModel(record);
        vm.NavigateBack = NavigateToBusinessPlan;
        vm.DeleteRecordAction = async () => { await DataService.Instance.DeleteAuditRecordsAsync("Business Plan and Asset Valuation Consulting", new[] { record }); };
        vm.NavigateToEditRecord = (r) =>
        {
            var editVm = new AddBusinessPlanViewModel(r);
            editVm.GoBack = async () =>
            {
                vm.Refresh();
                GoBackCommand.Execute(null);
            };
            NavigateTo(editVm, "Secretarial & Advisory", "Business Plan and Asset Valuation Consulting", "Edit Record", "BusinessPlan");
        };
        NavigateTo(vm, "Secretarial & Advisory", "Business Plan and Asset Valuation Consulting", "Client Details", "BusinessPlan");
    }

    [RelayCommand]
    private void NavigateToBusinessPlanAddRecord()
    {
        var vm = CurrentView as BusinessPlanViewModel ?? new BusinessPlanViewModel();
        vm.NavigateToAddRecord = NavigateToBusinessPlanAddRecord;

        var addVm = new AddBusinessPlanViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Business Plan and Asset Valuation Consulting", "Add Record", "BusinessPlan");
    }


    [RelayCommand]
    private void NavigateToSecretarialOthers()
    {
        var vm = new SecretarialOthersViewModel();
        vm.NavigateToAddRecord = NavigateToSecretarialOthersAddRecord;
        NavigateTo(vm, "Secretarial & Advisory", "Others", activePage: "SecretarialOthers");
    }

    [RelayCommand]
    private void NavigateToSecretarialOthersAddRecord()
    {
        var vm = CurrentView as SecretarialOthersViewModel ?? new SecretarialOthersViewModel();
        vm.NavigateToAddRecord = NavigateToSecretarialOthersAddRecord;

        var addVm = new AddSecretarialOthersViewModel();
        addVm.GoBack = async () => 
        {
            await vm.LoadDataAsync();
            GoBackCommand.Execute(null);
        };
        NavigateTo(addVm, "Secretarial & Advisory", "Others", "Add Record", "SecretarialOthers");
    }


    // Main Categories
    [RelayCommand]
    private void NavigateToClients() => NavigateTo(new ClientsViewModel(), "Home", "Clients", activePage: "Clients");

    [RelayCommand]
    private void NavigateToTeam() => NavigateTo(new TeamViewModel(), "Team");

    [RelayCommand]
    private void NavigateToActivityLog() => NavigateTo(new ActivityLogViewModel(), "Activity Log", activePage: "ActivityLog");


    [RelayCommand]
    private void NavigateToNexora() => NavigateTo(new NexoraViewModel(), "Nexora");

    [RelayCommand]
    private void NavigateToNotifications() => NavigateTo(new NotificationsViewModel(), "Notifications");

    // Sign Out Workflow
    [RelayCommand]
    private void ShowSignOutConfirm() => IsSignOutConfirmVisible = true;

    [RelayCommand]
    private void CancelSignOut() => IsSignOutConfirmVisible = false;

    [RelayCommand]
    private void ConfirmSignOut()
    {
        IsSignOutConfirmVisible = false;
        // Simulation of Logout
        NotificationService.Instance.AddNotification(CurrentUser?.Username ?? "Unknown", "logged out");
        LogService.Instance.AddLog("Logout", "Auth", "Central", $"User '{CurrentUser?.Username ?? "Unknown"}' signed out successfully.");
        // Redirect to LoginView
        if (Avalonia.Application.Current is App app)
        {
            app.SwitchToLoginWindow();
        }
    }

    // Edit Profile Workflow
    [RelayCommand]
    private void ShowEditProfile()
    {
        EditUsername = CurrentUser.Username ?? "";
        EditEmail = CurrentUser.Email ?? "";
        EditPhone = CurrentUser.Phone ?? "";
        CurrentPassword = "";
        NewPassword = "";
        ConfirmPassword = "";
        HasEditProfileError = false;
        EditProfileErrorMessage = "";
        IsEditProfileVisible = true;
    }

    [RelayCommand]
    private void CancelEditProfile()
    {
        IsEditProfileVisible = false;
    }

    [RelayCommand]
    private async Task UpdateProfile()
    {
        HasEditProfileError = false;
        EditProfileErrorMessage = "";

        // 1. Validation
        if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmPassword)
        {
            HasEditProfileError = true;
            EditProfileErrorMessage = "New passwords do not match.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            HasEditProfileError = true;
            EditProfileErrorMessage = "Current password is required to save changes.";
            return;
        }

        // 2. Request Object
        var request = new
        {
            Username = EditUsername,
            Email = EditEmail,
            Phone = EditPhone,
            CurrentPassword = CurrentPassword,
            NewPassword = string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword
        };

        // 3. API Call
        try
        {
            var response = await ApiService.Instance.PutAsync<object, ApiResponse<object>>("/api/v1/auth/profile", request);
            if (response?.Success == true)
            {
                NotificationService.Instance.AddNotification("Success", "Profile updated successfully.");
                IsEditProfileVisible = false;
                
                LogService.Instance.AddLog("Update", "Profile", CurrentUser.Branch ?? "Central", 
                    $"User '{EditUsername}' updated their profile details.");
                    
                // Refresh local user state
                await LoadProfileAsync();
            }
        }
        catch (Exception ex)
        {
            HasEditProfileError = true;
            EditProfileErrorMessage = ex.Message.Replace("Server error (400): ", "");
        }
    }
}

