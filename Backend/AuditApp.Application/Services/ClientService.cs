using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Clients;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using AuditApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class ClientService : IClientService
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ClientService(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<PaginatedResult<ClientResponse>> GetAllAsync(PaginationParams p, CancellationToken ct = default)
    {
        var query = _db.Clients.Include(c => c.Branch).Where(c => !c.IsDeleted).AsQueryable();

        if (p.BranchId.HasValue)
            query = query.Where(c => c.BranchId == p.BranchId.Value);
        if (!string.IsNullOrEmpty(p.PaymentStatus))
            query = query.Where(c => c.Status.ToString() == p.PaymentStatus);
        if (!string.IsNullOrEmpty(p.Search))
            query = query.Where(c => c.ClientName.Contains(p.Search) || c.ClientCode.Contains(p.Search));

        query = p.Order.ToLower() == "asc"
            ? query.OrderBy(c => c.ClientCode)
            : query.OrderByDescending(c => c.ClientCode);

        return await PaginatedResult<ClientResponse>.CreateAsync(
            query.Select(c => MapToResponse(c)), p.Page, p.Limit, ct);
    }

    public async Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Clients.Include(c => c.Branch).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        return client == null ? null : MapToResponse(client);
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        var lastClient = await _db.Clients
            .Where(c => c.ClientCode.StartsWith("CL-"))
            .OrderByDescending(c => c.ClientCode)
            .FirstOrDefaultAsync(ct);

        int nextId = 1;
        if (lastClient != null && int.TryParse(lastClient.ClientCode.Substring(3), out int lastNum))
        {
            nextId = lastNum + 1;
        }

        var client = new Client
        {
            ClientCode = $"CL-{nextId:D5}",
            ClientName = request.ClientName,
            Email = request.Email,
            Phone = request.Phone,
            Status = Enum.Parse<ClientStatus>(request.Status),
            BranchId = request.BranchId
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(client.Id, ct) ?? throw new Exception("Failed to create client.");
    }

    public async Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Client not found.");

        if (request.ClientName != null) client.ClientName = request.ClientName;
        if (request.Email != null) client.Email = request.Email;
        if (request.Phone != null) client.Phone = request.Phone;
        if (request.Status != null) client.Status = Enum.Parse<ClientStatus>(request.Status);
        if (request.BranchId != null) client.BranchId = request.BranchId;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to update client.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Client not found.");
        client.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<string> UploadLogoAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Client not found.");

        if (!string.IsNullOrEmpty(client.LogoStorageKey))
            await _storage.DeleteAsync(client.LogoStorageKey, ct);

        var key = $"clients/{id}/logo/{fileName}";
        await _storage.UploadAsync(key, fileStream, contentType, ct);
        client.LogoStorageKey = key;
        await _db.SaveChangesAsync(ct);
        return key;
    }

    public async Task DeleteLogoAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Client not found.");

        if (!string.IsNullOrEmpty(client.LogoStorageKey))
        {
            await _storage.DeleteAsync(client.LogoStorageKey, ct);
            client.LogoStorageKey = null;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<ClientRevenueSummary> GetRevenueSummaryAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Clients.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Client not found.");

        return new ClientRevenueSummary
        {
            TotalRevenue = client.TotalRevenue,
            OutstandingBalance = client.OutstandingBalance,
            TotalRecords = 0 // Will be calculated across modules in Phase 4
        };
    }

    private static ClientResponse MapToResponse(Client c) => new()
    {
        Id = c.Id,
        ClientCode = c.ClientCode,
        ClientName = c.ClientName,
        Email = c.Email,
        Phone = c.Phone,
        Status = c.Status.ToString(),
        BranchId = c.BranchId,
        BranchName = c.Branch?.Name,
        TotalRevenue = c.TotalRevenue,
        OutstandingBalance = c.OutstandingBalance,
        LogoStorageKey = c.LogoStorageKey,
        LastActiveAt = c.LastActiveAt,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
