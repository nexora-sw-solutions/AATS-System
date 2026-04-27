using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Tax;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using AuditApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class TaxService : ITaxService
{
    private readonly IApplicationDbContext _db;

    public TaxService(IApplicationDbContext db)
    {
        _db = db;
    }

    #region Tax Filings

    public async Task<PaginatedResult<TaxFilingResponse>> GetFilingsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.TaxFilings
            .Include(f => f.Branch)
            .Where(f => !f.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
        {
             query = query.Where(f => f.ClientName.Contains(@params.Search));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<TaxFilingResponse>(
            items.Select(f => MapToFilingResponse(f)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<TaxFilingResponse?> GetFilingByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filing = await _db.TaxFilings
            .Include(f => f.Branch)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);

        return filing != null ? MapToFilingResponse(filing) : null;
    }

    public async Task<TaxFilingResponse> CreateFilingAsync(CreateTaxFilingRequest request, CancellationToken ct = default)
    {
        var filing = new TaxFiling
        {
            FilingCode = $"TFL-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            ClientName = request.ClientName,
            TaxType = request.TaxType.ToString(),
            FilingDate = request.Period,
            TaxNumber = "PENDING", // TaxNumber not in DTO
            PeriodNumber = request.Period.ToString("yyyy-MM"),
            PeriodType = request.PeriodType?.ToString() ?? "Month",
            PaymentStatus = request.Status,
            Notes = request.Description
        };

        _db.TaxFilings.Add(filing);
        await _db.SaveChangesAsync(ct);

        return MapToFilingResponse(filing);
    }

    public async Task<TaxFilingResponse> UpdateFilingAsync(Guid id, UpdateTaxFilingRequest request, CancellationToken ct = default)
    {
        var filing = await _db.TaxFilings.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Tax filing not found.");

        if (request.ClientName != null) filing.ClientName = request.ClientName;
        if (request.TaxType != null) filing.TaxType = request.TaxType.Value.ToString();
        if (request.PeriodNumber != null) filing.PeriodNumber = request.PeriodNumber;
        if (request.PeriodType != null) filing.PeriodType = request.PeriodType.Value.ToString();
        if (request.PaymentStatus != null) filing.PaymentStatus = request.PaymentStatus;
        if (request.Notes != null) filing.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);
        return MapToFilingResponse(filing);
    }

    public async Task DeleteFilingAsync(Guid id, CancellationToken ct = default)
    {
        var filing = await _db.TaxFilings.FindAsync([id], ct);
        if (filing != null)
        {
            filing.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static TaxFilingResponse MapToFilingResponse(TaxFiling f) => new()
    {
        Id = f.Id,
        ClientName = f.ClientName,
        TaxType = Enum.TryParse<TaxType>(f.TaxType, out var tt) ? tt : TaxType.IIT,
        Period = f.FilingDate,
        PeriodNumber = f.PeriodNumber,
        PeriodType = Enum.TryParse<PeriodType>(f.PeriodType, out var pt) ? pt : null,
        Status = f.PaymentStatus ?? "Pending",
        Description = f.Notes,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };

    #endregion

    #region Tax Account Records

    public async Task<PaginatedResult<TaxAccountRecordResponse>> GetAccountRecordsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.TaxAccountRecords
            .Include(r => r.Branch)
            .Include(r => r.AssignedUser)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
        {
            query = query.Where(r => r.ClientName.Contains(@params.Search));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<TaxAccountRecordResponse>(
            items.Select(r => MapToRecordResponse(r)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<TaxAccountRecordResponse?> GetAccountRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.TaxAccountRecords
            .Include(r => r.Branch)
            .Include(r => r.AssignedUser)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        return record != null ? MapToRecordResponse(record) : null;
    }

    public async Task<TaxAccountRecordResponse> CreateAccountRecordAsync(CreateTaxAccountRecordRequest request, CancellationToken ct = default)
    {
        var record = new TaxAccountRecord
        {
            RecordCode = $"TAX-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            ClientName = request.ClientName,
            RecordDate = request.Date,
            BranchId = request.BranchId,
            Notes = request.Description,
            AssignedTo = request.AssignedToId,
            Process = request.Process ?? request.Status,
            ClientId = request.ClientId,
            PaymentStatus = request.PaymentStatus?.ToString(),
            SubTotal = request.ServiceFee,
            TotalPayment = request.TotalFee,
            PartialAmount = request.PaidAmount,
            PaymentOption = request.PaymentOption?.ToString()
        };

        _db.TaxAccountRecords.Add(record);
        await _db.SaveChangesAsync(ct);

        return MapToRecordResponse(record);
    }

    public async Task DeleteAccountRecordAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.TaxAccountRecords.FindAsync([id], ct);
        if (record != null)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<TaxAccountRecordResponse> UpdateAccountRecordAsync(Guid id, UpdateTaxAccountRecordRequest request, CancellationToken ct = default)
    {
        var record = await _db.TaxAccountRecords
            .Include(r => r.Branch)
            .Include(r => r.AssignedUser)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException("Tax account record not found.");

        if (request.ClientName != null) record.ClientName = request.ClientName;
        if (request.Date.HasValue) record.RecordDate = request.Date.Value;
        if (request.BranchId.HasValue) record.BranchId = request.BranchId;
        if (request.ClientId.HasValue) record.ClientId = request.ClientId;
        if (request.Process != null) record.Process = request.Process;
        if (request.Description != null) record.Notes = request.Description;
        if (request.AssignedToId.HasValue) record.AssignedTo = request.AssignedToId;
        if (request.PaymentStatus.HasValue) record.PaymentStatus = request.PaymentStatus.Value.ToString();
        if (request.PaymentOption.HasValue) record.PaymentOption = request.PaymentOption.Value.ToString();
        if (request.ServiceFee.HasValue) record.SubTotal = request.ServiceFee.Value;
        if (request.TotalFee.HasValue) record.TotalPayment = request.TotalFee.Value;
        if (request.PaidAmount.HasValue) record.PartialAmount = request.PaidAmount.Value;

        await _db.SaveChangesAsync(ct);
        return MapToRecordResponse(record);
    }

    public async Task<TaxAccountRecordResponse> UpdateAccountProcessAsync(Guid id, string process, CancellationToken ct = default)
    {
        var record = await _db.TaxAccountRecords
            .Include(r => r.Branch)
            .Include(r => r.AssignedUser)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException("Tax account record not found.");

        record.Process = process;
        await _db.SaveChangesAsync(ct);
        return MapToRecordResponse(record);
    }

    public async Task<TaxAccountRecordResponse> AssignStaffAsync(Guid id, Guid assignedToId, CancellationToken ct = default)
    {
        var record = await _db.TaxAccountRecords
            .Include(r => r.Branch)
            .Include(r => r.AssignedUser)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException("Tax account record not found.");

        record.AssignedTo = assignedToId;
        await _db.SaveChangesAsync(ct);
        return MapToRecordResponse(record);
    }

    private static TaxAccountRecordResponse MapToRecordResponse(TaxAccountRecord r) => new()
    {
        Id = r.Id,
        ClientName = r.ClientName,
        Date = r.RecordDate,
        BranchId = r.BranchId,
        BranchName = r.Branch?.Name,
        Description = r.Notes,
        AssignedToId = r.AssignedTo,
        AssignedToName = r.AssignedUser?.Username,
        Process = r.Process,
        ClientId = r.ClientId,
        PaymentStatus = Enum.TryParse<PaymentStatus>(r.PaymentStatus, out var ps) ? ps : null,
        ServiceFee = r.SubTotal,
        TotalFee = r.TotalPayment,
        PaidAmount = r.PartialAmount,
        PaymentOption = Enum.TryParse<PaymentOption>(r.PaymentOption, out var po) ? po : null,
        Status = r.Process,
        CreatedAt = r.CreatedAt
    };

    #endregion
}
