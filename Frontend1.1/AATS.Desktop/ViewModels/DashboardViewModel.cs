using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AATS.Desktop.ViewModels;

public record BranchActivity(string Branch, int Count);
public record StatusCount(string Status, int Count, string Color);

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDataService _dataService;

    [ObservableProperty] private int _totalClients;
    [ObservableProperty] private int _activeClients;
    [ObservableProperty] private int _inactiveClients;
    [ObservableProperty] private string _totalRevenue = "LKR 0";
    [ObservableProperty] private string _outstandingBalance = "LKR 0";
    [ObservableProperty] private int _clientsWithDues;
    [ObservableProperty] private int _totalAuditRecords;
    [ObservableProperty] private int _totalSecretarialRecords;
    [ObservableProperty] private int _totalTeamMembers;
    [ObservableProperty] private int _paidAudits;
    [ObservableProperty] private int _partialAudits;
    [ObservableProperty] private int _unpaidAudits;

    // Chart Angles - Payment Status
    public double PaidSweep => TotalAuditRecords > 0 ? (double)PaidAudits / TotalAuditRecords * 360 : 0;
    public double PartialSweep => TotalAuditRecords > 0 ? (double)PartialAudits / TotalAuditRecords * 360 : 0;
    public double UnpaidSweep => TotalAuditRecords > 0 ? (double)UnpaidAudits / TotalAuditRecords * 360 : 0;

    public double PaidStart => -90;
    public double PartialStart => PaidStart + PaidSweep;
    public double UnpaidStart => PartialStart + PartialSweep;

    // Chart Angles - Client Categories
    [ObservableProperty] private double _corporateSweep;
    [ObservableProperty] private double _smeSweep;
    [ObservableProperty] private double _individualSweep;
    [ObservableProperty] private double _othersSweep;

    public double CorporateStart => -90;
    public double SmeStart => CorporateStart + CorporateSweep;
    public double IndividualStart => SmeStart + SmeSweep;
    public double OthersStart => IndividualStart + IndividualSweep;

    public ObservableCollection<BranchActivity> BranchActivities { get; } = new();
    public ObservableCollection<StatusCount> PaymentStatuses { get; } = new();
    public ObservableCollection<StatusCount> ClientCategories { get; } = new();
    public ObservableCollection<ActivityLogEntry> RecentActivities { get; } = new();

    // Filters
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _searchText = string.Empty;
    public ObservableCollection<string> DateFilters { get; } = new() { "All Dates", "Today", "This Week", "This Month", "This Year", "Specific Date", "Specific Period" };
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpecificDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsMonthVisible))]
    [NotifyPropertyChangedFor(nameof(IsYearVisible))]
    [NotifyPropertyChangedFor(nameof(IsPeriodVisible))]
    [NotifyPropertyChangedFor(nameof(DateFilterSummary))]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedDateFilter;

    public ObservableCollection<string> StatusFilters { get; } = new() { "All Status", "Paid", "Unpaid", "Partial" };
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedStatusFilter;
    public ObservableCollection<string> BranchFilters { get; } = new() { "All Branch", "Central", "South", "West", "Northeast" };
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    private string _selectedBranchFilter;

    // Filter Visibility
    public bool IsSpecificDateVisible => SelectedDateFilter == "Specific Date";
    public bool IsMonthVisible => SelectedDateFilter == "This Month";
    public bool IsYearVisible => SelectedDateFilter == "This Year";
    public bool IsPeriodVisible => SelectedDateFilter == "Specific Period";

    [ObservableProperty] private DateTime? _specificDate;
    [ObservableProperty] private DateTime? _monthDate;
    [ObservableProperty] private string? _selectedMonth;
    [ObservableProperty] private string? _selectedYear;
    [ObservableProperty] private DateTime? _periodStartDate;
    [ObservableProperty] private DateTime? _periodEndDate;

    public ObservableCollection<string> Months { get; } = new() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public ObservableCollection<string> Years { get; } = new() { "2024", "2025", "2026", "2027", "2028" };

    public string SpecificDateDisplay => SpecificDate.HasValue ? SpecificDate.Value.ToString("dd/MM/yyyy") : "Pick a date";
    public string MonthDisplay => MonthDate.HasValue ? MonthDate.Value.ToString("MMMM yyyy") : "Pick month";
    
    public string PeriodDisplay => PeriodStartDate.HasValue && PeriodEndDate.HasValue 
        ? $"{PeriodStartDate:dd/MM/yyyy} - {PeriodEndDate:dd/MM/yyyy}" 
        : "Pick a range";

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchText) || 
                                   SelectedDateFilter != "All Dates" || 
                                   SelectedStatusFilter != "All Status" || 
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
    
    // Data storage
    private List<ClientRecord> _allClients = new();
    private List<AuditRecord> _allAudits = new();

    // Navigation Actions
    public Action? NavigateToAuditAssurance { get; set; }
    public Action? NavigateToCompanyRegistration { get; set; }
    public Action? NavigateToTeam { get; set; }
    public Action? NavigateToCIT { get; set; }
    public Action? NavigateToClients { get; set; }

    public DashboardViewModel()
    {
        _dataService = DataService.Instance;
        
        // Default filter selections
        _selectedDateFilter = DateFilters[0];
        _selectedStatusFilter = StatusFilters[0];
        _selectedBranchFilter = BranchFilters[0];

        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            _allClients = await _dataService.GetClientsAsync();
            
            var auditCategories = new[] { "Audit & Assurance", "Internal Audit", "Others", "Forensic Audit & Investigation", "Internal Control Systems & Outsourcing", "Management Accountings", "Tax Accountings" };
            _allAudits.Clear();
            foreach (var cat in auditCategories)
            {
                _allAudits.AddRange(await _dataService.GetAuditRecordsAsync(cat));
            }

            TotalTeamMembers = (await _dataService.GetTeamMembersAsync()).Count;

            ApplyFilters();
        }
        catch (Exception)
        {
            // Handle error
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedDateFilter = "All Dates";
        SelectedStatusFilter = "All Status";
        SelectedBranchFilter = "All Branch";
        
        // Reset sub-filters
        SpecificDate = null;
        MonthDate = null;
        SelectedYear = null;
        SelectedMonth = null;
        PeriodStartDate = null;
        PeriodEndDate = null;
        
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        // 1. Filter Clients and Audits
        var filteredClients = _allClients.Where(c => 
            (string.IsNullOrEmpty(SearchText) || (c.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (SelectedBranchFilter == "All Branch" || string.Equals(c.Branch, SelectedBranchFilter, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        var filteredAudits = _allAudits.Where(a => 
            (string.IsNullOrEmpty(SearchText) || (a.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (SelectedBranchFilter == "All Branch" || string.Equals(a.Branch, SelectedBranchFilter, StringComparison.OrdinalIgnoreCase)) &&
            (SelectedStatusFilter == "All Status" || string.Equals(a.PaymentStatus, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        // Apply Date Filters
        filteredAudits = ApplyDateFilter(filteredAudits);

        // 2. Update KPIs
        TotalClients = filteredClients.Count;
        ActiveClients = filteredClients.Count(c => c.Status == "Active");
        InactiveClients = TotalClients - ActiveClients;
        
        decimal revenue = filteredClients.Sum(c => c.TotalRevenue);
        TotalRevenue = $"LKR {revenue:N0}";
        
        decimal dues = filteredClients.Sum(c => c.DueAmount);
        OutstandingBalance = $"LKR {dues:N0}";
        ClientsWithDues = filteredClients.Count(c => c.DueAmount > 0);

        TotalAuditRecords = filteredAudits.Count;
        PaidAudits = filteredAudits.Count(a => a.PaymentStatus == "Paid");
        PartialAudits = filteredAudits.Count(a => a.PaymentStatus == "Partial");
        UnpaidAudits = filteredAudits.Count(a => a.PaymentStatus == "Unpaid");

        // Notify chart updates for payments
        OnPropertyChanged(nameof(PaidSweep));
        OnPropertyChanged(nameof(PartialSweep));
        OnPropertyChanged(nameof(UnpaidSweep));
        OnPropertyChanged(nameof(PaidStart));
        OnPropertyChanged(nameof(PartialStart));
        OnPropertyChanged(nameof(UnpaidStart));

        // Calculate Category Sweeps
        if (TotalClients > 0)
        {
            CorporateSweep = (double)filteredClients.Count(c => c.Category == "Corporate") / TotalClients * 360;
            SmeSweep = (double)filteredClients.Count(c => c.Category == "SME") / TotalClients * 360;
            IndividualSweep = (double)filteredClients.Count(c => c.Category == "Individual") / TotalClients * 360;
            OthersSweep = 360 - (CorporateSweep + SmeSweep + IndividualSweep);
        }
        else
        {
            CorporateSweep = SmeSweep = IndividualSweep = OthersSweep = 0;
        }

        OnPropertyChanged(nameof(CorporateStart));
        OnPropertyChanged(nameof(SmeStart));
        OnPropertyChanged(nameof(IndividualStart));
        OnPropertyChanged(nameof(OthersStart));

        // 3. Update Charts
        var branches = filteredAudits.GroupBy(a => a.Branch)
                                    .Select(g => new BranchActivity(g.Key ?? "Unknown", g.Count()))
                                    .OrderByDescending(b => b.Count);
        BranchActivities.Clear();
        foreach (var b in branches) BranchActivities.Add(b);

        PaymentStatuses.Clear();
        PaymentStatuses.Add(new StatusCount("Paid", PaidAudits, "#10B981"));
        PaymentStatuses.Add(new StatusCount("Partial", PartialAudits, "#F59E0B"));
        PaymentStatuses.Add(new StatusCount("Unpaid", UnpaidAudits, "#EF4444"));

        var catGroups = filteredClients.GroupBy(c => c.Category)
                                   .Select(g => new StatusCount(g.Key ?? "Unknown", g.Count(), GetCategoryColor(g.Key)));
        ClientCategories.Clear();
        foreach (var c in catGroups) ClientCategories.Add(c);

        _ = UpdateLogsAsync();
    }

    private List<AuditRecord> ApplyDateFilter(List<AuditRecord> audits)
    {
        if (SelectedDateFilter == "All Dates") return audits;

        DateTime now = DateTime.Now;
        return audits.Where(a => 
        {
            switch (SelectedDateFilter)
            {
                case "Today": return a.Date.Date == now.Date;
                case "This Week": 
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    return a.Date.Date >= startOfWeek.Date && a.Date.Date <= now.Date;
                case "This Month": 
                    int targetMonth = MonthDate?.Month ?? now.Month;
                    int targetYear = MonthDate?.Year ?? now.Year;
                    return a.Date.Month == targetMonth && a.Date.Year == targetYear;
                case "This Year": 
                    int year = !string.IsNullOrEmpty(SelectedYear) ? int.Parse(SelectedYear) : now.Year;
                    return a.Date.Year == year;
                case "Specific Date": return SpecificDate.HasValue && a.Date.Date == SpecificDate.Value.Date;
                case "Specific Period": return PeriodStartDate.HasValue && PeriodEndDate.HasValue && a.Date.Date >= PeriodStartDate.Value.Date && a.Date.Date <= PeriodEndDate.Value.Date;
                default: return true;
            }
        }).ToList();
    }

    private async Task UpdateLogsAsync()
    {
        var logs = await _dataService.GetActivityLogsAsync();
        RecentActivities.Clear();
        foreach (var log in logs.Take(6)) RecentActivities.Add(log);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedDateFilterChanged(string value) 
    {
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedBranchFilterChanged(string value) => ApplyFilters();
    
    partial void OnSpecificDateChanged(DateTime? value) 
    { 
        OnPropertyChanged(nameof(SpecificDateDisplay)); 
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters(); 
    }

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
    
    partial void OnSelectedYearChanged(string? value)
    {
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters();
    }

    partial void OnPeriodStartDateChanged(DateTime? value) 
    { 
        OnPropertyChanged(nameof(PeriodDisplay)); 
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters(); 
    }
    
    partial void OnPeriodEndDateChanged(DateTime? value) 
    { 
        OnPropertyChanged(nameof(PeriodDisplay)); 
        OnPropertyChanged(nameof(DateFilterSummary));
        ApplyFilters(); 
    }

    private string GetCategoryColor(string? category) => category switch
    {
        "Corporate" => "#6366F1",
        "SME" => "#A855F7",
        "Individual" => "#06B6D4",
        _ => "#94A3B8"
    };

    [RelayCommand] private void NewAuditRecord() => NavigateToAuditAssurance?.Invoke();
    [RelayCommand] private void RegisterCompany() => NavigateToCompanyRegistration?.Invoke();
    [RelayCommand] private void AddTeamMember() => NavigateToTeam?.Invoke();
    [RelayCommand] private void FileCIT() => NavigateToCIT?.Invoke();
    [RelayCommand] private void ViewClients() => NavigateToClients?.Invoke();
}
