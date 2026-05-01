using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Clients;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams p, CancellationToken ct)
    {
        var result = await _clientService.GetAllAsync(p, ct);
        return Ok(ApiResponse<PaginatedResult<ClientResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var client = await _clientService.GetByIdAsync(id, ct);
        return client != null
            ? Ok(ApiResponse<ClientResponse>.Ok(client))
            : NotFound(ApiResponse<object>.Fail("Client not found."));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken ct)
    {
        var client = await _clientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, ApiResponse<ClientResponse>.Ok(client));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct)
    {
        var client = await _clientService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ClientResponse>.Ok(client));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _clientService.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Client deleted successfully." }));
    }

    [HttpPost("{id:guid}/logo")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile file, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        var key = await _clientService.UploadLogoAsync(id, stream, file.FileName, file.ContentType, ct);
        return Ok(ApiResponse<object>.Ok(new { storageKey = key }));
    }

    [HttpDelete("{id:guid}/logo")]
    public async Task<IActionResult> DeleteLogo(Guid id, CancellationToken ct)
    {
        await _clientService.DeleteLogoAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Logo removed." }));
    }

    [HttpGet("{id:guid}/revenue-summary")]
    public async Task<IActionResult> GetRevenueSummary(Guid id, CancellationToken ct)
    {
        var summary = await _clientService.GetRevenueSummaryAsync(id, ct);
        return Ok(ApiResponse<ClientRevenueSummary>.Ok(summary));
    }
}
