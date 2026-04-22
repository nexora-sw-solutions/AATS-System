using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Audit;
using AuditApp.Domain.Entities;

namespace AuditApp.Application.Interfaces;

public interface IAuditService
{
    Task<PaginatedResult<AuditRecordResponse>> GetRecordsAsync<TEntity>(PaginationParams @params, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task<AuditRecordResponse?> GetRecordByIdAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task<AuditRecordResponse> CreateRecordAsync<TEntity>(CreateAuditRecordRequest request, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task<AuditRecordResponse> UpdateRecordAsync<TEntity>(Guid id, UpdateAuditRecordRequest request, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task DeleteRecordAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task<AuditRecordResponse> UpdateProcessAsync<TEntity>(Guid id, string process, CancellationToken ct = default) where TEntity : AuditBaseEntity;
    Task<AuditRecordResponse> UpdatePaymentAsync<TEntity>(Guid id, string? paymentStatus, string? paymentOption, decimal? subTotal, decimal? totalPayment, decimal? partialAmount, CancellationToken ct = default) where TEntity : AuditBaseEntity;
}
