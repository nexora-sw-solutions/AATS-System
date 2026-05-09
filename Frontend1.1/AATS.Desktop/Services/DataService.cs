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

        public async Task AddNexoraRequestAsync(NexoraRequest request)
        {
            await ApiService.Instance.PostAsync("/api/v1/Nexora/requests", request);
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
                return response?.Data?.Items ?? new List<TeamMember>();
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

        public async Task<List<Branch>> GetBranchesAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<Branch>>>("/api/v1/branches");
                return response?.Data?.Items ?? new List<Branch>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Branches: {ex.Message}");
                return new List<Branch>();
            }
        }

        public async Task<List<ClientRecord>> GetClientsAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ClientRecord>>>("/api/v1/clients");
                var items = response?.Data?.Items ?? new List<ClientRecord>();
                return items.OrderBy(c => c.ClientCode).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Clients: {ex.Message}");
                return new List<ClientRecord>();
            }
        }

        public async Task AddClientAsync(ClientRecord client)
        {
            await ApiService.Instance.PostAsync("/api/v1/clients", client);
        }

        public async Task UpdateClientAsync(ClientRecord client)
        {
            await ApiService.Instance.PutAsync($"/api/v1/clients/{client.Id}", client);
        }

        public async Task DeleteClientsAsync(IEnumerable<ClientRecord> clients)
        {
            foreach (var c in clients)
            {
                await ApiService.Instance.DeleteAsync($"/api/v1/clients/{c.Id}");
            }
        }

        public async Task<List<ActivityLogEntry>> GetActivityLogsAsync()
        {
            // Fallback since API wasn't provided for activity logs
            return MockData.ActivityLogs;
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
                "Audit Others" => "others",
                "Others" => "others",
                "Tax Account" => "tax",
                "Tax Accountings" => "tax",
                "Tax" => "tax",
                _ => category.ToLower().Replace(" ", "-").Replace("&", "and")
            };
        }

        private bool IsSecretarialCategory(string category)
        {
            var secretarialCategories = new[]
            {
                "Company Registration", "EPF/ETF", "Trade Marks", "Trade Licenses",
                "Import/Export", "HR Consulting", "Business Plans", "BOI Registrations", "Other Secretarial"
            };
            return secretarialCategories.Contains(category);
        }

        private string MapSecretarialCategoryToEndpoint(string category)
        {
            return category switch
            {
                "Company Registration" => "company-registrations",
                "EPF/ETF" => "epf-etf",
                "Trade Marks" => "trade-marks",
                "Trade Licenses" => "trade-licenses",
                "Import/Export" => "import-export",
                "HR Consulting" => "hr-consulting",
                "Business Plans" => "business-plans",
                "BOI Registrations" => "boi-registrations",
                "Other Secretarial" => "others",
                _ => category.ToLower().Replace(" ", "-")
            };
        }

        public async Task<List<AuditRecord>> GetAuditRecordsAsync(string category)
        {
            try
            {
                string baseUrl = IsSecretarialCategory(category) ? "/api/v1/Secretarial/" : "/api/v1/Audit/";
                string endpoint = IsSecretarialCategory(category) ? MapSecretarialCategoryToEndpoint(category) : MapAuditCategoryToEndpoint(category);
                
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<AuditRecord>>>(baseUrl + endpoint);
                return response?.Data?.Items ?? new List<AuditRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Records ({category}): {ex.Message}");
                return new List<AuditRecord>();
            }
        }

        public async Task<AuditRecord?> GetRecordByIdAsync(string category, string id)
        {
            try
            {
                string baseUrl = IsSecretarialCategory(category) ? "/api/v1/Secretarial/" : "/api/v1/Audit/";
                string endpoint = IsSecretarialCategory(category) ? MapSecretarialCategoryToEndpoint(category) : MapAuditCategoryToEndpoint(category);

                var response = await ApiService.Instance.GetAsync<ApiResponse<AuditRecord>>(baseUrl + endpoint + "/" + id);
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
                            .ToList()
                    };
                    await ApiService.Instance.PostAsync($"/api/v1/Secretarial/{endpoint}", req);
                }
                else if (category == "EPF/ETF")
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
                        Staff = record.StaffList?.Select(s => new { Name = s.StaffName, Nic = s.Phone, EpfNumber = s.StaffId }).ToList()
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
                        NoOfStaffs = record.NoOfStaffs
                    };
                    await ApiService.Instance.PostAsync($"/api/v1/Secretarial/{endpoint}", req);
                }
            }
            else
            {
                string endpoint = MapAuditCategoryToEndpoint(category);
                await ApiService.Instance.PostAsync($"/api/v1/Audit/{endpoint}", record);
            }
        }

        public async Task UpdateAuditRecordAsync(string category, AuditRecord record)
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
                        DirectorNames = string.Join(", ", record.DirectorsList?.Select(d => d.Name) ?? Enumerable.Empty<string>()),
                        SecretaryNames = string.Join(", ", record.SecretariesList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                        ShareholderNames = string.Join(", ", record.ShareholdersList?.Select(s => s.Name) ?? Enumerable.Empty<string>()),
                        OtherNames = string.Join(", ", record.OthersList?.Select(o => o.Detail) ?? Enumerable.Empty<string>()),
                        Officers = record.DirectorsList?.Select(d => new { Name = d.Name, Position = "Director", NicNumber = d.TIN }).ToList()
                            .Concat(record.SecretariesList?.Select(s => new { Name = s.Name, Position = "Secretary", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                            .Concat(record.ShareholdersList?.Select(s => new { Name = s.Name, Position = "Shareholder", NicNumber = s.TIN }) ?? Enumerable.Empty<object>())
                            .Concat(record.OthersList?.Select(o => new { Name = o.Detail, Position = "Other", NicNumber = "" }) ?? Enumerable.Empty<object>())
                            .ToList()
                    };
                }
                else
                {
                    req = new
                    {
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
                        Date = record.Date,
                        ClientId = record.ClientId,
                        BranchId = record.BranchId,
                        BranchName = record.Branch
                    };
                }
                
                await ApiService.Instance.PutAsync($"/api/v1/Secretarial/{endpoint}/{id}", req);
            }
            else
            {
                string endpoint = MapAuditCategoryToEndpoint(category);
                await ApiService.Instance.PutAsync($"/api/v1/Audit/{endpoint}/{record.ID}", record);
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
            }
        }

        // Tax Filing
        public async Task<List<TaxRecord>> GetTaxRecordsAsync(string category)
        {
            try
            {
                var endpoint = category.ToLower() == "filings" ? "/api/v1/Tax/filings" : "/api/v1/Tax/records";
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<TaxRecord>>>(endpoint);
                return response?.Data?.Items ?? new List<TaxRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Tax Records ({category}): {ex.Message}");
                return new List<TaxRecord>();
            }
        }

        public async Task AddTaxRecordAsync(string category, TaxRecord record)
        {
            var endpoint = category.ToLower() == "filings" ? "/api/v1/Tax/filings" : "/api/v1/Tax/records";
            await ApiService.Instance.PostAsync(endpoint, record);
        }

        public async Task UpdateTaxRecordAsync(string category, TaxRecord record)
        {
            var endpoint = category.ToLower() == "filings" ? $"/api/v1/Tax/filings/{record.ID}" : $"/api/v1/Tax/records/{record.ID}";
            await ApiService.Instance.PutAsync(endpoint, record);
        }

        public async Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records)
        {
            foreach (var r in records)
            {
                var endpoint = category.ToLower() == "filings" ? $"/api/v1/Tax/filings/{r.ID}" : $"/api/v1/Tax/records/{r.ID}";
                await ApiService.Instance.DeleteAsync(endpoint);
            }
        }
    }
}
