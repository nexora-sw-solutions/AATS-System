using AATS.Server.Models;

namespace AATS.Server.Data;

public class InMemoryAppStore
{
    private readonly object _sync = new();

    public TeamMember CurrentUser { get; private set; }

    public List<NexoraRequest> NexoraRequests { get; }
    public List<TeamMember> TeamMembers { get; }
    public List<ClientRecord> Clients { get; }
    public List<ActivityLogEntry> ActivityLogs { get; }
    public Dictionary<string, List<AuditRecord>> AuditRecords { get; }
    public Dictionary<string, List<TaxRecord>> TaxRecords { get; }

    public InMemoryAppStore()
    {
        TeamMembers =
        [
            new TeamMember { Id = "TM-001", Username = "Kasun Perera", Email = "kasun@aats.com", Phone = "+94 77 123 4567", Branch = "Central", Role = "Admin", CreatedAt = new DateTime(2024, 1, 15) },
            new TeamMember { Id = "TM-002", Username = "Nimali Silva", Email = "nimali@aats.com", Phone = "+94 71 234 5678", Branch = "South", Role = "Staff", CreatedAt = new DateTime(2024, 2, 20) },
            new TeamMember { Id = "TM-003", Username = "Amila Bandara", Email = "amila@aats.com", Phone = "+94 70 345 6789", Branch = "West", Role = "Staff", CreatedAt = new DateTime(2024, 3, 10) }
        ];

        CurrentUser = Clone(TeamMembers[0]);

        Clients =
        [
            new ClientRecord { Id = "CLT-001", Name = "Titan Industries", Email = "titan.industries@example.com", Phone = "+94 77 111 2222", Branch = "Central", Category = "SME", TotalRevenue = 1250000m, DueAmount = 59200m, Status = "Active" },
            new ClientRecord { Id = "CLT-002", Name = "Astra Finance", Email = "info@astrafinance.com", Phone = "+94 71 333 4444", Branch = "South", Category = "Corporate", TotalRevenue = 2500000m, DueAmount = 0m, Status = "Active" },
            new ClientRecord { Id = "CLT-003", Name = "Orbit Logistics", Email = "info@orbitlogistics.lk", Phone = "+94 91 555 6666", Branch = "West", Category = "SME", TotalRevenue = 350000m, DueAmount = 25000m, Status = "Inactive" }
        ];

        NexoraRequests =
        [
            new NexoraRequest { Id = "NEX-001", ClientFirstName = "John", ClientLastName = "Doe", CompanyName = "TechNova Solutions", Service = "Accounting Software", Phone = "+94 77 XXX XXXX", Date = DateTime.Now.AddDays(-2) },
            new NexoraRequest { Id = "NEX-002", ClientFirstName = "Jane", ClientLastName = "Smith", CompanyName = "GreenLeaf Estates", Service = "Website", Phone = "+94 71 XXX XXXX", Date = DateTime.Now.AddDays(-1) }
        ];

        ActivityLogs =
        [
            new ActivityLogEntry { Action = "Export", Module = "Team", Branch = "Central", Details = "Exported team list to Excel.", Timestamp = DateTime.Now.AddHours(-1), User = "Kasun Perera" },
            new ActivityLogEntry { Action = "Print", Module = "Clients", Branch = "South", Details = "Generated print report for clients.", Timestamp = DateTime.Now.AddHours(-2), User = "Nimali Silva" },
            new ActivityLogEntry { Action = "Create", Module = "Registration", Branch = "West", Details = "New company registration created.", Timestamp = DateTime.Now.AddHours(-4), User = "Amila Bandara" }
        ];

        AuditRecords = new Dictionary<string, List<AuditRecord>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Audit & Assurance"] =
            [
                new AuditRecord
                {
                    ID = "REC-001",
                    Date = new DateTime(2024, 1, 15),
                    ClientName = "Acme Corp",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    Process = "BOOKKEEP",
                    Branch = "South",
                    CurrentStep = 1,
                    Assignment = "Audit for FY 2023-2024",
                    SourceDocuments = [new SourceDocument { FileName = "Invoice-JAN.pdf", Description = "January invoices" }]
                }
            ],
            ["Internal Audit"] =
            [
                new AuditRecord { ID = "IA-001", Date = new DateTime(2024, 1, 15), ClientName = "Global Tech", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "2024 Year", Assignment = "Internal controls assessment." }
            ],
            ["Forensic Audit"] = [],
            ["Management Accountings"] = [],
            ["Tax Accountings"] = [],
            ["Internal Control Systems & Outsourcing"] = [],
            ["Others"] = [],
            ["Company Registration"] =
            [
                new AuditRecord
                {
                    ID = "CR-001",
                    Date = new DateTime(2024, 1, 15),
                    ClientName = "Alice Smith",
                    Company = "Alice's Bakery",
                    PaymentStatus = "Paid",
                    Process = "COMPLETED",
                    Branch = "South",
                    Address = "No. 45, Flower Road, Colombo 07",
                    Email = "alice.bakery@gmail.com",
                    PhoneNo = "077 123 4567",
                    Assignment = "Private Limited"
                }
            ],
            ["EPF / ETF"] =
            [
                new AuditRecord
                {
                    ID = "EPF-001",
                    Date = new DateTime(2024, 1, 15),
                    ClientName = "Alice Smith",
                    Company = "Alice's Bakery",
                    NoOfStaffs = 2,
                    Branch = "South",
                    StaffList =
                    [
                        new StaffMember { StaffId = "STF-0001", StaffName = "John Doe", Phone = "0771234567", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0002", StaffName = "Jane Smith", Phone = "0772345678", Process = "COMPLETE" }
                    ]
                }
            ],
            ["Trade License"] = [],
            ["Trade Mark"] = [],
            ["Import / Export"] = [],
            ["BOI"] = [],
            ["HR and Management Consulting"] = [],
            ["Business Plan and Asset Valuation Consulting"] = [],
            ["Secretarial Others"] = [],
            ["Tax Others"] = []
        };

        TaxRecords = new Dictionary<string, List<TaxRecord>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Corporate Income Tax (CIT)"] =
            [
                new TaxRecord { ID = "CIT-2024-001", ClientName = "Acme Corp", ClientNameSub = "CL-001", DINNo = "DIN-8821", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) }
            ],
            ["Individual Income Tax (IIT)"] =
            [
                new TaxRecord { ID = "IIT-2024-001", ClientName = "Jane Smith", ClientNameSub = "CL-101", DINNo = "TIN-8821", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) }
            ],
            ["Social Security Contribution Levy (SSCL)"] =
            [
                new TaxRecord { ID = "SSCL-2024-001", ClientName = "Omega Industries", ClientNameSub = "CL-301", DINNo = "SSCL-555444", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) }
            ],
            ["Value Added Tax (VAT)"] =
            [
                new TaxRecord { ID = "VAT-2024-001", ClientName = "ABC Trading", ClientNameSub = "CL-201", DINNo = "VAT-998877", TaxPeriod = "2024-01", Status = "Paid", Branch = "Central", Date = new DateTime(2026, 1, 26) }
            ],
            ["Withholding Tax (WHT)"] =
            [
                new TaxRecord { ID = "WHT-2024-001", ClientName = "Gamma Services", ClientNameSub = "CL-401", DINNo = "WHT-777888", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) }
            ]
        };
    }

    public TeamMember? Authenticate(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        lock (_sync)
        {
            var member = TeamMembers.FirstOrDefault(m =>
                string.Equals(m.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));

            if (member == null)
            {
                return null;
            }

            CurrentUser = Clone(member);
            return Clone(CurrentUser);
        }
    }

    public TeamMember? UpdateCurrentUser(string id, ProfileUpdateRequest request)
    {
        lock (_sync)
        {
            var existing = TeamMembers.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                return null;
            }

            existing.Username = request.Username;
            existing.Email = request.Email;
            existing.Phone = request.Phone;
            existing.Branch = request.Branch;
            existing.Role = request.Role;
            existing.CreatedAt = request.CreatedAt;

            CurrentUser = Clone(existing);
            return Clone(CurrentUser);
        }
    }

    public string NormalizeAuditCategory(string category) => category switch
    {
        "Import and Export Clearance" => "Import / Export",
        "Audit Others" => "Others",
        _ => category
    };

    public string NormalizeTaxCategory(string category) => category;

    public static TeamMember Clone(TeamMember member) => new()
    {
        Id = member.Id,
        Username = member.Username,
        Email = member.Email,
        Phone = member.Phone,
        Branch = member.Branch,
        Role = member.Role,
        CreatedAt = member.CreatedAt
    };
}
