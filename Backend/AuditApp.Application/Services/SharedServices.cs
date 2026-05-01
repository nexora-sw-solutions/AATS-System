using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _db;

    public PaymentService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<PaymentResponse>> GetPaymentsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.Payments
            .Include(p => p.ChequeDetails)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<PaymentResponse>(
            items.Select(p => MapToPaymentResponse(p)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .Include(p => p.ChequeDetails)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return payment != null ? MapToPaymentResponse(payment) : null;
    }

    public async Task<List<PaymentResponse>> GetPaymentsByReferenceAsync(Guid referenceId, string referenceType, CancellationToken ct = default)
    {
        var payments = await _db.Payments
            .Include(p => p.ChequeDetails)
            .Where(p => p.RecordId == referenceId && p.RecordType == referenceType)
            .AsNoTracking()
            .ToListAsync(ct);

        return payments.Select(p => MapToPaymentResponse(p)).ToList();
    }

    public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        var payment = new Payment
        {
            RecordId = request.ReferenceId ?? Guid.Empty,
            RecordType = request.ReferenceType,
            TotalAmount = request.Amount,
            PaymentDate = request.Date,
            PaymentOption = request.Option,
            Notes = request.Description,
            ChequeDetails = request.Cheques.Select(c => new ChequeDetail
            {
                ChequeNumber = c.ChequeNumber,
                BankName = c.Bank,
                ChequeDate = c.DueDate,
                Status = c.Status.ToString()
            }).ToList()
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return MapToPaymentResponse(payment);
    }

    public async Task UpdateChequeStatusAsync(Guid chequeId, Domain.Enums.ChequeStatus status, CancellationToken ct = default)
    {
        var cheque = await _db.ChequeDetails.FindAsync([chequeId], ct)
            ?? throw new KeyNotFoundException("Cheque not found.");

        cheque.Status = status.ToString();
        await _db.SaveChangesAsync(ct);
    }

    private static PaymentResponse MapToPaymentResponse(Payment p) => new()
    {
        Id = p.Id,
        Amount = p.TotalAmount,
        Date = p.PaymentDate,
        Option = p.PaymentOption,
        ReferenceId = p.RecordId,
        ReferenceType = p.RecordType,
        Description = p.Notes,
        Cheques = p.ChequeDetails.Select(c => new ChequeDetailResponse
        {
            Id = c.Id,
            ChequeNumber = c.ChequeNumber,
            Bank = c.BankName,
            DueDate = c.ChequeDate ?? DateOnly.FromDateTime(DateTime.Now),
            Status = Enum.TryParse<Domain.Enums.ChequeStatus>(c.Status, out var s) ? s : Domain.Enums.ChequeStatus.Pending
        }).ToList(),
        CreatedAt = p.CreatedAt
    };
}

public class NexoraAppService : INexoraAppService
{
    private readonly IApplicationDbContext _db;

    public NexoraAppService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<NexoraServiceResponse>> GetActiveServicesAsync(CancellationToken ct = default)
    {
        return await _db.NexoraServices
            .Where(s => s.IsActive)
            .Select(s => new NexoraServiceResponse
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToListAsync(ct);
    }

    public async Task<PaginatedResult<NexoraRequestResponse>> GetRequestsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.NexoraServiceRequests
            .Include(r => r.Service)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<NexoraRequestResponse>(
            items.Select(r => new NexoraRequestResponse
            {
                Id = r.Id,
                ClientName = r.ClientName,
                ServiceName = r.Service?.Name,
                Status = r.Status,
                Message = r.Notes,
                CreatedAt = r.CreatedAt
            }).ToList(), @params.Page, @params.Limit, total);
    }

    public async Task<NexoraRequestResponse> CreateRequestAsync(CreateNexoraRequest request, CancellationToken ct = default)
    {
        var nexoraRequest = new NexoraServiceRequest
        {
            ServiceId = request.ServiceId,
            ClientName = request.ClientName,
            Notes = request.Message,
            Status = "Pending",
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _db.NexoraServiceRequests.Add(nexoraRequest);
        await _db.SaveChangesAsync(ct);

        var service = await _db.NexoraServices.FindAsync([request.ServiceId], ct);

        return new NexoraRequestResponse
        {
            Id = nexoraRequest.Id,
            ClientName = nexoraRequest.ClientName,
            ServiceName = service?.Name,
            Status = nexoraRequest.Status,
            Message = nexoraRequest.Notes,
            CreatedAt = nexoraRequest.CreatedAt
        };
    }
}
