using AuditApp.Application.DTOs.Branches;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class BranchService : IBranchService
{
    private readonly IApplicationDbContext _db;

    public BranchService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<BranchResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Branches
            .OrderBy(b => b.Name)
            .Select(b => MapToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<BranchResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _db.Branches.FindAsync([id], ct);
        return branch == null ? null : MapToResponse(branch);
    }

    public async Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        var branch = new Branch
        {
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            Phone = request.Phone
        };

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(branch);
    }

    public async Task<BranchResponse> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        var branch = await _db.Branches.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Branch not found.");

        if (request.Name != null) branch.Name = request.Name;
        if (request.Code != null) branch.Code = request.Code;
        if (request.Address != null) branch.Address = request.Address;
        if (request.Phone != null) branch.Phone = request.Phone;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(branch);
    }

    public async Task<BranchResponse> ToggleStatusAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var branch = await _db.Branches.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Branch not found.");
        branch.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return MapToResponse(branch);
    }

    private static BranchResponse MapToResponse(Branch b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Code = b.Code,
        Address = b.Address,
        Phone = b.Phone,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
