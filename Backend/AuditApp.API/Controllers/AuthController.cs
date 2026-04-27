using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Auth;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuditApp.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPatch("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.ChangePasswordAsync(userId, request, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Password changed successfully." }));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT is stateless — client should discard the token
        return Ok(ApiResponse<object>.Ok(new { message = "Logged out successfully." }));
    }
}
