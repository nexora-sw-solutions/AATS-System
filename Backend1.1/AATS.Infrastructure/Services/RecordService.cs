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
            var prefix = modulePrefix switch
            {
                "AUD-ASR" => "AUD-AS",
                "AUD-INT" => "INT-AUD",
                "AUD-FOR" => "FOR-AUD",
                "AUD-CTL" => "CTL-AUD",
                "AUD-MGT" => "MGT-ACC",
                _ => modulePrefix.Replace("AUD-", "")
            };

            string? maxCode = modulePrefix switch
            {
                "AUD-ASR" => await _context.AuditAssuranceRecords.MaxAsync(r => (string?)r.RecordCode),
                "AUD-INT" => await _context.InternalAuditRecords.MaxAsync(r => (string?)r.RecordCode),
                "AUD-FOR" => await _context.ForensicAuditRecords.MaxAsync(r => (string?)r.RecordCode),
                "AUD-CTL" => await _context.InternalControlRecords.MaxAsync(r => (string?)r.RecordCode),
                "AUD-MGT" => await _context.ManagementAccountRecords.MaxAsync(r => (string?)r.RecordCode),
                "AUD-OTH" => await _context.OtherAuditRecords.MaxAsync(r => (string?)r.RecordCode),
                "TAX-ACC" => await _context.TaxAccountRecords.MaxAsync(r => (string?)r.RecordCode),
                "TAX-FIL" => await _context.TaxFilings.MaxAsync(r => (string?)r.RecordCode),
                "TAX-VAT" => await _context.VatFilings.MaxAsync(r => (string?)r.RecordCode),
                "TAX-CIT" => await _context.CitFilings.MaxAsync(r => (string?)r.RecordCode),
                "TAX-IIT" => await _context.IitFilings.MaxAsync(r => (string?)r.RecordCode),
                "TAX-SSC" => await _context.SsclFilings.MaxAsync(r => (string?)r.RecordCode),
                "TAX-WHT" => await _context.WhtFilings.MaxAsync(r => (string?)r.RecordCode),
                "SEC-REG" => await _context.CompanyRegistrations.MaxAsync(r => (string?)r.RecordCode),
                "SEC-EPF" => await _context.EpfEtfRecords.MaxAsync(r => (string?)r.RecordCode),
                "SEC-TRD" => await _context.TradeMarks.MaxAsync(r => (string?)r.RecordCode),
                "SEC-LIC" => await _context.TradeLicenses.MaxAsync(r => (string?)r.RecordCode),
                "SEC-IMP" => await _context.ImportExportClearances.MaxAsync(r => (string?)r.RecordCode),
                "SEC-BOI" => await _context.BoiRegistrations.MaxAsync(r => (string?)r.RecordCode),
                "SEC-BUS" => await _context.BusinessPlanValuations.MaxAsync(r => (string?)r.RecordCode),
                "SEC-HR" => await _context.HrManagementConsulting.MaxAsync(r => (string?)r.RecordCode),
                "SEC-OTH" => await _context.OtherSecretarialRecords.MaxAsync(r => (string?)r.RecordCode),
                _ => await _context.ActivityLogs.Where(l => l.Module == modulePrefix).MaxAsync(l => (string?)l.Id.ToString())
            };

            int nextNum = 1;
            if (!string.IsNullOrEmpty(maxCode))
            {
                var parts = maxCode.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out int currentMax))
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
                client.TotalRevenue += amount;
                client.OutstandingBalance += amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task LogActivityAsync(Guid? userId, Guid? branchId, string action, string module, Guid recordId, string description)
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

        public async Task ProcessChequeDetailsAsync(RecordBase record, string module)
        {
            if (record.PaymentOption != "Cheque" || string.IsNullOrWhiteSpace(record.ChequeBank) || string.IsNullOrWhiteSpace(record.ChequeNumber))
                return;

            var payment = await _context.Set<Payment>().FirstOrDefaultAsync(p => p.RecordId == record.Id);
            if (payment == null)
            {
                payment = new Payment
                {
                    RecordType = module,
                    RecordId = record.Id,
                    PaymentDate = DateTime.UtcNow,
                    Amount = record.ChequeAmount ?? record.TotalPayment,
                    PaymentMethod = "Cheque",
                    Notes = $"Initial cheque payment for {record.RecordCode}"
                };
                _context.Set<Payment>().Add(payment);
                await _context.SaveChangesAsync();
            }
            else
            {
                payment.Amount = record.ChequeAmount ?? record.TotalPayment;
                _context.Set<Payment>().Update(payment);
                await _context.SaveChangesAsync();
            }

            var chequeDetail = await _context.ChequeDetails.FirstOrDefaultAsync(c => c.PaymentId == payment.Id);
            if (chequeDetail == null)
            {
                chequeDetail = new ChequeDetail
                {
                    PaymentId = payment.Id,
                    BankName = record.ChequeBank,
                    ChequeNumber = record.ChequeNumber,
                    ChequeDate = record.ChequeDate ?? DateTime.UtcNow,
                    Status = record.ChequeStatus ?? "Pending"
                };
                _context.ChequeDetails.Add(chequeDetail);
            }
            else
            {
                chequeDetail.BankName = record.ChequeBank;
                chequeDetail.ChequeNumber = record.ChequeNumber;
                chequeDetail.ChequeDate = record.ChequeDate ?? DateTime.UtcNow;
                chequeDetail.Status = record.ChequeStatus ?? "Pending";
                _context.ChequeDetails.Update(chequeDetail);
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnrichRecordsAsync<T>(System.Collections.Generic.IEnumerable<T> records) where T : RecordBase
        {
            var userIds = records.Where(r => r.CreatedBy.HasValue).Select(r => r.CreatedBy!.Value).Distinct().ToList();
            if (userIds.Any())
            {
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);
                foreach (var record in records)
                {
                    if (record.CreatedBy.HasValue && users.TryGetValue(record.CreatedBy.Value, out var username))
                    {
                        record.CreatedByName = username;
                    }
                }
            }
        }
    }
}
