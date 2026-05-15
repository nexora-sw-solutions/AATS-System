using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Domain.Entities;

namespace AATS.Application.Common.Interfaces
{
    public interface IRecordService
    {
        Task<string> GenerateRecordCodeAsync(string modulePrefix);
        Task UpdateClientBalanceAsync(Guid clientId, decimal amount);
        Task LogActivityAsync(Guid? userId, Guid? branchId, string action, string module, Guid recordId, string description);
        Task ProcessChequeDetailsAsync(RecordBase record, string module);
        Task EnrichRecordsAsync<T>(System.Collections.Generic.IEnumerable<T> records) where T : RecordBase;
    }
}
