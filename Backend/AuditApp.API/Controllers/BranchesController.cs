using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Branches;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[ApiController]
[Route("api/v1/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var branches = await _branchService.GetAllAsync(ct);
        return Ok(ApiResponse<List<BranchResponse>>.Ok(branches));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var branch = await _branchService.GetByIdAsync(id, ct);
        return branch != null
            ? Ok(ApiResponse<BranchResponse>.Ok(branch))
            : NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Branch not found."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var branch = await _branchService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, ApiResponse<BranchResponse>.Ok(branch));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        var branch = await _branchService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<BranchResponse>.Ok(branch));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var branch = await _branchService.ToggleStatusAsync(id, request.IsActive, ct);
        return Ok(ApiResponse<BranchResponse>.Ok(branch));
    }
}

// Reuse UpdateStatusRequest from Users DTO
public class UpdateStatusRequest
{
    public bool IsActive { get; set; }
}
