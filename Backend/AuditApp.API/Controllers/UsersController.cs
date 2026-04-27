using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Users;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams p, [FromQuery] string? role, [FromQuery] bool? is_active, CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(p, role, is_active, ct);
        return Ok(ApiResponse<PaginatedResult<UserResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetByIdAsync(id, ct);
        return user != null
            ? Ok(ApiResponse<UserResponse>.Ok(user))
            : NotFound(ApiResponse<object>.Fail("User not found."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await _userService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserResponse>.Ok(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await _userService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<UserResponse>.Ok(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _userService.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "User deleted successfully." }));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var user = await _userService.ToggleStatusAsync(id, request.IsActive, ct);
        return Ok(ApiResponse<UserResponse>.Ok(user));
    }

    [HttpPost("{id:guid}/logo")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        var key = await _userService.UploadLogoAsync(id, stream, file.FileName, file.ContentType, ct);
        return Ok(ApiResponse<object>.Ok(new { storageKey = key }));
    }

    [HttpDelete("{id:guid}/logo")]
    public async Task<IActionResult> DeleteLogo(Guid id, CancellationToken ct)
    {
        await _userService.DeleteLogoAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Logo removed." }));
    }
}
