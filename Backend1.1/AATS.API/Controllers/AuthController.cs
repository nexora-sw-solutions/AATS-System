using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AATS.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AATS.Infrastructure.Persistence.ApplicationDbContext _context;

        public AuthController(IAuthService authService, AATS.Infrastructure.Persistence.ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto.Email, loginDto.Password);
                return Ok(ApiResponse<LoginResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                return Unauthorized(ApiResponse<object>.Failure("AUTH_ERROR", ex.Message));
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(ApiResponse<object>.Ok(new { message = "User registered successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure("REGISTRATION_ERROR", ex.Message));
            }
        }

        [HttpGet("test-users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(u => new { u.Email, u.Username, u.Role }).ToListAsync();
            return Ok(users);
        }
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out var userId)) return BadRequest();

            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var response = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Phone,
                Role = user.Role.ToString(),
                Branch = user.Branch?.Name ?? "N/A"
            };

            return Ok(ApiResponse<object>.Ok(response));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] AATS.Application.Common.Interfaces.UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)) 
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Validate current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(ApiResponse<object>.Failure("AUTH_ERROR", "Current password is incorrect."));
            }

            // Update basic info
            user.Username = request.Username;
            user.Email = request.Email;
            user.Phone = request.Phone;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { message = "Profile updated successfully." }));
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
