using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Nexora;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class NexoraService : INexoraService
{
    private readonly IApplicationDbContext _db;

    public NexoraService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<NexoraServiceRequestResponse>> GetRequestsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.NexoraServiceRequests
            .Include(r => r.Service)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
        {
            query = query.Where(r => r.ClientName.Contains(@params.Search) || 
                                   (r.CompanyName != null && r.CompanyName.Contains(@params.Search)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<NexoraServiceRequestResponse>(
            items.Select(r => MapToResponse(r)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<NexoraServiceRequestResponse?> GetRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _db.NexoraServiceRequests
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        return request != null ? MapToResponse(request) : null;
    }

    public async Task<NexoraServiceRequestResponse> CreateRequestAsync(CreateNexoraRequestDto request, CancellationToken ct = default)
    {
        // Try to find the service by name
        var service = await _db.NexoraServices
            .FirstOrDefaultAsync(s => s.Name == request.ServiceName, ct);

        var nexoraRequest = new NexoraServiceRequest
        {
            Id = Guid.NewGuid(),
            RequestDate = DateOnly.FromDateTime(request.Date),
            ClientId = null, // Can be extended to link to existing client
            ClientName = $"{request.ClientFirstName} {request.ClientLastName}".Trim(),
            CompanyName = request.CompanyName,
            ServiceId = service?.Id,
            Phone = request.Phone,
            Notes = request.Notes,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BranchId = null // Can be set if branch context is available
        };

        if (string.IsNullOrWhiteSpace(nexoraRequest.ClientName))
        {
            nexoraRequest.ClientName = "Unknown Client";
        }

        _db.NexoraServiceRequests.Add(nexoraRequest);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(nexoraRequest);
    }

    public async Task<NexoraServiceRequestResponse> UpdateRequestAsync(Guid id, UpdateNexoraRequestDto request, CancellationToken ct = default)
    {
        var nexoraRequest = await _db.NexoraServiceRequests
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Nexora request not found.");

        if (request.Date.HasValue) nexoraRequest.RequestDate = DateOnly.FromDateTime(request.Date.Value);
        
        if (request.ClientFirstName != null || request.ClientLastName != null)
        {
            var parts = nexoraRequest.ClientName.Split(' ', 2);
            var first = request.ClientFirstName ?? parts.ElementAtOrDefault(0) ?? "";
            var last = request.ClientLastName ?? parts.ElementAtOrDefault(1) ?? "";
            nexoraRequest.ClientName = $"{first} {last}".Trim();
        }

        if (request.CompanyName != null) nexoraRequest.CompanyName = request.CompanyName;
        if (request.Phone != null) nexoraRequest.Phone = request.Phone;
        if (request.Notes != null) nexoraRequest.Notes = request.Notes;
        if (request.Status != null) nexoraRequest.Status = request.Status;

        if (request.ServiceName != null)
        {
            var service = await _db.NexoraServices
                .FirstOrDefaultAsync(s => s.Name == request.ServiceName, ct);
            nexoraRequest.ServiceId = service?.Id;
        }

        await _db.SaveChangesAsync(ct);
        return MapToResponse(nexoraRequest);
    }

    public async Task DeleteRequestAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _db.NexoraServiceRequests.FindAsync([id], ct);
        if (request != null)
        {
            request.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<List<NexoraServiceResponse>> GetServicesAsync(CancellationToken ct = default)
    {
        return await _db.NexoraServices
            .Where(s => s.IsActive)
            .Select(s => new NexoraServiceResponse
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);
    }

    private static NexoraServiceRequestResponse MapToResponse(NexoraServiceRequest r)
    {
        var nameParts = r.ClientName.Split(' ', 2);
        return new NexoraServiceRequestResponse
        {
            Id = r.Id,
            RecordCode = $"NEX-{r.Id.ToString()[..8].ToUpper()}",
            Date = r.RequestDate.ToDateTime(TimeOnly.MinValue),
            ClientId = r.ClientId,
            ClientFirstName = nameParts.ElementAtOrDefault(0) ?? "",
            ClientLastName = nameParts.ElementAtOrDefault(1) ?? "",
            CompanyName = r.CompanyName,
            ServiceId = r.ServiceId,
            ServiceName = r.Service?.Name,
            Phone = r.Phone,
            Notes = r.Notes,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        };
    }
}
