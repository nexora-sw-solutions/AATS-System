using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Audit;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using AuditApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _db;

    public AuditService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<AuditRecordResponse>> GetRecordsAsync<TEntity>(PaginationParams @params, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var query = set
            .Include(r => r.Branch)
            .Include(r => r.Client)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
            query = query.Where(r => r.ClientName.Contains(@params.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<AuditRecordResponse>(
            items.Select(r => MapToResponse(r)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<AuditRecordResponse?> GetRecordByIdAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set
            .Include(r => r.Branch)
            .Include(r => r.Client)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return record != null ? MapToResponse(record) : null;
    }

    public async Task<AuditRecordResponse> CreateRecordAsync<TEntity>(CreateAuditRecordRequest request, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = Activator.CreateInstance<TEntity>();

        // Generate RecordCode
        var prefix = typeof(TEntity).Name.Replace("Record", "").ToUpper()[..3];
        record.RecordCode = $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        // Map request to record using reflection (since AuditBaseEntity properties are well-defined)
        record.RecordDate = request.Date;
        record.ClientName = request.ClientName;
        record.ClientId = request.ClientId;
        record.BranchId = request.BranchId;
        record.Process = request.Status ?? "Pending"; 
        record.Notes = request.Description;
        record.PaymentStatus = request.PaymentStatus?.ToString();
        record.PaymentOption = request.PaymentOption?.ToString();
        record.TotalPayment = request.TotalFee;
        record.PartialAmount = request.PaidAmount;
        record.SubTotal = request.ServiceFee;
        record.Discount = request.TotalFee - request.ServiceFee - request.GovFee; // Rough estimate

        // Handling assignment vs assigned_to
        if (record is TaxAccountRecord tar)
        {
            tar.AssignedTo = request.AssignedToId;
        }
        else
        {
            var assignmentProp = typeof(TEntity).GetProperty("Assignment");
            assignmentProp?.SetValue(record, request.Description);
        }

        set.Add(record);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(record);
    }

    public async Task DeleteRecordAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.FindAsync([id], ct);
        if (record != null)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<AuditRecordResponse> UpdateRecordAsync<TEntity>(Guid id, UpdateAuditRecordRequest request, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.Include(r => r.Branch).Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");

        if (request.ClientName != null) record.ClientName = request.ClientName;
        if (request.Date.HasValue) record.RecordDate = request.Date.Value;
        if (request.BranchId.HasValue) record.BranchId = request.BranchId;
        if (request.ClientId.HasValue) record.ClientId = request.ClientId;
        if (request.Process != null) record.Process = request.Process;
        if (request.Description != null) record.Notes = request.Description;
        if (request.PaymentStatus.HasValue) record.PaymentStatus = request.PaymentStatus.Value.ToString();
        if (request.PaymentOption.HasValue) record.PaymentOption = request.PaymentOption.Value.ToString();
        if (request.ServiceFee.HasValue) record.SubTotal = request.ServiceFee.Value;
        if (request.TotalFee.HasValue) record.TotalPayment = request.TotalFee.Value;
        if (request.PaidAmount.HasValue) record.PartialAmount = request.PaidAmount.Value;

        var assignmentProp = typeof(TEntity).GetProperty("Assignment");
        if (request.Description != null && assignmentProp != null) assignmentProp.SetValue(record, request.Description);

        await _db.SaveChangesAsync(ct);
        return MapToResponse(record);
    }

    public async Task<AuditRecordResponse> UpdateProcessAsync<TEntity>(Guid id, string process, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.Include(r => r.Branch).Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");

        record.Process = process;
        await _db.SaveChangesAsync(ct);
        return MapToResponse(record);
    }

    public async Task<AuditRecordResponse> UpdatePaymentAsync<TEntity>(Guid id, string? paymentStatus, string? paymentOption, decimal? subTotal, decimal? totalPayment, decimal? partialAmount, CancellationToken ct = default) where TEntity : AuditBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.Include(r => r.Branch).Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");

        if (paymentStatus != null) record.PaymentStatus = paymentStatus;
        if (paymentOption != null) record.PaymentOption = paymentOption;
        if (subTotal.HasValue) record.SubTotal = subTotal.Value;
        if (totalPayment.HasValue) record.TotalPayment = totalPayment.Value;
        if (partialAmount.HasValue) record.PartialAmount = partialAmount.Value;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(record);
    }


    private DbSet<TEntity> GetDbSet<TEntity>() where TEntity : AuditBaseEntity
    {
        return _db.GetType().GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(DbSet<TEntity>))
            ?.GetValue(_db) as DbSet<TEntity>
            ?? throw new InvalidOperationException($"DbSet for {typeof(TEntity).Name} not found.");
    }

    private static AuditRecordResponse MapToResponse<TEntity>(TEntity r) where TEntity : AuditBaseEntity
    {
        return new AuditRecordResponse
        {
            Id = r.Id,
            ClientName = r.ClientName,
            Date = r.RecordDate,
            BranchId = r.BranchId,
            BranchName = r.Branch?.Name,
            Description = r.Notes,
            AssignedToId = (r as TaxAccountRecord)?.AssignedTo,
            AssignedToName = (r as TaxAccountRecord)?.AssignedUser?.Username,
            Process = r.Process,
            ClientId = r.ClientId,
            PaymentStatus = Enum.TryParse<PaymentStatus>(r.PaymentStatus, out var ps) ? ps : null,
            PaymentOption = Enum.TryParse<PaymentOption>(r.PaymentOption, out var po) ? po : null,
            ServiceFee = r.SubTotal,
            TotalFee = r.TotalPayment,
            PaidAmount = r.PartialAmount,
            Status = r.Process,
            CreatedAt = r.CreatedAt
        };
    }
}
