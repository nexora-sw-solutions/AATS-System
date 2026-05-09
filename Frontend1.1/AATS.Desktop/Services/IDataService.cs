using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services
{
    public interface IDataService
    {
        Task<List<NexoraRequest>> GetNexoraRequestsAsync();
        Task<List<TeamMember>> GetTeamMembersAsync();
        Task<List<ClientRecord>> GetClientsAsync();
        Task<List<ActivityLogEntry>> GetActivityLogsAsync();
        Task<List<AuditRecord>> GetAuditRecordsAsync(string category);
        Task AddAuditRecordAsync(string category, AuditRecord record);
        Task UpdateAuditRecordAsync(string category, AuditRecord record);
        Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records);

        // Tax Filing
        Task<List<TaxRecord>> GetTaxRecordsAsync(string category);
        Task AddTaxRecordAsync(string category, TaxRecord record);
        Task UpdateTaxRecordAsync(string category, TaxRecord record);
        Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records);
    }
}
