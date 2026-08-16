using System;
using System.Collections.Generic;
using AATS.Desktop.Models;

namespace AATS.Desktop.Data
{
    public static class MockData
    {
        static MockData()
        {
            if (AuditRecords != null)
            {
                foreach (var list in AuditRecords.Values)
                {
                    if (list != null)
                    {
                        foreach (var record in list)
                        {
                            if (record != null)
                            {
                                record.SourceDocuments = null;
                            }
                        }
                    }
                }
            }
        }

        
        public static List<string> Currencies { get; } = new List<string> { "LKR", "USD", "EUR", "GBP", "AUD" };
        public static List<string> Countries { get; } = new List<string>
        {
            "Sri Lanka",
            "India",
            "United States",
            "United Kingdom",
            "Australia",
            "Canada",
            "Singapore",
            "United Arab Emirates",
            "China",
            "Japan"
        };
public static TeamMember CurrentUser { get; } = new TeamMember
        {
            Id = "TM-001",
            Username = "Kasun Perera",
            Email = "kasun@aats.com",
            Phone = "+94 77 123 4567",
            Branch = "Central",
            Role = "Admin",
            CreatedAt = new DateTime(2024, 1, 15)
        };

        public static List<NexoraRequest> NexoraRequests { get; } = new()
        {
            new NexoraRequest { Id = "NEX-001", ClientFirstName = "John", ClientLastName = "Doe", CompanyName = "TechNova Solutions", Service = "Accounting Software", Phone = "+94 77 XXX XXXX", Date = DateTime.Now.AddDays(-2) },
            new NexoraRequest { Id = "NEX-002", ClientFirstName = "Jane", ClientLastName = "Smith", CompanyName = "GreenLeaf Estates", Service = "Website", Phone = "+94 71 XXX XXXX", Date = DateTime.Now.AddDays(-1) },
            new NexoraRequest { Id = "NEX-003", ClientFirstName = "Mike", ClientLastName = "Ross", CompanyName = "Urban Cafe", Service = "POS System", Phone = "+94 76 XXX XXXX", Date = DateTime.Now },
            new NexoraRequest { Id = "NEX-004", ClientFirstName = "Sarah", ClientLastName = "Cole", CompanyName = "LogiTrans Pvt Ltd", Service = "Payroll Management", Phone = "+94 70 XXX XXXX", Date = DateTime.Now },
        };

        public static List<TeamMember> TeamMembers { get; } = new()
        {
            new TeamMember { Id = "TM-001", Username = "Kasun Perera", Email = "kasun@aats.com", Phone = "+94 77 123 4567", Branch = "Central", Role = "Admin", CreatedAt = new DateTime(2024, 1, 15) },
            new TeamMember { Id = "TM-002", Username = "Nimali Silva", Email = "nimali@aats.com", Phone = "+94 71 234 5678", Branch = "South", Role = "Audit and Assurance", CreatedAt = new DateTime(2024, 2, 20) },
            new TeamMember { Id = "TM-003", Username = "Amila Bandara", Email = "amila@aats.com", Phone = "+94 70 345 6789", Branch = "West", Role = "Secretarial and Advisory", CreatedAt = new DateTime(2024, 3, 10) },
            new TeamMember { Id = "TM-004", Username = "Suresh Fernando", Email = "suresh@aats.com", Phone = "+94 76 456 7890", Branch = "Northeast", Role = "Tax Filing", CreatedAt = new DateTime(2024, 4, 5) },
            new TeamMember { Id = "TM-005", Username = "Dilshan Jayasuriya", Email = "dilshan@aats.com", Phone = "+94 78 567 8901", Branch = "Central", Role = "All", CreatedAt = new DateTime(2024, 5, 12) },
            new TeamMember { Id = "TM-006", Username = "Chathurika De Silva", Email = "chathurika@aats.com", Phone = "+94 71 678 9012", Branch = "South", Role = "Admin", CreatedAt = new DateTime(2024, 6, 18) },
            new TeamMember { Id = "TM-007", Username = "Ruwan Kumara", Email = "ruwan@aats.com", Phone = "+94 75 789 0123", Branch = "West", Role = "Audit and Assurance", CreatedAt = new DateTime(2024, 7, 22) },
            new TeamMember { Id = "TM-008", Username = "Tharindu Gamage", Email = "tharindu@aats.com", Phone = "+94 72 890 1234", Branch = "Northeast", Role = "Secretarial and Advisory", CreatedAt = new DateTime(2024, 8, 30) },
            new TeamMember { Id = "TM-009", Username = "Nadeeka Wijesinghe", Email = "nadeeka@aats.com", Phone = "+94 77 901 2345", Branch = "Central", Role = "Tax Filing", CreatedAt = new DateTime(2024, 9, 14) },
            new TeamMember { Id = "TM-010", Username = "Isuru Madushan", Email = "isuru@aats.com", Phone = "+94 71 012 3456", Branch = "South", Role = "All", CreatedAt = new DateTime(2024, 10, 5) },
            new TeamMember { Id = "TM-011", Username = "Gayani Perera", Email = "gayani@aats.com", Phone = "+94 70 123 4567", Branch = "West", Role = "Audit and Assurance", CreatedAt = new DateTime(2024, 11, 10) },
            new TeamMember { Id = "TM-012", Username = "Kelum Sampath", Email = "kelum@aats.com", Phone = "+94 76 234 5678", Branch = "Central", Role = "Secretarial and Advisory", CreatedAt = new DateTime(2024, 12, 1) },
            new TeamMember { Id = "TM-013", Username = "Malith Cooray", Email = "malith@aats.com", Phone = "+94 78 345 6789", Branch = "Northeast", Role = "Tax Filing", CreatedAt = new DateTime(2025, 1, 5) },
        };

        public static List<ClientRecord> Clients { get; } = new()
        {
            new ClientRecord { Id = "CLT-001", Name = "Titan Industries", Email = "titan.industries@example.com", Phone = "+91 77 XXX XXXX", Branch = "Central", Category = "SME", TotalRevenue = 1250000m, OutstandingBalance = 59200m, Status = "Active", Date = DateTime.Now },
            new ClientRecord { Id = "CLT-002", Name = "Astra Finance", Email = "info@astrafinance.com", Phone = "+94 71 XXX XXXX", Branch = "South", Category = "Corporate", TotalRevenue = 2500000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddDays(-1) },
            new ClientRecord { Id = "CLT-003", Name = "Ember Logistics", Email = "info@emberlogistics.com", Phone = "+91 11 XXX XXXX", Branch = "West", Category = "Corporate", TotalRevenue = 4500000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddDays(-3) },
            new ClientRecord { Id = "CLT-004", Name = "Solar Systems", Email = "info@solarsystems.lk", Phone = "+94 11 XXX XXXX", Branch = "Northeast", Category = "Corporate", TotalRevenue = 8500000m, OutstandingBalance = 500000m, Status = "Active", Date = DateTime.Now.AddDays(-10) },
            new ClientRecord { Id = "CLT-005", Name = "Velvet Retail", Email = "info@velvetretail.lk", Phone = "+94 71 XXX XXXX", Branch = "South", Category = "Corporate", TotalRevenue = 6200000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddMonths(-1) },
            new ClientRecord { Id = "CLT-006", Name = "Evolve Systems", Email = "info@evolvesystems.lk", Phone = "+94 76 XXX XXXX", Branch = "Central", Category = "SME", TotalRevenue = 950000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddMonths(-2) },
            new ClientRecord { Id = "CLT-007", Name = "Prism Retail", Email = "info@prismretail.lk", Phone = "+94 75 XXX XXXX", Branch = "West", Category = "SME", TotalRevenue = 600000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddMonths(-5) },
            new ClientRecord { Id = "CLT-008", Name = "Orbit Logistics", Email = "info@orbitlogistics.lk", Phone = "+94 91 XXX XXXX", Branch = "Northeast", Category = "SME", TotalRevenue = 350000m, OutstandingBalance = 0, Status = "Inactive", Date = DateTime.Now.AddYears(-1) },
            new ClientRecord { Id = "CLT-009", Name = "Bridge Partners", Email = "info@bridgepartners.lk", Phone = "+94 21 XXX XXXX", Branch = "Central", Category = "SME", TotalRevenue = 2800000m, OutstandingBalance = 500000m, Status = "Active", Date = DateTime.Now.AddYears(-2) },
            new ClientRecord { Id = "CLT-010", Name = "Zenith Tech", Email = "info@zenithtech.lk", Phone = "+94 77 XXX XXXX", Branch = "South", Category = "SME", TotalRevenue = 1500000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddYears(-3) },
            new ClientRecord { Id = "CLT-011", Name = "Nexus Global", Email = "info@nexusglobal.lk", Phone = "+94 71 XXX XXXX", Branch = "West", Category = "Corporate", TotalRevenue = 12000000m, OutstandingBalance = 2500000m, Status = "Active", Date = DateTime.Now.AddDays(-5) },
            new ClientRecord { Id = "CLT-012", Name = "Quantum Solutions", Email = "info@quantum.lk", Phone = "+94 76 XXX XXXX", Branch = "Northeast", Category = "SME", TotalRevenue = 850000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddMonths(-3) },
            new ClientRecord { Id = "CLT-013", Name = "Apex Logistics", Email = "info@apex.lk", Phone = "+94 11 XXX XXXX", Branch = "Central", Category = "Corporate", TotalRevenue = 3300000m, OutstandingBalance = 0, Status = "Active", Date = DateTime.Now.AddYears(-1).AddMonths(-2) },
        };

        public static List<ActivityLogEntry> ActivityLogs { get; } = new()
        {
            new ActivityLogEntry { Action = "Export", Module = "Team", Branch = "Central", Details = "Exported team list (13 members) to Excel.", Timestamp = DateTime.Now.AddHours(-1) },
            new ActivityLogEntry { Action = "Print", Module = "Clients", Branch = "South", Details = "Generated print report for 13 clients.", Timestamp = DateTime.Now.AddHours(-2) },
            new ActivityLogEntry { Action = "Create", Module = "Team", Branch = "West", Details = "Added new member 'Chamara Silva'.", Timestamp = DateTime.Now.AddHours(-3) },
            new ActivityLogEntry { Action = "Update", Module = "Audit & Assurance", Branch = "Central", Details = "Modified record for 'Apex Corp'.", Timestamp = DateTime.Now.AddHours(-4) },
            new ActivityLogEntry { Action = "Delete", Module = "Team", Branch = "South", Details = "Removed member: Kasun Perera", Timestamp = DateTime.Now.AddHours(-5) },
            new ActivityLogEntry { Action = "Create", Module = "Clients", Branch = "Central", Details = "Registered new client: Titan Industries", Timestamp = DateTime.Now.AddDays(-1) },
            new ActivityLogEntry { Action = "Print", Module = "Audit & Assurance", Branch = "Northeast", Details = "Printed financial report for FY 2024.", Timestamp = DateTime.Now.AddDays(-1).AddHours(-2) },
            new ActivityLogEntry { Action = "Export", Module = "Audit & Assurance", Branch = "West", Details = "Exported audit summary to CSV.", Timestamp = DateTime.Now.AddDays(-1).AddHours(-5) },
            new ActivityLogEntry { Action = "Update", Module = "Team", Branch = "Central", Details = "Updated profile for member: Nimali Silva", Timestamp = DateTime.Now.AddDays(-2) },
            new ActivityLogEntry { Action = "Create", Module = "Registration", Branch = "South", Details = "New company registration: Quantum Quest", Timestamp = DateTime.Now.AddDays(-3) },
            new ActivityLogEntry { Action = "Delete", Module = "Clients", Branch = "Central", Details = "Deleted client: Old Business Ltd", Timestamp = DateTime.Now.AddDays(-3).AddHours(-6) },
            new ActivityLogEntry { Action = "Print", Module = "Team", Branch = "Central", Details = "Printed team hierarchy chart.", Timestamp = DateTime.Now.AddDays(-4) },
            new ActivityLogEntry { Action = "Export", Module = "Registration", Branch = "West", Details = "Exported pending registrations list.", Timestamp = DateTime.Now.AddDays(-4).AddHours(-8) },
            new ActivityLogEntry { Action = "Update", Module = "Clients", Branch = "Central", Details = "Updated contact info for Solar Systems.", Timestamp = DateTime.Now.AddDays(-5) },
        };
        public static Dictionary<string, List<AuditRecord>> AuditRecords { get; } = new()
        {
            ["Audit & Assurance"] = new()
            {
                new AuditRecord { ID = "REC-001", Date = new DateTime(2024, 01, 15), ClientName = "Acme Corp", PaymentStatus = "Paid", PaymentOption = "Online", Process = "BOOKKEEP", Branch = "South", CurrentStep = 1, Assignment = "Audit for FY 2023-2024. Complete verification of assets.", SourceDocuments = new() { new SourceDocument { FileName = "Invoice-JAN.pdf", Description = "January Invoices" }, new SourceDocument { FileName = "Bank-Stmt.pdf", Description = "BOC Bank Statement" } } },
                new AuditRecord { ID = "REC-002", Date = new DateTime(2024, 01, 16), ClientName = "Beta LLC", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "DRAFT", Branch = "West", CurrentStep = 2, Assignment = "Draft financial statements for Q4 2023.", SourceDocuments = new() { new SourceDocument { FileName = "Ledger-Q4.pdf", Description = "Q4 General Ledger" } } },
                new AuditRecord { ID = "REC-003", Date = new DateTime(2024, 01, 18), ClientName = "Gamma Inc", PaymentStatus = "Partial", PaymentOption = "Online", Process = "FINALIZE", Branch = "Central", CurrentStep = 3, Assignment = "Finalize audit report and management letter.", SourceDocuments = new() { new SourceDocument { FileName = "Trial-Balance.pdf", Description = "Trial Balance 2023" }, new SourceDocument { FileName = "Adj-Entries.pdf", Description = "Adjustment Entries" } } },
                new AuditRecord { ID = "REC-004", Date = new DateTime(2024, 01, 20), ClientName = "Delta Co", PaymentStatus = "Paid", PaymentOption = "Online", Process = "HANDOVER", Branch = "Northeast", CurrentStep = 4, Assignment = "Handover completed audit files to partner.", SourceDocuments = new() { new SourceDocument { FileName = "Audit-Report.pdf", Description = "Final Audit Report" } } },
                new AuditRecord { ID = "REC-005", Date = new DateTime(2024, 02, 05), ClientName = "Epsilon Enterprises", PaymentStatus = "Unpaid", PaymentOption = "Cheque", Process = "SUBMIT", Branch = "South", CurrentStep = 6, Assignment = "Submit final report to regulatory authority.", SourceDocuments = new() { new SourceDocument { FileName = "Submission-Form.pdf", Description = "Regulatory Submission" } } },
                new AuditRecord { ID = "REC-006", Date = new DateTime(2024, 02, 10), ClientName = "Zeta Partners", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "BOOKKEEP", Branch = "West", CurrentStep = 1, Assignment = "Initial bookkeeping and data entry for FY 2023.", SourceDocuments = new() { new SourceDocument { FileName = "Receipts-Feb.pdf", Description = "February Receipts" } } },
                new AuditRecord { ID = "REC-007", Date = new DateTime(2024, 02, 15), ClientName = "Eta Solutions", PaymentStatus = "Paid", PaymentOption = "Online", Process = "RETURN", Branch = "Central", CurrentStep = 5, Assignment = "Return audit queries to client for clarification.", SourceDocuments = new() { new SourceDocument { FileName = "Query-List.pdf", Description = "Audit Query List" }, new SourceDocument { FileName = "Response.pdf", Description = "Client Response" } } },
                new AuditRecord { ID = "REC-008", Date = new DateTime(2024, 03, 01), ClientName = "Theta Group", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "DRAFT", Branch = "Northeast", CurrentStep = 2, Assignment = "Prepare draft accounts for management review.", SourceDocuments = new() { new SourceDocument { FileName = "Chart-Accounts.pdf", Description = "Chart of Accounts" } } },
                new AuditRecord { ID = "REC-009", Date = new DateTime(2024, 03, 05), ClientName = "Iota Holdings", PaymentStatus = "Paid", PaymentOption = "Online", Process = "HANDOVER", Branch = "South", CurrentStep = 4, Assignment = "Complete handover of financial records.", SourceDocuments = new() { new SourceDocument { FileName = "Handover-Checklist.pdf", Description = "Handover Checklist" } } },
                new AuditRecord { ID = "REC-010", Date = new DateTime(2024, 03, 10), ClientName = "Kappa Systems", PaymentStatus = "Partial", PaymentOption = "Online", Process = "FINALIZE", Branch = "West", CurrentStep = 3, Assignment = "Finalize system audit and IT controls review.", SourceDocuments = new() { new SourceDocument { FileName = "IT-Controls.pdf", Description = "IT Controls Report" } } },
                new AuditRecord { ID = "REC-011", Date = new DateTime(2024, 03, 12), ClientName = "Lambda Corp", PaymentStatus = "Paid", PaymentOption = "Cash", Process = "BOOKKEEP", Branch = "Central", CurrentStep = 1, Assignment = "Begin bookkeeping for new fiscal year.", SourceDocuments = new() { new SourceDocument { FileName = "Opening-Bal.pdf", Description = "Opening Balances" } } },
                new AuditRecord { ID = "REC-012", Date = new DateTime(2024, 03, 15), ClientName = "Mu Industries", PaymentStatus = "Unpaid", PaymentOption = "Cheque", Process = "DRAFT", Branch = "South", CurrentStep = 2, Assignment = "Draft interim financial statements.", SourceDocuments = new() { new SourceDocument { FileName = "Interim-Report.pdf", Description = "Interim Financial Report" } } },
                new AuditRecord { ID = "REC-013", Date = new DateTime(2024, 03, 18), ClientName = "Nu Tech", PaymentStatus = "Partial", PaymentOption = "Online", Process = "SUBMIT", Branch = "Northeast", CurrentStep = 6, Assignment = "Submit technology audit findings.", SourceDocuments = new() { new SourceDocument { FileName = "Tech-Audit.pdf", Description = "Technology Audit Report" }, new SourceDocument { FileName = "Findings.pdf", Description = "Key Findings Summary" } } },
            },
            ["Internal Audit"] = new()
            {
                new AuditRecord { ID = "IA-001", Date = new DateTime(2024, 01, 15), ClientName = "Global Tech", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "2024 Year", Assignment = "Internal controls assessment and risk evaluation.", SourceDocuments = new() { new SourceDocument { FileName = "Risk-Assessment.pdf", Description = "Risk Assessment Report" } } },
                new AuditRecord { ID = "IA-002", Date = new DateTime(2024, 01, 16), ClientName = "Nexus Corp", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "MEETING COMPLETE", Branch = "West", CurrentStep = 2, Period = "2024 Year", Assignment = "Risk assessment and verification", SourceDocuments = new() { new SourceDocument { FileName = "Meeting-Notes.pdf", Description = "Meeting Minutes" }, new SourceDocument { FileName = "Action-Items.pdf", Description = "Action Items List" } } },
                new AuditRecord { ID = "IA-003", Date = new DateTime(2024, 01, 18), ClientName = "Vertex Solutions", PaymentStatus = "Partial", PaymentOption = "Online", Process = "-", Branch = "Central", CurrentStep = 0, Period = "2024 Year", Assignment = "Compliance audit for Q4 2023.", SourceDocuments = new() { new SourceDocument { FileName = "Compliance-Checklist.pdf", Description = "Compliance Checklist" } } },
                new AuditRecord { ID = "IA-004", Date = new DateTime(2024, 01, 20), ClientName = "Skyline Ventures", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "Northeast", CurrentStep = 1, Period = "2024 Year", Assignment = "Operational efficiency review and recommendations.", SourceDocuments = new() { new SourceDocument { FileName = "Efficiency-Report.pdf", Description = "Efficiency Report" } } },
                new AuditRecord { ID = "IA-005", Date = new DateTime(2024, 02, 05), ClientName = "Oceanic Ltd", PaymentStatus = "Unpaid", PaymentOption = "Cheque", Process = "-", Branch = "South", CurrentStep = 0, Period = "Jan 2024", Assignment = "Financial controls testing.", SourceDocuments = new() { new SourceDocument { FileName = "Controls-Test.pdf", Description = "Controls Testing Report" } } },
                new AuditRecord { ID = "IA-006", Date = new DateTime(2024, 02, 10), ClientName = "Peak Performance", PaymentStatus = "Paid", PaymentOption = "Cash", Process = "REPORTING", Branch = "West", CurrentStep = 1, Period = "2024 Year", Assignment = "Performance audit and KPI evaluation.", SourceDocuments = new() { new SourceDocument { FileName = "KPI-Report.pdf", Description = "KPI Evaluation" } } },
                new AuditRecord { ID = "IA-007", Date = new DateTime(2024, 02, 15), ClientName = "Swift Systems", PaymentStatus = "Partial", PaymentOption = "Online", Process = "MEETING COMPLETE", Branch = "Central", CurrentStep = 2, Period = "Feb 2024", Assignment = "IT systems audit and cybersecurity review.", SourceDocuments = new() { new SourceDocument { FileName = "IT-Audit.pdf", Description = "IT Audit Findings" }, new SourceDocument { FileName = "Cyber-Review.pdf", Description = "Cybersecurity Assessment" } } },
                new AuditRecord { ID = "IA-008", Date = new DateTime(2024, 03, 01), ClientName = "Golden Gate", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "-", Branch = "Northeast", CurrentStep = 0, Period = "2024 Year", Assignment = "Procurement process audit.", SourceDocuments = new() { new SourceDocument { FileName = "Procurement-Audit.pdf", Description = "Procurement Audit" } } },
                new AuditRecord { ID = "IA-009", Date = new DateTime(2024, 03, 05), ClientName = "Silver Lining", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "Mar 2024", Assignment = "Inventory management review.", SourceDocuments = new() { new SourceDocument { FileName = "Inventory-Report.pdf", Description = "Inventory Report" } } },
                new AuditRecord { ID = "IA-010", Date = new DateTime(2024, 03, 10), ClientName = "Bronze Age", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "MEETING COMPLETE", Branch = "West", CurrentStep = 2, Period = "2024 Year", Assignment = "Human resources compliance audit.", SourceDocuments = new() { new SourceDocument { FileName = "HR-Compliance.pdf", Description = "HR Compliance Report" } } },
            },
            ["Audit Others"] = new()
            {
                new AuditRecord { ID = "AUD-OTH-001", Date = new DateTime(2024, 02, 21), ClientName = "Alice Smith", Company = "Alice's Bakery", Assignment = "Annual Return", PaymentStatus = "Paid", Branch = "South" },
                new AuditRecord { ID = "AUD-OTH-002", Date = new DateTime(2024, 02, 18), ClientName = "Bob Jones", Company = "Tech Solutions", Assignment = "Board Resolution", PaymentStatus = "Unpaid", Branch = "West" },
                new AuditRecord { ID = "AUD-OTH-003", Date = new DateTime(2024, 02, 16), ClientName = "Charlie Brown", Company = "Charlie's Design", Assignment = "Document Review", PaymentStatus = "Partial", Branch = "Central" },
            },
            ["Tax Others"] = new()
            {
                new AuditRecord { ID = "TAX-OTH-001", Date = new DateTime(2024, 03, 01), ClientName = "David Wilson", Company = "Wilson Logistics", Assignment = "Tax Review", PaymentStatus = "Paid", Branch = "Northeast" },
                new AuditRecord { ID = "TAX-OTH-002", Date = new DateTime(2024, 03, 05), ClientName = "Eva Martinez", Company = "Eva's Event Planning", Assignment = "VAT Filing", PaymentStatus = "Unpaid", Branch = "South" },
            },
            ["Secretarial Others"] = new()
            {
                new AuditRecord { ID = "SEC-OTH-001", Date = new DateTime(2024, 03, 10), ClientName = "Frank Thomas", Company = "Thomas Construction", Assignment = "Secretarial Filing", PaymentStatus = "Partial", Branch = "West" },
                new AuditRecord { ID = "SEC-OTH-002", Date = new DateTime(2024, 03, 15), ClientName = "Grace Lee", Company = "Graceful Gardens", Assignment = "Share Register", PaymentStatus = "Paid", Branch = "Central" },
            },
            ["Forensic Audit"] = new()
            {
                new AuditRecord { ID = "FA-001", Date = new DateTime(2024, 01, 20), ClientName = "Titan Industries", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "1 Month", Assignment = "Fraud investigation for manufacturing division.", SourceDocuments = new() { new SourceDocument { FileName = "Fraud-Analysis.pdf", Description = "Manufacturing Unit Analysis" } } },
                new AuditRecord { ID = "FA-002", Date = new DateTime(2024, 01, 22), ClientName = "Astra Finance", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "MEETING COMPLETE", Branch = "West", CurrentStep = 2, Period = "3 Months", Assignment = "Financial discrepancy audit.", SourceDocuments = new() { new SourceDocument { FileName = "Discrepancy-Report.pdf", Description = "Audit Findings" } } },
                new AuditRecord { ID = "FA-003", Date = new DateTime(2024, 02, 01), ClientName = "Ember Logistics", PaymentStatus = "Partial", PaymentOption = "Online", Process = "-", Branch = "Central", CurrentStep = 0, Period = "1 Year", Assignment = "Logistics contract verification.", SourceDocuments = new() { new SourceDocument { FileName = "Logistics-Audit.pdf", Description = "Audit Plan" } } },
                new AuditRecord { ID = "FA-004", Date = new DateTime(2024, 02, 10), ClientName = "Solar Systems", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "Northeast", CurrentStep = 1, Period = "6 Months", Assignment = "Asset misappropriation investigation.", SourceDocuments = new() { new SourceDocument { FileName = "Asset-Verify.pdf", Description = "Physical Verification" } } },
                new AuditRecord { ID = "FA-005", Date = new DateTime(2024, 02, 15), ClientName = "Velvet Retail", PaymentStatus = "Unpaid", PaymentOption = "Cheque", Process = "-", Branch = "South", CurrentStep = 0, Period = "Q1 2024", Assignment = "Inventory mismatch investigation.", SourceDocuments = new() { new SourceDocument { FileName = "Inventory-Audit.pdf", Description = "Discrepancy List" } } },
                new AuditRecord { ID = "FA-006", Date = new DateTime(2024, 02, 28), ClientName = "Aurora Solutions", PaymentStatus = "Paid", PaymentOption = "Cash", Process = "REPORTING", Branch = "West", CurrentStep = 1, Period = "Feb 2024", Assignment = "Payroll fraud detection project.", SourceDocuments = new() { new SourceDocument { FileName = "Payroll-Audit.pdf", Description = "Employee Records Review" } } },
                new AuditRecord { ID = "FA-007", Date = new DateTime(2024, 03, 05), ClientName = "Nebula Partners", PaymentStatus = "Partial", PaymentOption = "Online", Process = "MEETING COMPLETE", Branch = "Central", CurrentStep = 2, Period = "2024 Year", Assignment = "Corporate embezzlement audit.", SourceDocuments = new() { new SourceDocument { FileName = "Corporate-Audit.pdf", Description = "Bank Statement Review" } } },
                new AuditRecord { ID = "FA-008", Date = new DateTime(2024, 03, 12), ClientName = "Quest Global", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "-", Branch = "Northeast", CurrentStep = 0, Period = "3 Months", Assignment = "Procurement fraud investigation.", SourceDocuments = new() { new SourceDocument { FileName = "Procure-Audit.pdf", Description = "Vendor Analysis" } } },
                new AuditRecord { ID = "FA-009", Date = new DateTime(2024, 03, 18), ClientName = "Apex Limited", PaymentStatus = "Paid", PaymentOption = "Online", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "Mar 2024", Assignment = "Tax evasion suspicion audit.", SourceDocuments = new() { new SourceDocument { FileName = "Tax-Verify.pdf", Description = "Filing Verification" } } },
                new AuditRecord { ID = "FA-010", Date = new DateTime(2024, 03, 25), ClientName = "Summit Corp", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "MEETING COMPLETE", Branch = "West", CurrentStep = 2, Period = "2024 Year", Assignment = "Suspicious transaction monitoring review.", SourceDocuments = new() { new SourceDocument { FileName = "Trans-Audit.pdf", Description = "Alert History" } } },
            },
            ["Internal Control Systems & Outsourcing"] = new()
            {
                new AuditRecord { ID = "IC-001", Date = new DateTime(2024, 03, 10), ClientName = "Alpha Manufacturing", PaymentStatus = "Paid", PaymentOption = "Online", Process = "MEETING COMPLETE", Branch = "South", CurrentStep = 2, Period = "1 Year", Assignment = "Internal control audit for inventory management systems.", SourceDocuments = new() { new SourceDocument { FileName = "Inventory-Control.pdf", Description = "System Audit Report" } } },
                new AuditRecord { ID = "IC-002", Date = new DateTime(2024, 03, 12), ClientName = "Beta Services", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "REPORTING", Branch = "West", CurrentStep = 1, Period = "6 Months", Assignment = "Outsourcing compliance review for HR services.", SourceDocuments = new() { new SourceDocument { FileName = "HR-Compliance.pdf", Description = "Compliance Certificate" } } },
                new AuditRecord { ID = "IC-003", Date = new DateTime(2024, 03, 15), ClientName = "Gamma Retailers", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "-", Branch = "Central", CurrentStep = 0, Period = "1 Year", Assignment = "Initial internal control setup for retail branches." },
                new AuditRecord { ID = "IC-004", Date = new DateTime(2024, 03, 20), ClientName = "Quantum Quest", PaymentStatus = "Paid", PaymentOption = "Cheque", Process = "MEETING COMPLETE", Branch = "Northeast", CurrentStep = 2, Period = "2 Years", Assignment = "Internal audit for IT infrastructure and outsourcing.", SourceDocuments = new() { new SourceDocument { FileName = "IT-Audit.pdf", Description = "Infrastructure Audit" } } },
                new AuditRecord { ID = "IC-005", Date = new DateTime(2024, 03, 25), ClientName = "Pulse Partners", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "REPORTING", Branch = "South", CurrentStep = 1, Period = "1 Year", Assignment = "Financial control review for regional offices.", SourceDocuments = new() { new SourceDocument { FileName = "Finance-Control.pdf", Description = "Internal Report" } } },
                new AuditRecord { ID = "IC-006", Date = new DateTime(2024, 04, 01), ClientName = "Future Flow", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "-", Branch = "West", CurrentStep = 0, Period = "6 Months", Assignment = "Internal control policy development." },
                new AuditRecord { ID = "IC-007", Date = new DateTime(2024, 04, 05), ClientName = "Global Grid", PaymentStatus = "Paid", PaymentOption = "Online", Process = "MEETING COMPLETE", Branch = "Central", CurrentStep = 2, Period = "1 Year", Assignment = "Operational risk assessment and control review.", SourceDocuments = new() { new SourceDocument { FileName = "Op-Risk.pdf", Description = "Risk Assessment" } } },
                new AuditRecord { ID = "IC-008", Date = new DateTime(2024, 04, 10), ClientName = "Bright Build", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "REPORTING", Branch = "Northeast", CurrentStep = 1, Period = "1 Year", Assignment = "Internal audit for construction project management.", SourceDocuments = new() { new SourceDocument { FileName = "Project-Audit.pdf", Description = "Project Management Review" } } },
                new AuditRecord { ID = "IC-009", Date = new DateTime(2024, 04, 15), ClientName = "Streamline Soft", PaymentStatus = "Unpaid", PaymentOption = "Cheque", Process = "-", Branch = "South", CurrentStep = 0, Period = "1 Year", Assignment = "Software development life cycle (SDLC) control review." },
                new AuditRecord { ID = "IC-010", Date = new DateTime(2024, 05, 01), ClientName = "Peak Port", PaymentStatus = "Paid", PaymentOption = "Online", Process = "MEETING COMPLETE", Branch = "West", CurrentStep = 2, Period = "2 Years", Assignment = "Logistics and port operation control audit.", SourceDocuments = new() { new SourceDocument { FileName = "Port-Ops.pdf", Description = "Operation Control Audit" } } },
            },
            ["Management Accountings"] = new()
            {
                new AuditRecord { ID = "MA-001", Date = new DateTime(2024, 01, 10), ClientName = "Solaris Corp", PaymentStatus = "Paid", PaymentOption = "Online", Process = "BOOKKEEP", Branch = "Central", CurrentStep = 1, Period = "Jan 2024", Assignment = "Provision of management accounts for the fiscal year 2023. Includes full bookkeeping and reconciliation.", SourceDocuments = new() { new SourceDocument { FileName = "Bank_Statement_Jan.pdf", Description = "January bank statements" }, new SourceDocument { FileName = "Invoices_Q4.pdf", Description = "Expense invoices from Q4 2023" } } },
                new AuditRecord { ID = "MA-002", Date = new DateTime(2024, 01, 12), ClientName = "Luna Logistics", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "DRAFT ACCOUNT", Branch = "West", CurrentStep = 2, Period = "Feb 2024", Assignment = "Drafting of quarterly management reports.", SourceDocuments = new() { new SourceDocument { FileName = "Trial-Balance.pdf", Description = "Unadjusted Trial Balance" } } },
                new AuditRecord { ID = "MA-003", Date = new DateTime(2024, 02, 05), ClientName = "Nova Retail", PaymentStatus = "Partial", PaymentOption = "Online", Process = "FINALIZE", Branch = "Central", CurrentStep = 3, Period = "Mar 2024", Assignment = "Finalization of monthly accounts and tax estimation.", SourceDocuments = new() { new SourceDocument { FileName = "Ledger-Review.pdf", Description = "General Ledger Review" } } },
                new AuditRecord { ID = "MA-004", Date = new DateTime(2024, 02, 20), ClientName = "Apex Solutions", PaymentStatus = "Paid", PaymentOption = "Cheque", Process = "HANDOVER", Branch = "Northeast", CurrentStep = 4, Period = "Q1 2024", Assignment = "Handover of finalized management files to the client.", SourceDocuments = new() { new SourceDocument { FileName = "Handover-Cert.pdf", Description = "Confirmation of Document Receipt" } } },
                new AuditRecord { ID = "MA-005", Date = new DateTime(2024, 03, 01), ClientName = "Quantum Leap", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "BOOKKEEP", Branch = "South", CurrentStep = 1, Period = "Apr 2024", Assignment = "Routine bookkeeping service for service division.", SourceDocuments = new() { new SourceDocument { FileName = "Receipts-Apr.pdf", Description = "Monthly Expense Receipts" } } },
                new AuditRecord { ID = "MA-006", Date = new DateTime(2024, 03, 05), ClientName = "Pulse Media", PaymentStatus = "Paid", PaymentOption = "Cash", Process = "FINALIZE", Branch = "West", CurrentStep = 3, Period = "May 2024", Assignment = "Mid-year account finalization and analysis.", SourceDocuments = new() { new SourceDocument { FileName = "Mid-Year-Rev.pdf", Description = "June Performance Analysis" } } },
                new AuditRecord { ID = "MA-007", Date = new DateTime(2024, 03, 10), ClientName = "Future Flow", PaymentStatus = "Partial", PaymentOption = "Online", Process = "-", Branch = "Central", CurrentStep = 0, Period = "Jun 2024", Assignment = "Initial setup and bookkeeping for new agency.", SourceDocuments = new() { new SourceDocument { FileName = "Setup-Guide.pdf", Description = "Accounting Chart of Accounts" } } },
                new AuditRecord { ID = "MA-008", Date = new DateTime(2024, 03, 15), ClientName = "Global Goods", PaymentStatus = "Paid", PaymentOption = "Online", Process = "HANDOVER", Branch = "Northeast", CurrentStep = 4, Period = "Jul 2024", Assignment = "Handover of audited management accounts.", SourceDocuments = new() { new SourceDocument { FileName = "Final-Accounts.pdf", Description = "Signed Management Reports" } } },
                new AuditRecord { ID = "MA-009", Date = new DateTime(2024, 03, 22), ClientName = "Bright Beam", PaymentStatus = "Unpaid", PaymentOption = "Cash", Process = "BOOKKEEP", Branch = "South", CurrentStep = 1, Period = "Aug 2024", Assignment = "High-volume retail bookkeeping and reconciliation.", SourceDocuments = new() { new SourceDocument { FileName = "Sales-Report.pdf", Description = "Monthly POS Summary" } } },
                new AuditRecord { ID = "MA-010", Date = new DateTime(2024, 04, 01), ClientName = "Streamline Co", PaymentStatus = "Paid", PaymentOption = "Cheque", Process = "FINALIZE", Branch = "West", CurrentStep = 3, Period = "Sep 2024", Assignment = "Quarterly management drafting for unit.", SourceDocuments = new() { new SourceDocument { FileName = "Trial-Balance.pdf", Description = "Unadjusted Trial Balance" } } },
            },
            ["Tax Accountings"] = new()
            {
                new AuditRecord { ID = "TX-001", Date = new DateTime(2024, 03, 15), ClientName = "Green Energy Corp", PaymentStatus = "Paid", PaymentOption = "Online", Process = "SUBMIT", Branch = "South", CurrentStep = 5, Period = "FY 2023", Assignment = "Annual tax filing and calculation for fiscal year 2023. Includes income tax, GST, and payroll tax compliance.", SourceDocuments = new() { new SourceDocument { FileName = "Q1_Tax_Returns.pdf", Description = "Final tax returns for Q1" } } },
                new AuditRecord { ID = "TX-002", Date = new DateTime(2024, 03, 18), ClientName = "Blue Ocean Logistics", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "TAX AMOUNT", Branch = "West", CurrentStep = 2, Period = "Q1 2024", Assignment = "Quarterly tax estimation and provision.", SourceDocuments = new() { new SourceDocument { FileName = "Estimates.pdf", Description = "Quarterly Tax Estimates" } } },
                new AuditRecord { ID = "TX-003", Date = new DateTime(2024, 03, 20), ClientName = "TechNova Solutions", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "BOOKKEEP", Branch = "Central", CurrentStep = 1, Period = "Feb 2024", Assignment = "Bookkeeping for tax compliance purposes.", SourceDocuments = new() { new SourceDocument { FileName = "Expense-Log.pdf", Description = "Monthly Expense Log" } } },
                new AuditRecord { ID = "TX-004", Date = new DateTime(2024, 03, 22), ClientName = "Sunrise Retail", PaymentStatus = "Paid", PaymentOption = "Cheque", Process = "FINALIZE", Branch = "Northeast", CurrentStep = 3, Period = "Mar 2024", Assignment = "Finalizing tax returns and obtaining client signatures.", SourceDocuments = new() { new SourceDocument { FileName = "Draft-Returns.pdf", Description = "Draft Tax Returns" } } },
                new AuditRecord { ID = "TX-005", Date = new DateTime(2024, 03, 25), ClientName = "Pioneer Manufacturing", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "TAX PAID", Branch = "South", CurrentStep = 4, Period = "Q4 2023", Assignment = "Verification of tax payments and ledger reconciliation.", SourceDocuments = new() { new SourceDocument { FileName = "Payment-Proof.pdf", Description = "Tax Payment Confirmation" } } },
                new AuditRecord { ID = "TX-006", Date = new DateTime(2024, 04, 01), ClientName = "Quantum Quest", PaymentStatus = "Paid", PaymentOption = "Online", Process = "SUBMIT", Branch = "West", CurrentStep = 5, Period = "FY 2024", Assignment = "Full tax compliance package submission.", SourceDocuments = new() { new SourceDocument { FileName = "Final-Submission.pdf", Description = "Submitted Return Receipt" } } },
                new AuditRecord { ID = "TX-007", Date = new DateTime(2024, 04, 05), ClientName = "Pulse Partners", PaymentStatus = "Partial", PaymentOption = "Cash", Process = "TAX AMOUNT", Branch = "Central", CurrentStep = 2, Period = "Apr 2024", Assignment = "Income tax calculation based on draft accounts.", SourceDocuments = new() { new SourceDocument { FileName = "Calc-Sheet.pdf", Description = "Tax Computation Sheet" } } },
                new AuditRecord { ID = "TX-008", Date = new DateTime(2024, 04, 10), ClientName = "Future Flow", PaymentStatus = "Unpaid", PaymentOption = "Online", Process = "BOOKKEEP", Branch = "Northeast", CurrentStep = 1, Period = "May 2024", Assignment = "Preparation of records for upcoming tax audit.", SourceDocuments = new() { new SourceDocument { FileName = "Audit-Ready.pdf", Description = "Records Checklist" } } },
                new AuditRecord { ID = "TX-009", Date = new DateTime(2024, 04, 15), ClientName = "Global Grid", PaymentStatus = "Paid", PaymentOption = "Online", Process = "FINALIZE", Branch = "South", CurrentStep = 3, Period = "Jun 2024", Assignment = "Final review of international tax compliance.", SourceDocuments = new() { new SourceDocument { FileName = "Intl-Tax-Rev.pdf", Description = "Foreign Tax Assessment" } } },
                new AuditRecord { ID = "TX-010", Date = new DateTime(2024, 04, 20), ClientName = "Bright Build", PaymentStatus = "Partial", PaymentOption = "Cheque", Process = "TAX PAID", Branch = "West", CurrentStep = 4, Period = "Jul 2024", Assignment = "Ledger update with actual tax payments.", SourceDocuments = new() { new SourceDocument { FileName = "Ledger-Update.pdf", Description = "Tax Ledger Entry" } } },
            },
            ["BOI Registration"] = new()
            {
                new AuditRecord
                {
                    ID = "BOI-001",
                    Date = new DateTime(2024, 01, 25),
                    ClientName = "Tony Stark",
                    Company = "Stark Industries",
                    Country = "USA",
                    Branch = "South",
                    Process = "COMPLETED",
                    Code = "BOI-STARK-01",
                    TIN = "TIN-US-78123",
                    InvestmentValue = "$ 50,000,000",
                    CountryAddress = "Malibu, California, USA",
                    Assignment = "BOI Registration – Technology & Manufacturing",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    CurrentStep = 4,
                    Notes = "All BOI approval stages completed. Certificate issued.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "BR_Stark.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "TIN_Cert_Stark.pdf", Description = "TIN Certificate" },
                        new SourceDocument { FileName = "BOI_Approval_Stark.pdf", Description = "BOI Final Approval Letter" },
                        new SourceDocument { FileName = "Investment_Agreement_Stark.pdf", Description = "Investment Agreement" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-002",
                    Date = new DateTime(2024, 02, 15),
                    ClientName = "Bruce Wayne",
                    Company = "Wayne Enterprises",
                    Country = "USA",
                    Branch = "West",
                    Process = "IN PROGRESS",
                    Code = "BOI-WAYNE-01",
                    TIN = "TIN-US-90234",
                    InvestmentValue = "$ 75,000,000",
                    CountryAddress = "Gotham City, New Jersey, USA",
                    Assignment = "BOI Registration – Finance & Infrastructure",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    CurrentStep = 3,
                    Notes = "Awaiting BOI approval committee sign-off.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "BR_Wayne.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "Form01_Wayne.pdf", Description = "BOI Form 01 Application" },
                        new SourceDocument { FileName = "InvestmentPlan_Wayne.pdf", Description = "Investment Plan Document" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-003",
                    Date = new DateTime(2024, 02, 20),
                    ClientName = "Steve Rogers",
                    Company = "Shield Logistics",
                    Country = "USA",
                    Branch = "Central",
                    Process = "PENDING",
                    Code = "BOI-SHIELD-01",
                    TIN = "TIN-US-11456",
                    InvestmentValue = "$ 12,000,000",
                    CountryAddress = "Brooklyn, New York, USA",
                    Assignment = "BOI Registration – Logistics & Distribution",
                    PaymentStatus = "Unpaid",
                    PaymentOption = "Cash",
                    CurrentStep = 1,
                    Notes = "Application submitted. Documentation collection in progress.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Shield.pdf", Description = "BOI Application Form" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-004",
                    Date = new DateTime(2024, 02, 25),
                    ClientName = "Peter Parker",
                    Company = "Daily Bugle Media",
                    Country = "USA",
                    Branch = "Northeast",
                    Process = "IN PROGRESS",
                    Code = "BOI-BUGLE-01",
                    TIN = "TIN-US-23789",
                    InvestmentValue = "$ 5,500,000",
                    CountryAddress = "Queens, New York, USA",
                    Assignment = "BOI Registration – Media & Communications",
                    PaymentStatus = "Partial",
                    PaymentOption = "Online",
                    CurrentStep = 2,
                    Notes = "Documentation under review. Pending NIC verification.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Bugle.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_Bugle.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "NIC_Parker.pdf", Description = "Director NIC Copy" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-005",
                    Date = new DateTime(2024, 03, 01),
                    ClientName = "Natasha Romanoff",
                    Company = "Black Widow Security",
                    Country = "Russia",
                    Branch = "South",
                    Process = "COMPLETED",
                    Code = "BOI-BWS-01",
                    TIN = "TIN-RU-44567",
                    InvestmentValue = "$ 18,000,000",
                    CountryAddress = "Moscow, Russia",
                    Assignment = "BOI Registration – Security & Consulting",
                    PaymentStatus = "Paid",
                    PaymentOption = "Cheque",
                    CurrentStep = 4,
                    Notes = "Registration certificate issued. Compliance monitoring active.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_BWS.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_BWS.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "Approval_BWS.pdf", Description = "BOI Approval Letter" },
                        new SourceDocument { FileName = "Cert_BWS.pdf", Description = "BOI Registration Certificate" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-006",
                    Date = new DateTime(2024, 03, 05),
                    ClientName = "Thor Odinson",
                    Company = "Asgard Exports",
                    Country = "Norway",
                    Branch = "West",
                    Process = "IN PROGRESS",
                    Code = "BOI-ASGARD-01",
                    TIN = "TIN-NO-55890",
                    InvestmentValue = "$ 30,000,000",
                    CountryAddress = "Oslo, Norway",
                    Assignment = "BOI Registration – Export & Trade",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    CurrentStep = 3,
                    Notes = "Approval stage in progress. Supplementary documents requested.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Asgard.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_Asgard.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "InvestmentPlan_Asgard.pdf", Description = "Export Investment Plan" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-007",
                    Date = new DateTime(2024, 03, 10),
                    ClientName = "Wanda Maximoff",
                    Company = "WandaVision Props",
                    Country = "Sokovia",
                    Branch = "Central",
                    Process = "PENDING",
                    Code = "BOI-WVP-01",
                    TIN = "TIN-SK-67012",
                    InvestmentValue = "$ 8,200,000",
                    CountryAddress = "Novi Grad, Sokovia",
                    Assignment = "BOI Registration – Creative & Entertainment",
                    PaymentStatus = "Unpaid",
                    PaymentOption = "Cash",
                    CurrentStep = 1,
                    Notes = "Initial application received. Client briefing completed.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_WVP.pdf", Description = "BOI Application Form" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-008",
                    Date = new DateTime(2024, 03, 15),
                    ClientName = "Stephen Strange",
                    Company = "Sanctum Solutions",
                    Country = "USA",
                    Branch = "Northeast",
                    Process = "IN PROGRESS",
                    Code = "BOI-SANCT-01",
                    TIN = "TIN-US-78345",
                    InvestmentValue = "$ 22,500,000",
                    CountryAddress = "Greenwich Village, New York, USA",
                    Assignment = "BOI Registration – Healthcare & Research",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    CurrentStep = 2,
                    Notes = "Documents being verified by BOI assessment committee.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Sanctum.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_Sanctum.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "ResearchPlan_Sanctum.pdf", Description = "Research & Investment Plan" },
                        new SourceDocument { FileName = "TIN_Sanctum.pdf", Description = "TIN Certificate" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-009",
                    Date = new DateTime(2024, 03, 20),
                    ClientName = "T'Challa",
                    Company = "Wakanda Tech",
                    Country = "Wakanda",
                    Branch = "South",
                    Process = "COMPLETED",
                    Code = "BOI-WKND-01",
                    TIN = "TIN-WK-89234",
                    InvestmentValue = "$ 100,000,000",
                    CountryAddress = "Birnin Zana, Wakanda",
                    Assignment = "BOI Registration – Technology & Innovation",
                    PaymentStatus = "Paid",
                    PaymentOption = "Online",
                    CurrentStep = 4,
                    Notes = "Full registration complete. Vibranium tech investment approved.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Wakanda.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_Wakanda.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "Approval_Wakanda.pdf", Description = "BOI Approval Letter" },
                        new SourceDocument { FileName = "Cert_Wakanda.pdf", Description = "BOI Registration Certificate" },
                        new SourceDocument { FileName = "InvestmentAgreement_Wakanda.pdf", Description = "Investment Agreement" }
                    }
                },
                new AuditRecord
                {
                    ID = "BOI-010",
                    Date = new DateTime(2024, 03, 25),
                    ClientName = "Carol Danvers",
                    Company = "Starforce Travels",
                    Country = "USA",
                    Branch = "West",
                    Process = "IN PROGRESS",
                    Code = "BOI-STRF-01",
                    TIN = "TIN-US-90567",
                    InvestmentValue = "$ 15,750,000",
                    CountryAddress = "Los Angeles, California, USA",
                    Assignment = "BOI Registration – Aviation & Tourism",
                    PaymentStatus = "Partial",
                    PaymentOption = "Online",
                    CurrentStep = 2,
                    Notes = "Documentation submitted. Site inspection scheduled.",
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "Application_Starforce.pdf", Description = "BOI Application Form" },
                        new SourceDocument { FileName = "BR_Starforce.pdf", Description = "Business Registration Certificate" },
                        new SourceDocument { FileName = "TravelPlan_Starforce.pdf", Description = "Aviation Investment Plan" }
                    }
                },
            },
            ["Business Plan and Asset Valuation Consulting"] = new()
            {
                new AuditRecord 
                { 
                    ID = "BP-001", 
                    Date = new DateTime(2024, 01, 05), 
                    ClientName = "Tony Stark", 
                    Company = "Stark Industries", 
                    Branch = "Main", 
                    Assignment = "Business Strategy Plan", 
                    PaymentStatus = "Paid",
                    Code = "-",
                    CurrentStep = 1
                },
                new AuditRecord { ID = "BP-002", Date = new DateTime(2024, 01, 12), ClientName = "Steve Rogers", Company = "Shield Logistics", Branch = "West", PaymentStatus = "Unpaid" },
                new AuditRecord { ID = "BP-003", Date = new DateTime(2024, 01, 20), ClientName = "Natasha Romanoff", Company = "Red Room Tech", Branch = "Central", PaymentStatus = "Partial" },
                new AuditRecord { ID = "BP-004", Date = new DateTime(2024, 02, 02), ClientName = "Bruce Banner", Company = "Gamma Research", Branch = "Northeast", PaymentStatus = "Paid" },
                new AuditRecord { ID = "BP-005", Date = new DateTime(2024, 02, 10), ClientName = "Thor Odinson", Company = "Asgard Power", Branch = "South", PaymentStatus = "Unpaid" },
                new AuditRecord { ID = "BP-006", Date = new DateTime(2024, 02, 18), ClientName = "Clint Barton", Company = "Arrow Archery", Branch = "West", PaymentStatus = "Partial" },
                new AuditRecord { ID = "BP-007", Date = new DateTime(2024, 03, 01), ClientName = "Wanda Maximoff", Company = "Chaos Magic", Branch = "Central", PaymentStatus = "Paid" },
                new AuditRecord { ID = "BP-008", Date = new DateTime(2024, 03, 08), ClientName = "Vision", Company = "Synthezoid Systems", Branch = "Northeast", PaymentStatus = "Unpaid" },
                new AuditRecord { ID = "BP-009", Date = new DateTime(2024, 03, 15), ClientName = "Sam Wilson", Company = "Falcon Aviation", Branch = "South", PaymentStatus = "Partial" },
                new AuditRecord { ID = "BP-010", Date = new DateTime(2024, 03, 22), ClientName = "Bucky Barnes", Company = "Winter Security", Branch = "West", PaymentStatus = "Paid" },
            },
            ["EPF / ETF"] = new()
            {
                new AuditRecord 
                { 
                    ID = "EPF-001", 
                    Date = new DateTime(2024, 01, 15), 
                    ClientName = "Alice Smith", 
                    Company = "Alice's Bakery", 
                    NoOfStaffs = 5, 
                    Branch = "South",
                    StaffList = new()
                    {
                        new StaffMember 
                        { 
                            StaffId = "STF-0001", 
                            StaffName = "John Doe", 
                            Phone = "0771234567",
                            Process = "SUBMIT",
                            History = new()
                            {
                                new StaffHistory { Date = new DateTime(2024, 01, 15), Description = "EPF Contribution Jan 2024", Amount = 12500m },
                                new StaffHistory { Date = new DateTime(2024, 02, 15), Description = "EPF Contribution Feb 2024", Amount = 12500m },
                                new StaffHistory { Date = new DateTime(2024, 03, 15), Description = "EPF Contribution Mar 2024", Amount = 12500m }
                            }
                        },
                        new StaffMember { StaffId = "STF-0002", StaffName = "Jane Smith", Phone = "0772345678", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0003", StaffName = "Robert Johnson", Phone = "0773456789", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0004", StaffName = "Emily Davis", Phone = "0774567890", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0005", StaffName = "Michael Wilson", Phone = "0775678901", Process = "SUBMIT" }
                    }
                },
                new AuditRecord 
                { 
                    ID = "EPF-002", 
                    Date = new DateTime(2024, 01, 20), 
                    ClientName = "Bob Jones", 
                    Company = "Tech Solutions", 
                    NoOfStaffs = 4, 
                    Branch = "West",
                    StaffList = new()
                    {
                        new StaffMember { StaffId = "STF-0006", StaffName = "Sarah Jenkins", Phone = "0711234567", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0007", StaffName = "Tom Brown", Phone = "0712345678", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0008", StaffName = "Lisa Ray", Phone = "0713456789", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0009", StaffName = "Mark Hamill", Phone = "0714567890", Process = "COMPLETE" }
                    }
                },
                new AuditRecord 
                { 
                    ID = "EPF-003", 
                    Date = new DateTime(2024, 02, 10), 
                    ClientName = "Charlie Brown", 
                    Company = "Charlie's Design", 
                    NoOfStaffs = 3, 
                    Branch = "Central",
                    StaffList = new()
                    {
                        new StaffMember { StaffId = "STF-0010", StaffName = "Linus Van Pelt", Phone = "0761234567", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0011", StaffName = "Sally Brown", Phone = "0762345678", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0012", StaffName = "Lucy Van Pelt", Phone = "0763456789", Process = "SUBMIT" }
                    }
                },
                new AuditRecord 
                { 
                    ID = "EPF-004", 
                    Date = new DateTime(2024, 03, 05), 
                    ClientName = "David Wilson", 
                    Company = "Wilson Logistics", 
                    NoOfStaffs = 4, 
                    Branch = "Northeast",
                    StaffList = new()
                    {
                        new StaffMember { StaffId = "STF-0013", StaffName = "Kevin Hart", Phone = "0701234567", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0014", StaffName = "Dwayne Johnson", Phone = "0702345678", Process = "SUBMIT" },
                        new StaffMember { StaffId = "STF-0015", StaffName = "Jack Black", Phone = "0703456789", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0016", StaffName = "Karen Gillan", Phone = "0704567890", Process = "SUBMIT" }
                    }
                },
                new AuditRecord 
                { 
                    ID = "EPF-005", 
                    Date = new DateTime(2024, 03, 12), 
                    ClientName = "Eva Martinez", 
                    Company = "Eva's Event Planning", 
                    NoOfStaffs = 2, 
                    Branch = "South",
                    StaffList = new()
                    {
                        new StaffMember { StaffId = "STF-0017", StaffName = "Tony Montana", Phone = "0751234567", Process = "COMPLETE" },
                        new StaffMember { StaffId = "STF-0018", StaffName = "Elvira Hancock", Phone = "0752345678", Process = "SUBMIT" }
                    }
                },
            },
            ["Company Registration"] = new()
            {
                new AuditRecord 
                { 
                    ID = "CR-001", 
                    Date = new DateTime(2024, 01, 15), 
                    ClientName = "Alice Smith", 
                    Company = "Alice's Bakery", 
                    PaymentStatus = "Paid", 
                    Process = "COMPLETED", 
                    Branch = "South",
                    Address = "No. 45, Flower Road, Colombo 07",
                    Email = "alice.bakery@gmail.com",
                    PhoneNo = "077 123 4567",
                    Assignment = "Private Limited",
                    DirectorsList = new() { new CompanyCharacter { Name = "Alice Smith", Role = "Managing Director" }, new CompanyCharacter { Name = "John Smith", Role = "Director" } },
                    RegistrationDocuments = new() 
                    { 
                        new AppDocument { FileName = "Alice_NIC.pdf", FileSize = "1.2 MB", Category = "NIC", Type = "NIC Front/Back" },
                        new AppDocument { FileName = "Incorp_Cert.pdf", FileSize = "2.4 MB", Category = "PROCESS", Type = "Certificate", Description = "Incorporation Certificate" }
                    }
                },
                new AuditRecord 
                { 
                    ID = "CR-002", 
                    Date = new DateTime(2024, 01, 20), 
                    ClientName = "Bob Jones", 
                    Company = "Tech Solutions", 
                    PaymentStatus = "Unpaid", 
                    Process = "PENDING", 
                    Branch = "West",
                    Address = "123 Innovation Drive, Kandy",
                    Email = "contact@techsolutions.lk",
                    PhoneNo = "081 222 3333",
                    Assignment = "Private Limited",
                    DirectorsList = new() { new CompanyCharacter { Name = "Bob Jones", Role = "Director" } }
                },
                new AuditRecord { ID = "CR-003", Date = new DateTime(2024, 02, 10), ClientName = "Charlie Brown", Company = "Charlie's Design", PaymentStatus = "Partial", Process = "IN PROGRESS", Branch = "Central", Address = "88 Art Lane, Galle", Email = "charlie@designs.com", PhoneNo = "091 444 5555", Assignment = "Partnership" },
                new AuditRecord 
                { 
                    ID = "CR-004", 
                    Date = new DateTime(2024, 03, 05), 
                    ClientName = "David Wilson", 
                    Company = "Wilson Logistics", 
                    PaymentStatus = "Paid", 
                    Process = "REVIEW", 
                    Branch = "Northeast",
                    Address = "No 12, Logistics Park, Biyagama",
                    Email = "info@wilsonlogistics.lk",
                    PhoneNo = "011 555 6666",
                    Assignment = "Private Limited",
                    DirectorsList = new() { new CompanyCharacter { Name = "David Wilson", Role = "Managing Director" }, new CompanyCharacter { Name = "Sarah Wilson", Role = "Director" } }
                },
                new AuditRecord 
                { 
                    ID = "CR-005", 
                    Date = new DateTime(2024, 03, 12), 
                    ClientName = "Eva Martinez", 
                    Company = "Eva's Event Planning", 
                    PaymentStatus = "Unpaid", 
                    Process = "DRAFT", 
                    Branch = "South",
                    Address = "55/2 Celebration Way, Matara",
                    Email = "eva@events.lk",
                    PhoneNo = "041 888 9999",
                    Assignment = "Sole Proprietorship",
                    DirectorsList = new() { new CompanyCharacter { Name = "Eva Martinez", Role = "Owner" } }
                }
            },
            ["Form - 15"] = new()
            {
                new AuditRecord { ID = "F15-001", Date = new DateTime(2024, 05, 10), ClientName = "Alpha Corp", Company = "Alpha Corp Ltd", Branch = "West", Process = "FORM - 15", LoginId = "alpha123", Password = "pwd123", PhoneNo = "011 222 3333" },
                new AuditRecord { ID = "F15-002", Date = new DateTime(2024, 06, 12), ClientName = "Beta LLC", Company = "Beta Systems LLC", Branch = "North", Process = "PAYMENT", LoginId = "beta456", Password = "pwd456", PhoneNo = "011 555 7777" },
                new AuditRecord { ID = "F15-003", Date = new DateTime(2024, 07, 05), ClientName = "Gamma Inc", Company = "Gamma Tech Inc", Branch = "East", Process = "CERTIFIED COPY", LoginId = "gamma789", Password = "pwd789", PhoneNo = "011 333 4444" },
                new AuditRecord { ID = "F15-004", Date = new DateTime(2024, 01, 20), ClientName = "Delta Holdings", Company = "Delta Holdings PVT", Branch = "South", Process = "FORM - 15", LoginId = "delta321", Password = "pwd321", PhoneNo = "011 999 8888" },
                new AuditRecord { ID = "F15-005", Date = new DateTime(2024, 03, 15), ClientName = "Omega Solutions", Company = "Omega IT Solutions", Branch = "West", Process = "PAYMENT", LoginId = "omega654", Password = "pwd654", PhoneNo = "011 777 6666" },
                new AuditRecord { ID = "F15-006", Date = new DateTime(2024, 04, 25), ClientName = "Zeta Retail", Company = "Zeta Retail Chain", Branch = "North", Process = "CERTIFIED COPY", LoginId = "zeta987", Password = "pwd987", PhoneNo = "011 111 2222" },
                new AuditRecord { ID = "F15-007", Date = new DateTime(2024, 02, 28), ClientName = "Epsilon Group", Company = "Epsilon Group of Companies", Branch = "East", Process = "FORM - 15", LoginId = "epsilon111", Password = "pwd111", PhoneNo = "011 444 5555" },
                new AuditRecord { ID = "F15-008", Date = new DateTime(2024, 05, 05), ClientName = "Sigma Logistics", Company = "Sigma Logistics PLC", Branch = "South", Process = "PAYMENT", LoginId = "sigma222", Password = "pwd222", PhoneNo = "011 666 7777" },
                new AuditRecord { ID = "F15-009", Date = new DateTime(2024, 06, 18), ClientName = "Kappa Foods", Company = "Kappa Foods & Beverages", Branch = "West", Process = "CERTIFIED COPY", LoginId = "kappa333", Password = "pwd333", PhoneNo = "011 888 9999" },
                new AuditRecord { ID = "F15-010", Date = new DateTime(2024, 07, 02), ClientName = "Theta Capital", Company = "Theta Capital Investments", Branch = "North", Process = "FORM - 15", LoginId = "theta444", Password = "pwd444", PhoneNo = "011 000 1111" }
            },
            ["Trade License"] = new()
            {
                new AuditRecord 
                { 
                    ID = "TL-001", 
                    Date = new DateTime(2024, 01, 22), 
                    ClientName = "Grace's Salon", 
                    Company = "Grace's Salon", 
                    Branch = "West", 
                    PaymentStatus = "Unpaid", 
                    Assignment = "BEAUTY CARE", 
                    Process = "BOOKKEEP", 
                    Address = "22 Beauty Lane, Negombo",
                    Email = "grace@salon.lk",
                    PhoneNo = "031 999 0000",
                    SourceDocuments = new() { new SourceDocument { FileName = "NIC_Grace.pdf", Description = "National Identity Card" }, new SourceDocument { FileName = "Premise_Lease.pdf", Description = "Lease Agreement" } } 
                },
                new AuditRecord 
                { 
                    ID = "TL-002", 
                    Date = new DateTime(2024, 01, 15), 
                    ClientName = "Modern Bakers", 
                    Company = "Modern Bakers", 
                    Branch = "South", 
                    PaymentStatus = "Paid", 
                    Assignment = "FOOD & BEVERAGE", 
                    Process = "SUBMIT",
                    Address = "88 Bakery St, Galle",
                    Email = "admin@modernbakers.lk",
                    PhoneNo = "091 222 1111"
                },
                new AuditRecord 
                { 
                    ID = "TL-003", 
                    Date = new DateTime(2024, 02, 10), 
                    ClientName = "Swift Couriers", 
                    Company = "Swift Couriers", 
                    Branch = "Central", 
                    PaymentStatus = "Partial", 
                    Assignment = "LOGISTICS", 
                    Process = "FINALIZE",
                    Address = "100 Delivery Hub, Colombo 03",
                    Email = "ops@swiftcouriers.lk",
                    PhoneNo = "011 777 8888"
                },
            },
            ["Trade Mark"] = new()
            {
                new AuditRecord 
                { 
                    ID = "TM-001", 
                    Date = new DateTime(2024, 01, 10), 
                    ClientName = "Eve White", 
                    Company = "Eve's Fashion", 
                    Branch = "South", 
                    PaymentStatus = "Paid", 
                    Assignment = "Trademark registration for Eve's Fashion. Client Name: Eve White. Code: 867958. Standard secretarial procedures for trademark filing and documentation tracking.", 
                    Address = "No 11, Fashion Ave, Colombo 03", 
                    Email = "eve@fashion.lk",
                    Process = "BOOKKEEP",
                    CurrentStep = 1,
                    SourceDocuments = new() 
                    { 
                        new SourceDocument { FileName = "NIC.pdf", Description = "National Identity Card" },
                        new SourceDocument { FileName = "BR.pdf", Description = "Business Registration" },
                        new SourceDocument { FileName = "R1.pdf", Description = "Trademark Application Form" },
                        new SourceDocument { FileName = "ART.pdf", Description = "Articles of Association" },
                        new SourceDocument { FileName = "LOGO-TM.pdf", Description = "Trademark Logo Design" },
                        new SourceDocument { FileName = "NO.pdf", Description = "Notice of Application" },
                        new SourceDocument { FileName = "CATEGORY.pdf", Description = "Class Specification" }
                    }
                },
                new AuditRecord { ID = "TM-002", Date = new DateTime(2024, 02, 05), ClientName = "John Wick", Company = "Continental Hotel", Branch = "Central", PaymentStatus = "Unpaid", Assignment = "SLOGAN PROTECTION", Address = "New York / Colombo", Email = "manager@continental.lk" }
            },
            ["Import / Export"] = new()
            {
                new AuditRecord { ID = "IE-001", Date = new DateTime(2024, 03, 01), ClientName = "Jack Sparrow", Company = "Black Pearl Shipping", Branch = "West", PaymentStatus = "Partial", Assignment = "LICENSE RENEWAL", Address = "Port of Colombo, Pier 01", Email = "jack@pearl.com" },
                new AuditRecord 
                { 
                    ID = "001", 
                    Date = new DateTime(2024, 01, 25), 
                    ClientName = "Robert Downey Jr", 
                    Company = "Stark Logistics", 
                    Branch = "South", 
                    PaymentStatus = "Regular", 
                    Assignment = "IMPORT", 
                    Address = "Stark Tower, Colombo", 
                    Email = "shipping@stark.lk",
                    TIN = "TIN-12345678",
                    Process = "DOCUMENTATION",
                    CurrentStep = 1,
                    SourceDocuments = new()
                    {
                        new SourceDocument { FileName = "NIC.pdf", Description = "National Identity Card" }
                    }
                }
            },
            ["HR and Management Consulting"] = new()
            {
                new AuditRecord 
                { 
                    ID = "001", 
                    Date = new DateTime(2024, 01, 10), 
                    ClientName = "John Smith", 
                    Company = "Tech Corp", 
                    Branch = "Main", 
                    PaymentStatus = "PENDING", 
                    Assignment = "HR Strategy", 
                    Process = "PENDING",
                    Code = "-",
                    CurrentStep = 1,
                    Address = "Tech Park, Colombo 02", 
                    Email = "contact@techcorp.lk" 
                },
                new AuditRecord { ID = "HR-002", Date = new DateTime(2024, 03, 20), ClientName = "Gordon Ramsay", Company = "Hell's Kitchen", Branch = "South", PaymentStatus = "Unpaid", Assignment = "STAFF TRAINING", Address = "Culinary Academy, Galle", Email = "chef@hellskitchen.lk" }
            }
        };

        public static Dictionary<string, List<TaxRecord>> TaxRecords { get; } = new()
        {
            ["Corporate Income Tax (CIT)"] = new()
            {
                new() { ID = "CIT-2024-001", ClientName = "Acme Corp", ClientNameSub = "CL-001", DINNo = "DIN-8821", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) },
                new() { ID = "CIT-2024-002", ClientName = "Globex Inc", ClientNameSub = "CL-002", DINNo = "DIN-9932", TaxPeriod = "2024 Q1", Status = "Pending", Branch = "West", Date = new DateTime(2026, 1, 13) },
                new() { ID = "CIT-2024-003", ClientName = "Soylent Corp", ClientNameSub = "CL-003", DINNo = "DIN-1123", TaxPeriod = "2023 Annual", Status = "IRD pending", Branch = "Central", Date = new DateTime(2025, 12, 14) },
                new() { ID = "CIT-2024-004", ClientName = "Initech", ClientNameSub = "CL-004", DINNo = "DIN-4451", TaxPeriod = "2024-02", Status = "Paid", Branch = "Northeast", Date = new DateTime(2026, 1, 28) }
            },
            ["Individual Income Tax (IIT)"] = new()
            {
                new() { ID = "IIT-2024-001", ClientName = "Jane Smith", ClientNameSub = "CL-101", DINNo = "TIN-8821", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) },
                new() { ID = "IIT-2024-002", ClientName = "Robert Johnson", ClientNameSub = "CL-102", DINNo = "TIN-9932", TaxPeriod = "2024 Q1", Status = "Pending", Branch = "West", Date = new DateTime(2026, 1, 13) },
                new() { ID = "IIT-2024-003", ClientName = "Emily Davis", ClientNameSub = "CL-103", DINNo = "TIN-1123", TaxPeriod = "2023 Annual", Status = "IRD Paid", Branch = "Central", Date = new DateTime(2025, 12, 14) },
            },
            ["Social Security Contribution Levy (SSCL)"] = new()
            {
                new() { ID = "SSCL-2026-335", ClientName = "Doggyboy shanuka", ClientNameSub = "CL-403", DINNo = "SSCL-1234", TaxPeriod = "3 Days", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 28) },
                new() { ID = "SSCL-2024-001", ClientName = "Omega Industries", ClientNameSub = "CL-301", DINNo = "SSCL-555444", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) },
                new() { ID = "SSCL-2024-002", ClientName = "Delta Corp", ClientNameSub = "CL-302", DINNo = "SSCL-333222", TaxPeriod = "2024 Q1", Status = "Pending", Branch = "West", Date = new DateTime(2026, 1, 20) },
                new() { ID = "SSCL-2024-003", ClientName = "Theta Solutions", ClientNameSub = "CL-303", DINNo = "SSCL-111000", TaxPeriod = "2023 Annual", Status = "IRD Paid", Branch = "Central", Date = new DateTime(2026, 1, 3) },
            },
            ["Value Added Tax (VAT)"] = new()
            {
                new() { ID = "VAT-2024-001", ClientName = "ABC Trading", ClientNameSub = "CL-201", DINNo = "VAT-998877", TaxPeriod = "2024-01", Status = "Paid", Branch = "Central", Date = new DateTime(2026, 1, 26) },
                new() { ID = "VAT-2024-002", ClientName = "XYZ Services", ClientNameSub = "CL-202", DINNo = "VAT 445566", TaxPeriod = "2024 Q1", Status = "Pending", Branch = "Northeast", Date = new DateTime(2026, 1, 18) },
                new() { ID = "VAT-2024-003", ClientName = "Global Imports", ClientNameSub = "CL-203", DINNo = "VAT-112233", TaxPeriod = "2023 Q4", Status = "IRD Paid", Branch = "South", Date = new DateTime(2025, 12, 19) },
            },
            ["Withholding Tax (WHT)"] = new()
            {
                new() { ID = "WHT-2026-653", ClientName = "fasfdaf", ClientNameSub = "dsfa12313", DINNo = "342dsga", TaxPeriod = "2 Months", Status = "IRD Paid", Branch = "South", Date = new DateTime(2026, 1, 28) },
                new() { ID = "WHT-2024-001", ClientName = "Gamma Services", ClientNameSub = "CL-401", DINNo = "WHT 777888", TaxPeriod = "2024-01", Status = "Paid", Branch = "South", Date = new DateTime(2026, 1, 26) },
                new() { ID = "WHT-2024-002", ClientName = "Lambda Corp", ClientNameSub = "CL-402", DINNo = "WHT-555666", TaxPeriod = "2024 Q1", Status = "Pending", Branch = "West", Date = new DateTime(2026, 1, 20) },
                new() { ID = "WHT-2024-003", ClientName = "Sigma Inc", ClientNameSub = "CL-403", DINNo = "WHT-333444", TaxPeriod = "2023 Annual", Status = "IRD Paid", Branch = "South", Date = new DateTime(2025, 12, 29) },
            }
        };

        public static List<OutstandingBalanceRecord> OutstandingBalances { get; } = new()
        {
            new OutstandingBalanceRecord
            {
                ClientId = "CLT-001",
                ClientName = "Titan Industries",
                ServiceModule = "Forensic Audit",
                InvoiceNumber = "INV-2026-004",
                TotalAmount = 150000m,
                AmountPaid = 90800m,
                OutstandingAmount = 59200m,
                PaymentType = "Cheque",
                ChequeNumber = "CHQ-882901",
                DueDate = new DateTime(2026, 4, 15),
                PaymentStatus = "Partial",
                DaysOverdue = 41,
                LastPaymentDate = new DateTime(2026, 4, 20),
                Notes = "Partial payment made via cheque. Awaiting clearance of the remaining balance.",
                PaymentHistory = new()
                {
                    new PaymentHistoryEntry { Date = new DateTime(2026, 4, 20), Description = "Partial payment received", Amount = 90800m, PaymentMethod = "Cheque", Reference = "CHQ-882901" }
                },
                ChequeDetails = new()
                {
                    new ChequeDetail { ChequeNumber = "CHQ-882901", Bank = "Bank of Ceylon (BOC)", Amount = 90800m, ChequeDate = new DateTime(2026, 4, 20), Status = "Realized" }
                }
            },
            new OutstandingBalanceRecord
            {
                ClientId = "CLT-004",
                ClientName = "Solar Systems",
                ServiceModule = "SSCL (Tax Filing)",
                InvoiceNumber = "INV-2026-012",
                TotalAmount = 500000m,
                AmountPaid = 0m,
                OutstandingAmount = 500000m,
                PaymentType = "Cheque",
                ChequeNumber = "CHQ-551023",
                DueDate = new DateTime(2026, 5, 10),
                PaymentStatus = "Pending Cheque",
                DaysOverdue = 16,
                LastPaymentDate = null,
                Notes = "Post-dated cheque received and currently pending deposit on due instruction.",
                PaymentHistory = new(),
                ChequeDetails = new()
                {
                    new ChequeDetail { ChequeNumber = "CHQ-551023", Bank = "Commercial Bank", Amount = 500000m, ChequeDate = new DateTime(2026, 5, 10), Status = "Pending" }
                }
            },
            new OutstandingBalanceRecord
            {
                ClientId = "CLT-009",
                ClientName = "Bridge Partners",
                ServiceModule = "Company Registration",
                InvoiceNumber = "INV-2026-025",
                TotalAmount = 800000m,
                AmountPaid = 300000m,
                OutstandingAmount = 500000m,
                PaymentType = "Bank Transfer",
                ChequeNumber = "CHQ-100234",
                DueDate = new DateTime(2026, 3, 30),
                PaymentStatus = "Bounced Cheque",
                DaysOverdue = 57,
                LastPaymentDate = new DateTime(2026, 4, 05),
                Notes = "The initial cheque payment bounced due to insufficient funds. The client promised a direct bank transfer but it remains outstanding.",
                PaymentHistory = new()
                {
                    new PaymentHistoryEntry { Date = new DateTime(2026, 4, 05), Description = "Advanced registration deposit", Amount = 300000m, PaymentMethod = "Cash", Reference = "CSH-9921" }
                },
                ChequeDetails = new()
                {
                    new ChequeDetail { ChequeNumber = "CHQ-100234", Bank = "HNB Bank", Amount = 500000m, ChequeDate = new DateTime(2026, 4, 10), Status = "Bounced" }
                }
            },
            new OutstandingBalanceRecord
            {
                ClientId = "CLT-011",
                ClientName = "Nexus Global",
                ServiceModule = "Audit & Assurance",
                InvoiceNumber = "INV-2026-089",
                TotalAmount = 3500000m,
                AmountPaid = 1000000m,
                OutstandingAmount = 2500000m,
                PaymentType = "Bank Transfer",
                DueDate = new DateTime(2026, 5, 01),
                PaymentStatus = "Partial",
                DaysOverdue = 25,
                LastPaymentDate = new DateTime(2026, 5, 05),
                Notes = "Major audit milestone completed. First partial payment received. Remaining milestone billing outstanding.",
                PaymentHistory = new()
                {
                    new PaymentHistoryEntry { Date = new DateTime(2026, 5, 05), Description = "First milestone payment", Amount = 1000000m, PaymentMethod = "Bank Transfer", Reference = "TXN-88129034" }
                },
                ChequeDetails = new()
            }
        };

        public static void SyncOutstandingBalanceRecord(string category, AuditRecord record)
        {
            OutstandingBalances.RemoveAll(r => r.InvoiceNumber == record.Code || r.InvoiceNumber == record.ID);
            if (record.PaymentStatus == "Partial" || record.PaymentStatus == "Unpaid" || 
                record.PaymentStatus == "Pending Cheque" || record.PaymentStatus == "Bounced Cheque")
            {
                var ob = new OutstandingBalanceRecord
                {
                    ClientId = record.ClientId?.ToString() ?? record.ClientCode,
                    ClientName = record.ClientName,
                    ServiceModule = category,
                    InvoiceNumber = record.Code ?? record.ID,
                    TotalAmount = record.TotalPayment,
                    AmountPaid = record.PartialAmount,
                    OutstandingAmount = record.TotalPayment - record.PartialAmount,
                    PaymentType = record.PaymentOption,
                    DueDate = record.Date.AddDays(30),
                    PaymentStatus = record.PaymentStatus,
                    ChequeNumber = record.ChequeNumber
                };

                if (record.PaymentOption == "Cheque")
                {
                    ob.ChequeDetails = new List<ChequeDetail>
                    {
                        new ChequeDetail
                        {
                            ChequeNumber = record.ChequeNumber,
                            Bank = record.ChequeBank,
                            Amount = record.ChequeAmount ?? record.TotalPayment,
                            ChequeDate = record.ChequeDate ?? record.Date,
                            Status = record.ChequeStatus ?? "Pending"
                        }
                    };
                }

                OutstandingBalances.Add(ob);
            }
        }
    }
}
