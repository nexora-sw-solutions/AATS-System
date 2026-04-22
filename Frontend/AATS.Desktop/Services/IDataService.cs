using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services
{
    public interface IDataService
    {
        TeamMember CurrentUser { get; }

        Task<bool> LoginAsync(string usernameOrEmail, string password);
        Task RequestPasswordResetAsync(PasswordResetRequest request);
        Task UpdateCurrentUserProfileAsync(TeamMember member, string? currentPassword, string? newPassword);

        Task<List<NexoraRequest>> GetNexoraRequestsAsync();

        Task<List<TeamMember>> GetTeamMembersAsync();
        Task AddTeamMemberAsync(TeamMember member);
        Task UpdateTeamMemberAsync(TeamMember member);
        Task DeleteTeamMembersAsync(IEnumerable<TeamMember> members);

        Task<List<ClientRecord>> GetClientsAsync();
        Task AddClientAsync(ClientRecord client);
        Task UpdateClientAsync(ClientRecord client);
        Task DeleteClientsAsync(IEnumerable<ClientRecord> clients);

        Task<List<ActivityLogEntry>> GetActivityLogsAsync();
        Task AddActivityLogAsync(ActivityLogEntry entry);

        Task<List<AuditRecord>> GetAuditRecordsAsync(string category);
        Task AddAuditRecordAsync(string category, AuditRecord record);
        Task UpdateAuditRecordAsync(string category, AuditRecord record);
        Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records);

        Task<List<TaxRecord>> GetTaxRecordsAsync(string category);
        Task AddTaxRecordAsync(string category, TaxRecord record);
        Task UpdateTaxRecordAsync(string category, TaxRecord record);
        Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records);
    }
}
