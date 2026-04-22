using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Auth;
using AuditApp.Application.DTOs.Users;
using AuditApp.Application.DTOs.Branches;
using AuditApp.Application.DTOs.Clients;

namespace AuditApp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> GetAllAsync(PaginationParams p, string? role, bool? isActive, CancellationToken ct = default);
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse> ToggleStatusAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<string> UploadLogoAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteLogoAsync(Guid id, CancellationToken ct = default);
}

public interface IBranchService
{
    Task<List<BranchResponse>> GetAllAsync(CancellationToken ct = default);
    Task<BranchResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<BranchResponse> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default);
    Task<BranchResponse> ToggleStatusAsync(Guid id, bool isActive, CancellationToken ct = default);
}

public interface IClientService
{
    Task<PaginatedResult<ClientResponse>> GetAllAsync(PaginationParams p, CancellationToken ct = default);
    Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadLogoAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteLogoAsync(Guid id, CancellationToken ct = default);
    Task<ClientRevenueSummary> GetRevenueSummaryAsync(Guid id, CancellationToken ct = default);
}
