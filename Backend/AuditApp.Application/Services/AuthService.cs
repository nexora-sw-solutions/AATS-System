using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuditApp.Application.DTOs.Auth;
using AuditApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuditApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(IApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled. Contact an administrator.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var token = GenerateJwt(user);
        var refreshToken = GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                UserLogo = user.UserLogo
            }
        };
    }

    public Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // In a production system, validate the refresh token against a stored token table
        throw new NotImplementedException("Refresh token validation requires a token store — to be implemented.");
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([userId], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync(ct);
    }

    private string GenerateJwt(Domain.Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? "ThisIsADefaultDevKeyThatShouldBeChangedInProduction2026!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("branch_id", user.BranchId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "AuditApp",
            audience: _config["Jwt:Audience"] ?? "AuditApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
