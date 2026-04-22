using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Tax;

namespace AuditApp.Application.Interfaces;

public interface ITaxService
{
    // Tax Filings
    Task<PaginatedResult<TaxFilingResponse>> GetFilingsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<TaxFilingResponse?> GetFilingByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaxFilingResponse> CreateFilingAsync(CreateTaxFilingRequest request, CancellationToken ct = default);
    Task<TaxFilingResponse> UpdateFilingAsync(Guid id, UpdateTaxFilingRequest request, CancellationToken ct = default);
    Task DeleteFilingAsync(Guid id, CancellationToken ct = default);

    // Tax Account Records
    Task<PaginatedResult<TaxAccountRecordResponse>> GetAccountRecordsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<TaxAccountRecordResponse?> GetAccountRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaxAccountRecordResponse> CreateAccountRecordAsync(CreateTaxAccountRecordRequest request, CancellationToken ct = default);
    Task<TaxAccountRecordResponse> UpdateAccountRecordAsync(Guid id, UpdateTaxAccountRecordRequest request, CancellationToken ct = default);
    Task DeleteAccountRecordAsync(Guid id, CancellationToken ct = default);
    Task<TaxAccountRecordResponse> UpdateAccountProcessAsync(Guid id, string process, CancellationToken ct = default);
    Task<TaxAccountRecordResponse> AssignStaffAsync(Guid id, Guid assignedToId, CancellationToken ct = default);
}
