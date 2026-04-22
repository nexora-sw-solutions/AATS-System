using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;

namespace AuditApp.Application.Interfaces;

public interface IPaymentService
{
    Task<PaginatedResult<PaymentResponse>> GetPaymentsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<PaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task UpdateChequeStatusAsync(Guid chequeId, Domain.Enums.ChequeStatus status, CancellationToken ct = default);
    Task<List<PaymentResponse>> GetPaymentsByReferenceAsync(Guid referenceId, string referenceType, CancellationToken ct = default);
}

public interface INexoraAppService
{
    Task<List<NexoraServiceResponse>> GetActiveServicesAsync(CancellationToken ct = default);
    Task<PaginatedResult<NexoraRequestResponse>> GetRequestsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<NexoraRequestResponse> CreateRequestAsync(CreateNexoraRequest request, CancellationToken ct = default);
}
