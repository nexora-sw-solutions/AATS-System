using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Nexora;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuditApp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class NexoraController : ControllerBase
{
    private readonly INexoraService _nexoraService;
    private readonly ILogger<NexoraController> _logger;

    public NexoraController(INexoraService nexoraService, ILogger<NexoraController> logger)
    {
        _nexoraService = nexoraService;
        _logger = logger;
    }

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NexoraServiceRequestResponse>>>> GetRequests([FromQuery] PaginationParams @params)
    {
        var result = await _nexoraService.GetRequestsAsync(@params);
        return Ok(ApiResponse<PaginatedResult<NexoraServiceRequestResponse>>.Ok(result));
    }

    [HttpGet("requests/{id}")]
    public async Task<ActionResult<ApiResponse<NexoraServiceRequestResponse>>> GetRequestById(Guid id)
    {
        var result = await _nexoraService.GetRequestByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<NexoraServiceRequestResponse>.Fail("Request not found"));
        return Ok(ApiResponse<NexoraServiceRequestResponse>.Ok(result));
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<NexoraServiceRequestResponse>>> CreateRequest([FromBody] CreateNexoraRequestDto request)
    {
        _logger.LogInformation("Creating Nexora request for {FirstName} {LastName}", request.ClientFirstName, request.ClientLastName);
        try
        {
            var result = await _nexoraService.CreateRequestAsync(request);
            return Ok(ApiResponse<NexoraServiceRequestResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Nexora request");
            return StatusCode(500, ApiResponse<NexoraServiceRequestResponse>.Fail($"Internal error: {ex.Message} - {ex.InnerException?.Message} | STACK: {ex.StackTrace}"));
        }
    }

    [HttpPut("requests/{id}")]
    public async Task<ActionResult<ApiResponse<NexoraServiceRequestResponse>>> UpdateRequest(Guid id, [FromBody] UpdateNexoraRequestDto request)
    {
        var result = await _nexoraService.UpdateRequestAsync(id, request);
        return Ok(ApiResponse<NexoraServiceRequestResponse>.Ok(result));
    }

    [HttpDelete("requests/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRequest(Guid id)
    {
        await _nexoraService.DeleteRequestAsync(id);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<List<NexoraServiceResponse>>>> GetServices()
    {
        var result = await _nexoraService.GetServicesAsync();
        return Ok(ApiResponse<List<NexoraServiceResponse>>.Ok(result));
    }
}
