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
            await ApiService.Instance.PostAsync("/api/v1/users", member);
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

        public async Task<List<ClientRecord>> GetClientsAsync()
        {
            try
            {
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<ClientRecord>>>("/api/v1/clients");
                return response?.Data?.Items ?? new List<ClientRecord>();
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
                _ => category.ToLower().Replace(" ", "-").Replace("&", "and")
            };
        }

        public async Task<List<AuditRecord>> GetAuditRecordsAsync(string category)
        {
            try
            {
                string endpoint = MapAuditCategoryToEndpoint(category);
                var response = await ApiService.Instance.GetAsync<ApiResponse<PaginatedResult<AuditRecord>>>($"/api/v1/Audit/{endpoint}");
                return response?.Data?.Items ?? new List<AuditRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Audit Records ({category}): {ex.Message}");
                return new List<AuditRecord>();
            }
        }
        
        public async Task AddAuditRecordAsync(string category, AuditRecord record)
        {
            string endpoint = MapAuditCategoryToEndpoint(category);
            await ApiService.Instance.PostAsync($"/api/v1/Audit/{endpoint}", record);
        }

        public async Task UpdateAuditRecordAsync(string category, AuditRecord record)
        {
            string endpoint = MapAuditCategoryToEndpoint(category);
            await ApiService.Instance.PutAsync($"/api/v1/Audit/{endpoint}/{record.ID}", record);
        }

        public async Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records)
        {
            string endpoint = MapAuditCategoryToEndpoint(category);
            foreach (var r in records)
            {
                await ApiService.Instance.DeleteAsync($"/api/v1/Audit/{endpoint}/{r.ID}");
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
