using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AATS.Application.Common.Interfaces;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;

namespace AATS.Infrastructure.Services
{
    public class RecordService : IRecordService
    {
        private readonly ApplicationDbContext _context;

        public RecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateRecordCodeAsync(string modulePrefix)
        {
            // Standardize prefix based on module
            var prefix = modulePrefix switch
            {
                "AUD-ASR" => "AUD-AS",
                "AUD-INT" => "INT-AUD",
                "AUD-FOR" => "FOR-AUD",
                "AUD-CTL" => "CTL-AUD",
                "AUD-MGT" => "MGT-ACC",
                _ => modulePrefix.Replace("AUD-", "")
            };
            
            // Count from specific tables to ensure independent sequences
            int count = modulePrefix switch
            {
                "AUD-ASR" => await _context.AuditAssuranceRecords.CountAsync(),
                "AUD-INT" => await _context.InternalAuditRecords.CountAsync(),
                "AUD-FOR" => await _context.ForensicAuditRecords.CountAsync(),
                "AUD-CTL" => await _context.InternalControlRecords.CountAsync(),
                "AUD-MGT" => await _context.ManagementAccountRecords.CountAsync(),
                "TAX-AUD" => await _context.TaxAccountRecords.CountAsync(),
                "TAX-FLG" => await _context.TaxFilings.CountAsync(),
                "SEC-REG" => await _context.CompanyRegistrations.CountAsync(),
                "SEC-EPF" => await _context.EpfEtfRecords.CountAsync(),
                "SEC-TRD" => await _context.TradeMarks.CountAsync(),
                "SEC-LIC" => await _context.TradeLicenses.CountAsync(),
                "SEC-IMP" => await _context.ImportExportClearances.CountAsync(),
                "SEC-BOI" => await _context.BoiRegistrations.CountAsync(),
                "SEC-BUS" => await _context.BusinessPlanValuations.CountAsync(),
                "SEC-HR" => await _context.HrManagementConsulting.CountAsync(),
                "SEC-OTH" => await _context.OtherSecretarialRecords.CountAsync(),
                _ => await _context.ActivityLogs.CountAsync(l => l.Module == modulePrefix && l.Action == "CREATE")
            };
            
            count++;
            
            // Format: PREFIX 000001
            return $"{prefix} {count:D6}";
        }

        public async Task UpdateClientBalanceAsync(Guid clientId, decimal amount)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client != null)
            {
                client.TotalRevenue += amount;
                client.OutstandingBalance += amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task LogActivityAsync(Guid userId, Guid branchId, string action, string module, Guid recordId, string description)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                BranchId = branchId,
                Action = action,
                Module = module,
                RecordId = recordId,
                Description = description
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
