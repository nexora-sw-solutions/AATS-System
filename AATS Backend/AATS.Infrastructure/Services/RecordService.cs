using System;
using System.Linq;
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
            var prefix = modulePrefix switch
            {
                "AUD-ASR" => "AUD-AS",
                "AUD-INT" => "INT-AUD",
                "AUD-FOR" => "FOR-AUD",
                "AUD-CTL" => "CTL-AUD",
                "AUD-MGT" => "MGT-ACC",
                _ => modulePrefix.Replace("AUD-", "")
            };

            string? maxCode = null;

            if (modulePrefix.StartsWith("AUD-") || modulePrefix.StartsWith("SEC-"))
            {
                maxCode = await _context.AuditRecords
                    .Where(r => r.RecordCode != null && r.RecordCode.StartsWith(prefix))
                    .MaxAsync(r => (string?)r.RecordCode);
            }
            else if (modulePrefix.StartsWith("TAX-"))
            {
                maxCode = await _context.TaxRecords
                    .Where(r => r.RecordCode != null && r.RecordCode.StartsWith(prefix))
                    .MaxAsync(r => (string?)r.RecordCode);
            }
            else if (modulePrefix == "NEX")
            {
                maxCode = await _context.NexoraRequests
                    .MaxAsync(r => (string?)r.Id.ToString());
            }

            int nextNum = 1;
            if (!string.IsNullOrEmpty(maxCode))
            {
                var parts = maxCode.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int currentMax))
                {
                    nextNum = currentMax + 1;
                }
            }

            return $"{prefix} {nextNum:D6}";
        }

        public async Task UpdateClientBalanceAsync(Guid clientId, decimal amount)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client != null)
            {
                client.OutstandingBalance += amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task LogActivityAsync(Guid? userId, Guid? branchId, string action, string module, Guid recordId, string description)
        {
            string? userName = null;
            string? branchName = null;

            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null) userName = user.Username;
            }

            if (branchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(branchId.Value);
                if (branch != null) branchName = branch.Name;
            }

            var log = new ActivityLog
            {
                UserId = userId,
                UserName = userName,
                BranchId = branchId,
                BranchName = branchName,
                Action = action,
                Module = module,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
