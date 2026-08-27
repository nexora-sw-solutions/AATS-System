using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services
{
    public interface IDataService
    {
        Task<List<NexoraRequest>> GetNexoraRequestsAsync();
        Task<List<TeamMember>> GetTeamMembersAsync();
        Task<List<ClientRecord>> GetClientsAsync(bool forceRefresh = false);
        Task<List<ActivityLogEntry>> GetActivityLogsAsync();
        Task<List<AuditRecord>> GetAuditRecordsAsync(string category, bool enrich = true);
        Task AddAuditRecordAsync(string category, AuditRecord record);
        Task UpdateAuditRecordAsync(string category, AuditRecord record);
        Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records);

        // Tax Filing
        Task<List<TaxRecord>> GetTaxRecordsAsync(string category);
        Task AddTaxRecordAsync(string category, TaxRecord record);
        Task UpdateTaxRecordAsync(string category, TaxRecord record);
        Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records);

        // Outstanding Balances
        Task<List<OutstandingBalanceRecord>> GetOutstandingBalancesAsync();
        Task<int> GetTotalSecretarialRecordsAsync();

        // Branches
        Task<List<Branch>> GetBranchesAsync();

        // Trash / Soft Delete Operations
        Task<List<ClientRecord>> GetDeletedClientsAsync();
        Task<bool> RestoreClientAsync(string id);
        Task<bool> PermanentlyDeleteClientAsync(string id);

        Task<List<TeamMember>> GetDeletedTeamMembersAsync();
        Task<bool> RestoreTeamMemberAsync(string id);
        Task<bool> PermanentlyDeleteTeamMemberAsync(string id);

        Task<List<AuditRecord>> GetDeletedAuditRecordsAsync(string category);
        Task<bool> RestoreAuditRecordAsync(string category, string id);
        Task<bool> PermanentlyDeleteAuditRecordAsync(string category, string id);
    }
}
