using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AATS.Desktop.ViewModels;

public record BranchActivity(string Branch, int Count);
public record StatusCount(string Status, int Count, string Color);
public record ChartSegment(double StartAngle, double SweepAngle, string Color);

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDataService _dataService;

    public bool IsAdmin => MainViewModel.Instance?.IsAdmin ?? false;

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

    // Tab index for merged Module Summary / Recent Activity card (0 = Module Summary, 1 = Recent Activity)
    [ObservableProperty] private int _selectedTabIndex;

    public ObservableCollection<BranchActivity> BranchActivities { get; } = new();
    public ObservableCollection<StatusCount> PaymentStatuses { get; } = new();
    public ObservableCollection<StatusCount> ClientCategories { get; } = new();
    public ObservableCollection<ActivityLogEntry> RecentActivities { get; } = new();
    
    // Dynamic analytical chart segment collections
    public ObservableCollection<ChartSegment> PaymentStatusSegments { get; } = new();
    public ObservableCollection<ChartSegment> ClientCategorySegments { get; } = new();

    // Live Telemetry Chart (Sliding window of 30 points)
    private readonly ITelemetryDataService _telemetryService;
    private const int MaxVisiblePoints = 30;
    public ObservableCollection<ISeries> TelemetrySeries { get; set; } = new();
    public ObservableCollection<ObservablePoint> RealtimePoints { get; set; } = new();
    private double _currentSeconds = 0;
    public Axis[] XAxes { get; set; } =
    [
        new Axis
        {
            IsVisible = true,
            Name = "Time (Seconds)",
            NamePadding = new Padding(0, 5, 0, 0),
            NamePaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
            NameTextSize = 13,
            LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
            TextSize = 11,
            MinStep = 5,
            ForceStepToMin = true,
            ShowSeparatorLines = true,
            Labeler = value => $"{value:0}s",
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8").WithAlpha(35))
        }
    ];
    public Axis[] YAxes { get; set; } =
    [
        new Axis
        {
            IsVisible = true,
            Name = "Response Latency (ms)",
            NamePadding = new Padding(0, 0, 5, 0),
            NamePaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
            NameTextSize = 13,
            LabelsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8")),
            TextSize = 11,
            MinLimit = 0,
            MaxLimit = 60,
            MinStep = 15,
            ForceStepToMin = true,
            ShowSeparatorLines = true,
            AnimationsSpeed = TimeSpan.Zero,
            Labeler = value => $"{value:0}",
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#94A3B8").WithAlpha(35))
        }
    ];

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
    public Action? NavigateToOutstandingBalances { get; set; }

    public DashboardViewModel()
    {
        _dataService = DataService.Instance;
        _telemetryService = SignalRTelemetryDataService.Instance;
        
        // Default filter selections
        _selectedDateFilter = DateFilters[0];
        _selectedStatusFilter = StatusFilters[0];
        _selectedBranchFilter = BranchFilters[0];

        // Populate initial baseline values for live telemetry chart
        for (int i = 0; i < MaxVisiblePoints; i++)
        {
            _currentSeconds += 1;
            RealtimePoints.Add(new ObservablePoint(_currentSeconds, 25));
        }

        var emeraldColor = SKColor.Parse("#10B981");
        TelemetrySeries = new ObservableCollection<ISeries>
        {
            new LineSeries<ObservablePoint>
            {
                Values = RealtimePoints,
                Fill = new SolidColorPaint(emeraldColor.WithAlpha(35)),
                Stroke = new SolidColorPaint(emeraldColor, 2f),
                GeometrySize = 0, // Hides point dots for a clean line
                LineSmoothness = 0.55
            }
        };

        _telemetryService.TelemetryTickReceived += AddPointToChart;
        _ = _telemetryService.StartStreamingAsync();

        _ = LoadDashboardDataAsync();
    }

    public void AddPointToChart(double value)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _currentSeconds += 1;
            RealtimePoints.Add(new ObservablePoint(_currentSeconds, Math.Round(value, 1)));
            if (RealtimePoints.Count > MaxVisiblePoints)
            {
                RealtimePoints.RemoveAt(0);
            }
        });
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Dispatch all API calls concurrently
            var clientsTask = _dataService.GetClientsAsync();
            var teamMembersTask = _dataService.GetTeamMembersAsync();

            var auditCategories = new[] { "Audit & Assurance", "Internal Audit", "Others", "Forensic Audit & Investigation", "Internal Control Systems & Outsourcing", "Management Accountings", "Tax Accountings" };

            // Pass enrich: false to optimize performance
            var auditTasks = auditCategories.Select(cat => _dataService.GetAuditRecordsAsync(cat, enrich: false)).ToList();
            var totalSecretarialTask = _dataService.GetTotalSecretarialRecordsAsync();

            // Await parallel execution of all requests
            await Task.WhenAll(
                new Task[] { clientsTask, teamMembersTask, totalSecretarialTask }
                .Concat(auditTasks)
            );

            // Assign results
            _allClients = await clientsTask;
            TotalTeamMembers = (await teamMembersTask).Count;

            _allAudits.Clear();
            foreach (var task in auditTasks)
            {
                _allAudits.AddRange(await task);
            }

            TotalSecretarialRecords = await totalSecretarialTask;

            // Check for overdue pending cheques
            await CheckOverdueChequesAsync();

            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
    }

    private async Task CheckOverdueChequesAsync()
    {
        try
        {
            var outstandingBalances = await _dataService.GetOutstandingBalancesAsync();
            if (outstandingBalances == null) return;

            foreach (var ob in outstandingBalances)
            {
                if (ob.ChequeDetails == null) continue;

                foreach (var cheque in ob.ChequeDetails)
                {
                    if (cheque.Status?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true &&
                        cheque.ChequeDate.Date < DateTime.Today)
                    {
                        string actionMessage = $"Cheque {cheque.ChequeNumber} from {ob.ClientName} (LKR {cheque.Amount:N0}) is overdue (Date: {cheque.ChequeDate:dd/MM/yyyy})";
                        
                        // Check if notification already exists
                        bool exists = NotificationService.Instance.Notifications.Any(n => 
                            n.UserName == "Cheque Overdue" && n.Action == actionMessage);

                        if (!exists)
                        {
                            NotificationService.Instance.AddNotification("Cheque Overdue", actionMessage);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking overdue cheques: {ex.Message}");
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
            (SelectedBranchFilter == "All Branch" || string.Equals(c.Branch?.Trim(), SelectedBranchFilter, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        var filteredAudits = _allAudits.Where(a => 
            (string.IsNullOrEmpty(SearchText) || (a.ClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (SelectedBranchFilter == "All Branch" || string.Equals(a.Branch?.Trim(), SelectedBranchFilter, StringComparison.OrdinalIgnoreCase)) &&
            (SelectedStatusFilter == "All Status" || string.Equals(a.PaymentStatus?.Trim(), SelectedStatusFilter, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        // Apply Date Filters
        filteredAudits = ApplyDateFilter(filteredAudits);

        // 2. Update KPIs
        TotalClients = filteredClients.Count;
        ActiveClients = filteredClients.Count(c => string.Equals(c.Status?.Trim(), "Active", StringComparison.OrdinalIgnoreCase));
        InactiveClients = TotalClients - ActiveClients;
        
        decimal revenue = filteredClients.Sum(c => c.TotalRevenue);
        TotalRevenue = $"LKR {revenue:N0}";
        
        decimal dues = filteredClients.Sum(c => c.DueAmount);
        OutstandingBalance = $"LKR {dues:N0}";
        ClientsWithDues = filteredClients.Count(c => c.DueAmount > 0);

        TotalAuditRecords = filteredAudits.Count;
        PaidAudits = filteredAudits.Count(a => string.Equals(a.PaymentStatus?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase));
        PartialAudits = filteredAudits.Count(a => string.Equals(a.PaymentStatus?.Trim(), "Partial", StringComparison.OrdinalIgnoreCase));
        UnpaidAudits = filteredAudits.Count(a => string.Equals(a.PaymentStatus?.Trim(), "Unpaid", StringComparison.OrdinalIgnoreCase));

        // 3. Update Charts
        var branches = filteredAudits.GroupBy(a => NormalizeString(a.Branch))
                                    .Select(g => new BranchActivity(g.Key, g.Count()))
                                    .OrderByDescending(b => b.Count);
        BranchActivities.Clear();
        foreach (var b in branches) BranchActivities.Add(b);

        PaymentStatuses.Clear();
        PaymentStatuses.Add(new StatusCount("Paid", PaidAudits, "#10B981"));
        PaymentStatuses.Add(new StatusCount("Partial", PartialAudits, "#F59E0B"));
        PaymentStatuses.Add(new StatusCount("Unpaid", UnpaidAudits, "#EF4444"));

        // Generate dynamic Payment Status segments
        PaymentStatusSegments.Clear();
        if (TotalAuditRecords > 0)
        {
            double startAngle = -90;
            foreach (var status in PaymentStatuses)
            {
                double sweep = (double)status.Count / TotalAuditRecords * 360;
                if (sweep > 0)
                {
                    PaymentStatusSegments.Add(new ChartSegment(startAngle, sweep, status.Color));
                    startAngle += sweep;
                }
            }
        }

        var catGroups = filteredClients.GroupBy(c => NormalizeString(c.Category))
                                    .Select(g => new StatusCount(g.Key, g.Count(), GetCategoryColor(g.Key)))
                                    .ToList();
        ClientCategories.Clear();
        foreach (var c in catGroups) ClientCategories.Add(c);

        // Generate dynamic Client Category segments
        ClientCategorySegments.Clear();
        if (TotalClients > 0)
        {
            double startAngle = -90;
            foreach (var cat in catGroups)
            {
                double sweep = (double)cat.Count / TotalClients * 360;
                if (sweep > 0)
                {
                    ClientCategorySegments.Add(new ChartSegment(startAngle, sweep, cat.Color));
                    startAngle += sweep;
                }
            }
        }

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

    private string NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Unknown";
        
        var normalized = value.Trim();
        
        if (normalized.Equals("Suspended", StringComparison.OrdinalIgnoreCase) || 
            normalized.Equals("Suspend", StringComparison.OrdinalIgnoreCase)) 
            return "Suspend";
            
        if (normalized.Equals("Blacklisted", StringComparison.OrdinalIgnoreCase) || 
            normalized.Equals("Black Listed", StringComparison.OrdinalIgnoreCase)) 
            return "Blacklisted";

        if (normalized.Equals("SME", StringComparison.OrdinalIgnoreCase))
            return "SME";

        // Title Casing
        if (normalized.Length == 1) return normalized.ToUpperInvariant();
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
    }

    private string GetCategoryColor(string category) => category switch
    {
        "Active" or "Loyal" => "#10B981",
        "Suspend" => "#F59E0B",
        "Blacklisted" => "#6B7280",
        "Corporate" => "#6366F1",
        "Sme" or "SME" => "#A855F7",
        "Individual" => "#06B6D4",
        _ => GetDynamicColor(category)
    };

    private string GetDynamicColor(string? name)
    {
        if (string.IsNullOrEmpty(name)) return "#94A3B8";
        string[] colors = { "#F59E0B", "#EC4899", "#8B5CF6", "#14B8A6", "#3B82F6" };
        int hash = Math.Abs(name.GetHashCode());
        return colors[hash % colors.Length];
    }

    [RelayCommand] private void SelectModuleSummaryTab() => SelectedTabIndex = 0;
    [RelayCommand] private void SelectRecentActivityTab() => SelectedTabIndex = 1;

    [RelayCommand] private void NewAuditRecord() => NavigateToAuditAssurance?.Invoke();
    [RelayCommand] private void RegisterCompany() => NavigateToCompanyRegistration?.Invoke();
    [RelayCommand] private void AddTeamMember() => NavigateToTeam?.Invoke();
    [RelayCommand] private void FileCIT() => NavigateToCIT?.Invoke();
    [RelayCommand] private void ViewClients() => NavigateToClients?.Invoke();
    [RelayCommand] private void OpenOutstandingBalances() => NavigateToOutstandingBalances?.Invoke();

    [RelayCommand] private void ViewAccountsAndAudit() => NavigateToAuditAssurance?.Invoke();
    [RelayCommand] private void ViewSecretarialAndAdvisory() => NavigateToCompanyRegistration?.Invoke();
    [RelayCommand] private void ViewTaxFiling() => NavigateToCIT?.Invoke();
    [RelayCommand] private void ViewTeamManagement() => NavigateToTeam?.Invoke();
}
