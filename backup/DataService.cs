using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.Data;

namespace AATS.Desktop.Services
{
    public class DataService : IDataService
    {
        private static DataService? _instance;
        private static readonly object _lock = new object();
        private List<ClientRecord>? _cachedClients;
        public static DataService Instance => _instance ??= new DataService();



        private DataService() { }

        public async Task<List<NexoraRequest>> GetNexoraRequestsAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<NexoraRequest>>>("/api/v1/Nexora/requests");
                return response?.Data?.Items ?? new List<NexoraRequest>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Nexora requests: {ex.Message}");
                return new List<NexoraRequest>();
            }
        }

        public async Task<NexoraRequest?> AddNexoraRequestAsync(NexoraRequest request)
        {
            var response = await ApiService.Instance.PostAsync<NexoraRequest, ApiResponse<NexoraRequest>>("/api/v1/Nexora/requests", request);
            return response?.Data;
        }

        public async Task UpdateNexoraRequestAsync(NexoraRequest request)
        {
            await ApiService.Instance.PutAsync($"/api/v1/Nexora/requests/{request.Id}", request);
        }

        public async Task DeleteNexoraRequestsAsync(IEnumerable<NexoraRequest> requests)
        {
            foreach (var r in requests)
            {
                await ApiService.Instance.DeleteAsync($"/api/v1/Nexora/requests/{r.Id}");
            }
        }



        public async Task<List<TeamMember>> GetTeamMembersAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<TeamMember>>>("/api/v1/users");
                var items = response?.Data?.Items ?? new List<TeamMember>();
                foreach (var item in items)
                {
                    item.Branch = NormalizeBranchName(item.Branch);
                }
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Team Members: {ex.Message}");
                return new List<TeamMember>();
            }
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            var req = new 
            { 
                Username = member.Username ?? "", 
                Email = member.Email ?? "", 
                Password = member.Password ?? "Pass@123",
                BranchId = member.BranchId,
                Phone = member.Phone ?? "",
                Role = member.Role == "Admin" ? 1 : (member.Role == "Manager" ? 3 : 2)
            };
            await ApiService.Instance.PostAsync("/api/v1/auth/register", req);
        }

        public async Task UpdateTeamMemberAsync(TeamMember member)
        {
            await ApiService.Instance.PutAsync($"/api/v1/users/{member.Id}", member);
        }

        public async Task DeleteTeamMembersAsync(IEnumerable<TeamMember> members)
        {
            foreach (var m in members)
            {
                await ApiService.Instance.DeleteAsync($"/api/v1/users/{m.Id}");
            }
        }

        private static string NormalizeBranchName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";

            var trimmed = name.Trim();
            if (trimmed.Equals("Central Branch", StringComparison.OrdinalIgnoreCase) || 
                trimmed.Equals("Central", StringComparison.OrdinalIgnoreCase)) 
                return "Central";

            if (trimmed.Equals("Southern Branch", StringComparison.OrdinalIgnoreCase) || 
                trimmed.Equals("Southern", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("South", StringComparison.OrdinalIgnoreCase)) 
                return "South";

            if (trimmed.Equals("Western Branch", StringComparison.OrdinalIgnoreCase) || 
                trimmed.Equals("Western", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("West", StringComparison.OrdinalIgnoreCase)) 
                return "West";

            if (trimmed.Equals("Northeast Branch", StringComparison.OrdinalIgnoreCase) || 
                trimmed.Equals("Northeast", StringComparison.OrdinalIgnoreCase)) 
                return "Northeast";

            return trimmed;
        }

        public async Task<List<Branch>> GetBranchesAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<Branch>>>("/api/v1/branches");
                var rawBranches = response?.Data?.Items ?? new List<Branch>();

                var uniqueBranches = new List<Branch>();
                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var b in rawBranches)
                {
                    b.Name = NormalizeBranchName(b.Name);
                    if (!seenNames.Contains(b.Name))
                    {
                        seenNames.Add(b.Name);
                        uniqueBranches.Add(b);
                    }
                }

                return uniqueBranches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Branches: {ex.Message}");
                return new List<Branch>();
            }
        }

        public async Task<List<ClientRecord>> GetClientsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cachedClients != null)
            {
                return _cachedClients;
            }

            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ClientRecord>>>("/api/v1/clients");
                var items = response?.Data?.Items ?? new List<ClientRecord>();
                foreach (var item in items)
                {
                    item.Branch = NormalizeBranchName(item.Branch);
                }
                _cachedClients = items.OrderBy(c => c.ClientCode).ToList();
                return _cachedClients;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Clients: {ex.Message}");
                return new List<ClientRecord>();
            }
        }

        public async Task<ClientRecord?> GetClientByIdAsync(string id)
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<ClientRecord>>($"/api/v1/clients/{id}");
                return response?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching client {id}: {ex.Message}");
                return null;
            }
        }

        public async Task AddClientAsync(ClientRecord client)
        {
            await ApiService.Instance.PostAsync("/api/v1/clients", client);
            _cachedClients = null; // Invalidate cache
        }

        public async Task UpdateClientAsync(ClientRecord client)
        {
            Console.WriteLine($"[DEBUG] Sending PUT request for client {client.Id} (Status: {client.Status})");
            await ApiService.Instance.PutAsync($"/api/v1/clients/{client.Id}", client);
            _cachedClients = null; // Invalidate cache
        }

        public async Task DeleteClientsAsync(IEnumerable<ClientRecord> clients)
        {
            foreach (var c in clients)
            {
                await ApiService.Instance.DeleteAsync($"/api/v1/clients/{c.Id}");
            }
            _cachedClients = null; // Invalidate cache
        }

        private class ApiActivityUser
        {
            public string Username { get; set; } = string.Empty;
        }

        private class ApiActivityBranch
        {
            public string Name { get; set; } = string.Empty;
        }

        private class ApiActivityLog
        {
            public long Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public ApiActivityUser? User { get; set; }
            public ApiActivityBranch? Branch { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Module { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        public async Task<List<ActivityLogEntry>> GetActivityLogsAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ApiActivityLog>>>("/api/v1/activity-logs");
                var items = response?.Data?.Items ?? new List<ApiActivityLog>();
                
                return items.Select(log => new ActivityLogEntry
                {
                    Id = $"LOG-{log.Id:D5}",
                    Timestamp = log.CreatedAt.ToLocalTime(),
                    User = log.User?.Username ?? "System",
                    Action = log.Action,
                    Module = log.Module,
                    Branch = NormalizeBranchName(log.Branch?.Name),
                    Details = log.Description ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Activity Logs from API: {ex.Message}");
                // Fallback to mock data on error so it doesn't break if server is down
                return MockData.ActivityLogs;
            }
        }

        private string MapAuditCategoryToEndpoint(string category)
        {
            return category switch
            {
                "Audit & Assurance" => "assurance",
                "Internal Audit" => "internal",
                "Forensic Audit & Investigation" => "forensic",
                "Forensic Audit" => "forensic",
                "Internal Control Systems & Outsourcing" => "internal-control",
                "Internal Control" => "internal-control",
                "Management Accountings" => "management-accounts",
                "Management Accounting" => "management-accounts",
                "Audit Others" => "others",
                "Others" => "others",
                "Tax Others" => "others",
                "Tax Account" => "/api/v1/Tax/records",
                "Tax Accountings" => "/api/v1/Tax/records",
                "Tax" => "/api/v1/Tax/records",
                _ => category.ToLower().Replace(" ", "-").Replace("&", "and")
            };
        }

        private bool IsTaxFilingCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return false;
            var taxFilingCategories = new[]
            {
                "Value Added Tax (VAT)", "Corporate Income Tax (CIT)", "Individual Income Tax (IIT)",
                "Social Security Contribution Levy (SSCL)", "Withholding Tax (WHT)", "Tax Others",
                "Tax Account", "Tax Accountings"
            };
            return taxFilingCategories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        private string MapTaxCategoryToEndpoint(string category)
        {
            return category switch
            {
                "Value Added Tax (VAT)" => "vat",
                "Corporate Income Tax (CIT)" => "cit",
                "Individual Income Tax (IIT)" => "iit",
                "Social Security Contribution Levy (SSCL)" => "sscl",
                "Withholding Tax (WHT)" => "wht",
                "Tax Others" => "filings",
                "Tax Accountings" => "records",
                "Tax Account" => "records",
                _ => "filings"
            };
        }

        private bool IsSecretarialCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return false;
            
            var normalizedInput = category.Replace(" ", "").Replace("&", "AND").ToUpper();
            var secretarialCategories = new[]
            {
                "COMPANYREGISTRATION", "EPF/ETF", "TRADEMARK", "TRADEMARKS", "TRADELICENSE", "TRADELICENSES", "FORM-15", "FORM-15",
                "IMPORT/EXPORT", "IMPORTANDEXPORTCLEARANCE", 
                "HRANDMANAGEMENTCONSULTING", "HRCONSULTING", "BUSINESSPLANANDASSETVALUATION", 
                "BUSINESSPLANANDASSETVALUATIONCONSULTING", "BUSINESSPLANS", "BOIREGISTRATION", "BOI", "BOIREGISTRATIONS", "SECRETARIALOTHERS", "PAYROLL"
            };
            return secretarialCategories.Contains(normalizedInput);
        }

        private string MapSecretarialCategoryToEndpoint(string category)
        {
            if (string.IsNullOrEmpty(category)) return "others";
            
            var normalized = category.Replace(" ", "").Replace("&", "AND").ToUpper();
            return normalized switch
            {
                "COMPANYREGISTRATION" => "company-registrations",
                "EPF/ETF" => "epf-etf",
                "TRADEMARK" or "TRADEMARKS" => "trade-marks",
                "TRADELICENSE" or "TRADELICENSES" => "trade-licenses",
                "FORM-15" or "FORM-15" => "form-15",
                "PAYROLL" => "payroll",
                "IMPORT/EXPORT" => "import-export",
                "IMPORTANDEXPORTCLEARANCE" => "import-export",
                "HRANDMANAGEMENTCONSULTING" or "HRCONSULTING" => "hr-consulting",
                "BUSINESSPLANANDASSETVALUATION" or "BUSINESSPLANANDASSETVALUATIONCONSULTING" or "BUSINESSPLANS" => "business-plans",
                "BOIREGISTRATION" or "BOI" or "BOIREGISTRATIONS" => "boi-registrations",
                "SECRETARIALOTHERS" => "others",
                _ => category.ToLower().Replace(" ", "-").Replace("&", "and")
            };
        }

        public async Task<List<AuditRecord>> GetAuditRecordsAsync(string category, bool enrich = true)
        {
            try
            {
                string baseUrl = IsSecretarialCategory(category) ? "/api/v1/Secretarial/" : "/api/v1/Audit/";
                string endpoint = IsSecretarialCategory(category) ? MapSecretarialCategoryToEndpoint(category) : MapAuditCategoryToEndpoint(category);
                string url = endpoint.StartsWith("/") ? endpoint : baseUrl + endpoint;
                
                url += $"?enrich={enrich.ToString().ToLower()}";

                Console.WriteLine($"[DEBUG] Fetching Records for {category}. URL: {url}");
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<AuditRecord>>>(url);
                var items = response?.Data?.Items ?? new List<AuditRecord>();
                foreach (var item in items)
                {
                    item.Branch = NormalizeBranchName(item.Branch);
                }
                Console.WriteLine($"[DEBUG] Received {items.Count} items for {category}");

                // Fallback to mock data when API returns no results
                if (items.Count == 0 && MockData.AuditRecords.TryGetValue(category, out var mockItems))
                {
                    Console.WriteLine($"[DEBUG] API returned 0 items for {category}. Falling back to MockData ({mockItems.Count} records).");
                    return mockItems;
                }

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Records ({category}): {ex.Message}");
                // Fallback to mock data on error so the UI is not empty when server is down
                if (MockData.AuditRecords.TryGetValue(category, out var mockItems))
                {
                    Console.WriteLine($"[DEBUG] Falling back to MockData for {category} ({mockItems.Count} records).");
                    return mockItems;
                }
                return new List<AuditRecord>();
            }
        }

        public async Task<AuditRecord?> GetRecordByIdAsync(string category, string id)
        {
            try
            {
                string baseUrl = IsSecretarialCategory(category) ? "/api/v1/Secretarial/" : "/api/v1/Audit/";
                string endpoint = IsSecretarialCategory(category) ? MapSecretarialCategoryToEndpoint(category) : MapAuditCategoryToEndpoint(category);
                string url = endpoint.StartsWith("/") ? endpoint : baseUrl + endpoint;

                var response = await ApiService.Instance.GetAsync<ApiResponse<AuditRecord>>(url + "/" + id);
                return response?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Record ({category}) ID {id}: {ex.Message}");
                return null;
            }
        }
        
        public async Task AddAuditRecordAsync(string category, AuditRecord record)
        {
            try
            {
                if (IsSecretarialCategory(category))
                {
                    string endpoint = MapSecretarialCategoryToEndpoint(category);
                    if (category == "Company Registration")
                    {
                        var req = new
                        {
                            CompanyName = record.Company,
                            CompanyType = record.Type,
                            ClientName = record.ClientName,
                            ClientId = record.ClientId,
                            ClientCode = record.ClientCode,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            Date = record.Date,
                            Email = record.Email,
                            Phone = record.PhoneNo,
                            Objective = record.Assignment,
                            Address = record.Address,
                            Description = record.Description,
                            Status = record.Status ?? "ACTIVE",
                            PaymentStatus = record.PaymentStatus ?? "Pending",
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            DirectorNames = string.Join(", ", record.DirectorsList?.Select(d => d.Name) ?? Enumerable.Empty<string>()),
                            SecretaryNames = string.Join(", ", record.SecretariesList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                            ShareholderNames = string.Join(", ", record.ShareholdersList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                            OtherNames = string.Join(", ", record.OthersList?.Select(o => o.Detail) ?? Enumerable.Empty<string>()),
                            Officers = record.DirectorsList?.Select(d => new { Name = d.Name, Position = "Director", NicNumber = d.TIN }).ToList()
                                .Concat(record.SecretariesList?.Select(s => new { Name = s.Name, Position = "Secretary", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                                .Concat(record.ShareholdersList?.Select(s => new { Name = s.Name, Position = "Shareholder", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                                .Concat(record.OthersList?.Select(o => new { Name = o.Detail, Position = "Other", NicNumber = "" }) ?? Enumerable.Empty<object>())
                                .ToList(),
                            SourceDocuments = record.SourceDocuments
                        };
                        await ApiService.Instance.PostAsync($"/api/v1/Secretarial/{endpoint}", req);
                    }
                    else if (category == "EPF / ETF")
                    {
                        var req = new
                        {
                            ClientName = record.ClientName,
                            ClientId = record.ClientId,
                            ClientCode = record.ClientCode,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            CompanyName = record.Company,
                            Status = record.Status ?? "ACTIVE",
                            PaymentStatus = record.PaymentStatus ?? "Pending",
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            StaffMembers = record.StaffList?.Select(s => new { Id = s.Id, Name = s.StaffName, Phone = s.Phone, StaffCode = s.StaffId, ProcessStatus = s.Process, History = s.History }).ToList(),
                            SourceDocuments = record.SourceDocuments
                        };
                        await ApiService.Instance.PostAsync($"/api/v1/Secretarial/{endpoint}", req);
                    }
                    else
                    {
                        var req = new
                        {
                            ClientName = record.ClientName,
                            ClientId = record.ClientId,
                            ClientCode = record.ClientCode,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            CompanyName = record.Company,
                            Description = record.Description,
                            Assignment = record.Assignment,
                            Status = record.Status ?? "ACTIVE",
                            PaymentStatus = record.PaymentStatus ?? "Pending",
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            Country = record.Country,
                            CountryAddress = record.CountryAddress,
                            InvestmentValueUsd = record.InvestmentValue,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            SourceDocuments = record.SourceDocuments
                        };
                        await ApiService.Instance.PostAsync($"/api/v1/Secretarial/{endpoint}", req);
                    }
                }
                else
                {
                    string endpoint = MapAuditCategoryToEndpoint(category);
                    string url = endpoint.StartsWith("/") ? endpoint : $"/api/v1/Audit/{endpoint}";
                    await ApiService.Instance.PostAsync(url, record);
                }
                MockData.SyncOutstandingBalanceRecord(category, record);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding record ({category}): {ex.Message}");
            }
        }

        public async Task UpdateAuditRecordAsync(string category, AuditRecord record)
        {
            try
            {
                if (IsSecretarialCategory(category))
                {
                    string endpoint = MapSecretarialCategoryToEndpoint(category);
                    string id = record.ID ?? string.Empty;
                    
                    // Map to appropriate Update DTO
                    object req;
                    if (category == "Company Registration")
                    {
                        req = new
                        {
                            Id = record.ID,
                            RecordCode = record.Code,
                            CompanyName = record.Company,
                            CompanyType = record.Type,
                            ClientName = record.ClientName,
                            ClientId = record.ClientId,
                            ClientCode = record.ClientCode,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            Date = record.Date,
                            Email = record.Email,
                            Phone = record.PhoneNo,
                            Objective = record.Assignment,
                            Address = record.Address,
                            Description = record.Description,
                            Status = record.Status,
                            PaymentStatus = record.PaymentStatus,
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            NumberOfStaff = record.NoOfStaffs, // Match backend property if different
                            DirectorNames = string.Join(", ", record.DirectorsList?.Select(d => d.Name) ?? Enumerable.Empty<string>()),
                            SecretaryNames = string.Join(", ", record.SecretariesList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                            ShareholderNames = string.Join(", ", record.ShareholdersList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                            OtherNames = string.Join(", ", record.OthersList?.Select(o => o.Detail) ?? Enumerable.Empty<string>()),
                            Officers = record.DirectorsList?.Select(d => new { Name = d.Name, Position = "Director", NicNumber = d.TIN }).ToList()
                                .Concat(record.SecretariesList?.Select(s => new { Name = s.Name, Position = "Secretary", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                                .Concat(record.ShareholdersList?.Select(s => new { Name = s.Name, Position = "Shareholder", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                                .Concat(record.OthersList?.Select(o => new { Name = o.Detail, Position = "Other", NicNumber = "" }) ?? Enumerable.Empty<object>())
                                .ToList(),
                            SourceDocuments = record.SourceDocuments
                        };
                    }
                    else if (category == "EPF / ETF")
                    {
                        var staffList = record.StaffList?.Select(s => new { Id = s.Id, Name = s.StaffName, Phone = s.Phone, StaffCode = s.StaffId, ProcessStatus = s.Process, History = s.History }).ToList();
                        Console.WriteLine($"[DEBUG] Updating EPF/ETF. StaffCount: {record.NoOfStaffs}, StaffList: {staffList?.Count ?? 0}");
                        
                        req = new
                        {
                            Id = record.ID,
                            RecordCode = record.Code,
                            ClientName = record.ClientName,
                            ClientId = record.ClientId,
                            ClientCode = record.ClientCode,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            CompanyName = record.Company,
                            Status = record.Status,
                            PaymentStatus = record.PaymentStatus,
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            NumberOfStaff = record.NoOfStaffs, 
                            StaffMembers = staffList,
                            SourceDocuments = record.SourceDocuments
                        };
                    }
                    else
                    {
                        req = new
                        {
                            Id = record.ID,
                            RecordCode = record.Code,
                            ClientName = record.ClientName,
                            CompanyName = record.Company,
                            Description = record.Description,
                            Assignment = record.Assignment,
                            Status = record.Status,
                            PaymentStatus = record.PaymentStatus,
                            Process = record.Process,
                            CurrentStep = record.CurrentStep,
                            Country = record.Country,
                            CountryAddress = record.CountryAddress,
                            InvestmentValueUsd = record.InvestmentValue,
                            SubTotal = record.SubTotal,
                            Discount = record.Discount,
                            TotalPayment = record.TotalPayment,
                            PartialAmount = record.PartialAmount,
                            PaymentOption = record.PaymentOption,
                            NoOfStaffs = record.NoOfStaffs,
                            NumberOfStaff = record.NoOfStaffs,
                            Date = record.Date,
                            ClientId = record.ClientId,
                            BranchId = record.BranchId,
                            BranchName = record.Branch,
                            SourceDocuments = record.SourceDocuments
                        };
                    }
                    
                    string url = $"/api/v1/Secretarial/{endpoint}/{id}";
                    Console.WriteLine($"[DEBUG] Updating Secretarial Record: {url}");
                    var json = System.Text.Json.JsonSerializer.Serialize(req);
                    Console.WriteLine($"[DEBUG] Request Body: {json}");
                    
                    await ApiService.Instance.PutAsync(url, req);
                    Console.WriteLine($"[DEBUG] Update successful for {id}");
                }
                else
                {
                    string endpoint = MapAuditCategoryToEndpoint(category);
                    string url = endpoint.StartsWith("/") ? endpoint : $"/api/v1/Audit/{endpoint}";
                    Console.WriteLine($"[DEBUG] Updating Audit Record: {url}/{record.ID}");
                    await ApiService.Instance.PutAsync($"{url}/{record.ID}", record);
                    Console.WriteLine($"[DEBUG] Update successful for {record.ID}");
                }
                MockData.SyncOutstandingBalanceRecord(category, record);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to update record ({category}): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[INNER ERROR] {ex.InnerException.Message}");
                }
                throw; // Rethrow to allow UI to handle if needed
            }
        }

        public async Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records)
        {
            string baseUrl = IsSecretarialCategory(category) ? "/api/v1/Secretarial/" : "/api/v1/Audit/";
            string endpoint = IsSecretarialCategory(category) ? MapSecretarialCategoryToEndpoint(category) : MapAuditCategoryToEndpoint(category);
            
            foreach (var r in records)
            {
                string id = r.ID ?? string.Empty;
                await ApiService.Instance.DeleteAsync($"{baseUrl}{endpoint}/{id}");
                MockData.OutstandingBalances.RemoveAll(ob => ob.InvoiceNumber == r.Code || ob.InvoiceNumber == r.ID);
            }
        }

        // Tax Filing
        public async Task<List<TaxRecord>> GetTaxRecordsAsync(string category)
        {
            try
            {
                var endpoint = IsTaxFilingCategory(category) 
                    ? $"/api/v1/Tax/{MapTaxCategoryToEndpoint(category)}" 
                    : "/api/v1/Tax/records";
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<TaxRecord>>>(endpoint);
                var items = response?.Data?.Items ?? new List<TaxRecord>();
                foreach (var item in items)
                {
                    item.Branch = NormalizeBranchName(item.Branch);
                }
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Tax Records ({category}): {ex.Message}");
                return new List<TaxRecord>();
            }
        }

        public async Task AddTaxRecordAsync(string category, TaxRecord record)
        {
            try
            {
                var endpoint = IsTaxFilingCategory(category) 
                    ? $"/api/v1/Tax/{MapTaxCategoryToEndpoint(category)}" 
                    : "/api/v1/Tax/records";
                
                var json = System.Text.Json.JsonSerializer.Serialize(record);
                Console.WriteLine($"[DEBUG] AddTaxRecordAsync: Category={category}, Endpoint={endpoint}, JSON={json}");
                await ApiService.Instance.PostAsync(endpoint, record);
                Console.WriteLine("[DEBUG] AddTaxRecordAsync: PostAsync completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error adding Tax Record: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateTaxRecordAsync(string category, TaxRecord record)
        {
            var endpoint = IsTaxFilingCategory(category) 
                ? $"/api/v1/Tax/{MapTaxCategoryToEndpoint(category)}/{record.ID}" 
                : $"/api/v1/Tax/records/{record.ID}";
            await ApiService.Instance.PutAsync(endpoint, record);
        }

        public async Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records)
        {
            foreach (var r in records)
            {
                var endpoint = IsTaxFilingCategory(category) 
                    ? $"/api/v1/Tax/{MapTaxCategoryToEndpoint(category)}/{r.ID}" 
                    : $"/api/v1/Tax/records/{r.ID}";
                await ApiService.Instance.DeleteAsync(endpoint);
            }
        }

        public async Task<List<OutstandingBalanceRecord>> GetOutstandingBalancesAsync()
        {
            await Task.Delay(100); // Simulate network delay
            return MockData.OutstandingBalances;
        }

        private class DashboardStats
        {
            public int TotalSecretarialRecords { get; set; }
        }

        public async Task<int> GetTotalSecretarialRecordsAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<DashboardStats>>("/api/v1/dashboard/stats");
                return response?.Data?.TotalSecretarialRecords ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching total secretarial records: {ex.Message}");
                return 0;
            }
        }
    }
}
