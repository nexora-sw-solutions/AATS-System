using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Users;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using AuditApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageService _storage;

    public UserService(IApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<PaginatedResult<UserResponse>> GetAllAsync(PaginationParams p, string? role, bool? isActive, CancellationToken ct = default)
    {
        var query = _db.Users.Include(u => u.Branch).AsQueryable();

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role.ToString() == role);
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);
        if (p.BranchId.HasValue)
            query = query.Where(u => u.BranchId == p.BranchId.Value);
        if (!string.IsNullOrEmpty(p.Search))
            query = query.Where(u => u.Username.Contains(p.Search) || u.Email.Contains(p.Search));

        query = p.Order.ToLower() == "asc"
            ? query.OrderBy(u => u.CreatedAt)
            : query.OrderByDescending(u => u.CreatedAt);

        return await PaginatedResult<UserResponse>.CreateAsync(
            query.Select(u => MapToResponse(u)), p.Page, p.Limit, ct);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Branch).FirstOrDefaultAsync(u => u.Id == id, ct);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Enum.Parse<UserRole>(request.Role),
            BranchId = request.BranchId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(user.Id, ct) ?? throw new Exception("Failed to create user.");
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.Username != null) user.Username = request.Username;
        if (request.Email != null) user.Email = request.Email;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.Role != null) user.Role = Enum.Parse<UserRole>(request.Role);
        if (request.BranchId != null) user.BranchId = request.BranchId.Value;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to update user.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");
        user.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<UserResponse> ToggleStatusAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to update user status.");
    }

    public async Task<string> UploadLogoAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.UserLogo))
            await _storage.DeleteAsync(user.UserLogo, ct);

        var key = $"users/{id}/logo/{fileName}";
        await _storage.UploadAsync(key, fileStream, contentType, ct);
        user.UserLogo = key;
        await _db.SaveChangesAsync(ct);
        return key;
    }

    public async Task DeleteLogoAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.UserLogo))
        {
            await _storage.DeleteAsync(user.UserLogo, ct);
            user.UserLogo = null;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        Phone = u.Phone,
        UserLogo = u.UserLogo,
        Role = u.Role.ToString(),
        BranchId = u.BranchId,
        BranchName = u.Branch?.Name,
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
