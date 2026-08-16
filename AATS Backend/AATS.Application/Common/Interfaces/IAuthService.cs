using System;
using System.Threading.Tasks;

namespace AATS.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string identifier, string password);
        Task RegisterAsync(RegisterRequest request);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateJwtToken(Guid userId, string username, string role);
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string? Phone { get; set; }
        public int Role { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }
}
