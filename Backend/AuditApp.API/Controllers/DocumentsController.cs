using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{referenceType}/{referenceId}")]
    public async Task<ActionResult<ApiResponse<List<DocumentResponse>>>> GetDocuments(string referenceType, Guid referenceId)
        => Ok(ApiResponse<List<DocumentResponse>>.Ok(await _documentService.GetDocumentsByReferenceAsync(referenceId, referenceType)));

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<DocumentResponse>>> UploadDocument([FromForm] UploadDocumentRequest request, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse<DocumentResponse>.Fail("No file uploaded."));

        using var stream = file.OpenReadStream();
        var result = await _documentService.UploadDocumentAsync(request, stream, file.ContentType);
        return Ok(ApiResponse<DocumentResponse>.Ok(result));
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult<ApiResponse<string>>> DownloadDocument(Guid id)
    {
        var url = await _documentService.GetDocumentDownloadUrlAsync(id);
        return Ok(ApiResponse<string>.Ok(url));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteDocument(Guid id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return Ok(ApiResponse<object?>.Ok(null));
    }
}
