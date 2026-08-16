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
        private readonly IEmailService _emailService;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        public AuthController(
            IAuthService authService, 
            AATS.Infrastructure.Persistence.ApplicationDbContext context,
            IEmailService emailService)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
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

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                            <h2 style='color: #0066cc; border-bottom: 2px solid #0066cc; padding-bottom: 10px;'>AATS - Password Reset Request</h2>
                            <p>A user has requested their forgot password details.</p>
                            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Username</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.Username}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Email</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.Email}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Phone</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.Phone}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Role</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.Role}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Branch</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.Branch}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Last Remembered Password</td>
                                    <td style='padding: 8px; border: 1px solid #ddd;'>{request.LastRememberedPassword}</td>
                                </tr>
                            </table>
                            <p style='margin-top: 20px; font-size: 12px; color: #777;'>This request was generated automatically by the AATS Desktop application.</p>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync("nexora280@gmail.com", "AATS - Password Reset Request", body);

                return Ok(ApiResponse<object>.Ok(new { message = "Password reset request submitted successfully." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure("FORGOT_PASSWORD_ERROR", ex.Message));
            }
        }

        [HttpPost("request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
        {
            try
            {
                var random = new Random();
                var otp = random.Next(100000, 999999).ToString();
                var expiry = DateTime.UtcNow.AddMinutes(5);

                _otpStore[dto.Username] = (otp, expiry);

                var admins = await _context.Users
                    .Where(u => u.Role == AATS.Domain.Entities.UserRole.Admin && u.IsActive)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    var body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                                <h2 style='color: #d32f2f; border-bottom: 2px solid #d32f2f; padding-bottom: 10px;'>AATS - Record Edit Authorization OTP</h2>
                                <p>A staff member has requested authorization to edit a record in the system.</p>
                                <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Requesting Staff</td>
                                        <td style='padding: 8px; border: 1px solid #ddd;'>{dto.Username}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>OTP Code</td>
                                        <td style='padding: 8px; border: 1px solid #ddd; font-size: 18px; font-weight: bold; color: #d32f2f;'>{otp}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold; background-color: #f9f9f9;'>Expiry</td>
                                        <td style='padding: 8px; border: 1px solid #ddd;'>5 Minutes</td>
                                    </tr>
                                </table>
                                <p style='margin-top: 20px;'>Please provide this code to the requesting staff member if you authorize their edit request.</p>
                                <p style='margin-top: 20px; font-size: 12px; color: #777;'>This request was generated automatically by the AATS Desktop application.</p>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(admin.Email, "AATS - Edit Authorization OTP", body);
                }

                if (!admins.Any())
                {
                    Console.WriteLine($"[WARNING] No active Admin users found in the database. OTP generated: {otp}");
                }

                return Ok(ApiResponse<object>.Ok(new { otp = otp, message = "OTP generated and sent to administrators." }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure("OTP_REQUEST_ERROR", ex.Message));
            }
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (_otpStore.TryGetValue(dto.Username, out var storedInfo))
            {
                if (storedInfo.Otp == dto.Otp && DateTime.UtcNow <= storedInfo.Expiry)
                {
                    _otpStore.TryRemove(dto.Username, out _);
                    return Ok(ApiResponse<object>.Ok(new { message = "OTP verified successfully." }));
                }
            }
            return BadRequest(ApiResponse<object>.Failure("INVALID_OTP", "Invalid or expired OTP code."));
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RequestOtpDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class VerifyOtpDto
    {
        public string Username { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
