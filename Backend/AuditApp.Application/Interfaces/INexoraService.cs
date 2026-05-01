using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Nexora;

namespace AuditApp.Application.Interfaces;

public interface INexoraService
{
    Task<PaginatedResult<NexoraServiceRequestResponse>> GetRequestsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<NexoraServiceRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task<NexoraServiceRequestResponse> CreateRequestAsync(CreateNexoraRequestDto request, CancellationToken ct = default);
    Task<NexoraServiceRequestResponse> UpdateRequestAsync(Guid id, UpdateNexoraRequestDto request, CancellationToken ct = default);
    Task DeleteRequestAsync(Guid id, CancellationToken ct = default);
    
    Task<List<NexoraServiceResponse>> GetServicesAsync(CancellationToken ct = default);
}
