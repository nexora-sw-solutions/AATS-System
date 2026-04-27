using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Tax;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TaxController : ControllerBase
{
    private readonly ITaxService _taxService;

    public TaxController(ITaxService taxService)
    {
        _taxService = taxService;
    }

    #region Tax Filings

    [HttpGet("filings")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TaxFilingResponse>>>> GetFilings([FromQuery] PaginationParams @params)
    {
        var result = await _taxService.GetFilingsAsync(@params);
        return Ok(ApiResponse<PaginatedResult<TaxFilingResponse>>.Ok(result));
    }

    [HttpGet("filings/{id}")]
    public async Task<ActionResult<ApiResponse<TaxFilingResponse>>> GetFiling(Guid id)
    {
        var filing = await _taxService.GetFilingByIdAsync(id);
        if (filing == null) return NotFound(ApiResponse<TaxFilingResponse>.Fail("Tax filing not found."));
        return Ok(ApiResponse<TaxFilingResponse>.Ok(filing));
    }

    [HttpPost("filings")]
    public async Task<ActionResult<ApiResponse<TaxFilingResponse>>> CreateFiling(CreateTaxFilingRequest request)
    {
        var filing = await _taxService.CreateFilingAsync(request);
        return CreatedAtAction(nameof(GetFiling), new { id = filing.Id }, ApiResponse<TaxFilingResponse>.Ok(filing));
    }

    [HttpPut("filings/{id}")]
    public async Task<ActionResult<ApiResponse<TaxFilingResponse>>> UpdateFiling(Guid id, UpdateTaxFilingRequest request)
    {
        var filing = await _taxService.UpdateFilingAsync(id, request);
        return Ok(ApiResponse<TaxFilingResponse>.Ok(filing));
    }

    [HttpDelete("filings/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFiling(Guid id)
    {
        await _taxService.DeleteFilingAsync(id);
        return Ok(ApiResponse<object>.Ok(null));
    }

    #endregion

    #region Tax Account Records

    [HttpGet("records")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TaxAccountRecordResponse>>>> GetAccountRecords([FromQuery] PaginationParams @params)
    {
        var result = await _taxService.GetAccountRecordsAsync(@params);
        return Ok(ApiResponse<PaginatedResult<TaxAccountRecordResponse>>.Ok(result));
    }

    [HttpGet("records/{id}")]
    public async Task<ActionResult<ApiResponse<TaxAccountRecordResponse>>> GetAccountRecord(Guid id)
    {
        var record = await _taxService.GetAccountRecordByIdAsync(id);
        if (record == null) return NotFound(ApiResponse<TaxAccountRecordResponse>.Fail("Tax account record not found."));
        return Ok(ApiResponse<TaxAccountRecordResponse>.Ok(record));
    }

    [HttpPost("records")]
    public async Task<ActionResult<ApiResponse<TaxAccountRecordResponse>>> CreateAccountRecord(CreateTaxAccountRecordRequest request)
    {
        var record = await _taxService.CreateAccountRecordAsync(request);
        return CreatedAtAction(nameof(GetAccountRecord), new { id = record.Id }, ApiResponse<TaxAccountRecordResponse>.Ok(record));
    }

    [HttpDelete("records/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAccountRecord(Guid id)
    {
        await _taxService.DeleteAccountRecordAsync(id);
        return Ok(ApiResponse<object>.Ok(null));
    }

    [HttpPut("records/{id}")]
    public async Task<ActionResult<ApiResponse<TaxAccountRecordResponse>>> UpdateAccountRecord(Guid id, UpdateTaxAccountRecordRequest request)
    {
        var record = await _taxService.UpdateAccountRecordAsync(id, request);
        return Ok(ApiResponse<TaxAccountRecordResponse>.Ok(record));
    }

    [HttpPatch("records/{id}/process")]
    public async Task<ActionResult<ApiResponse<TaxAccountRecordResponse>>> UpdateAccountProcess(Guid id, [FromBody] UpdateProcessRequest request)
    {
        var record = await _taxService.UpdateAccountProcessAsync(id, request.Process);
        return Ok(ApiResponse<TaxAccountRecordResponse>.Ok(record));
    }

    [HttpPatch("records/{id}/assign")]
    public async Task<ActionResult<ApiResponse<TaxAccountRecordResponse>>> AssignStaff(Guid id, [FromBody] AssignStaffRequest request)
    {
        var record = await _taxService.AssignStaffAsync(id, request.AssignedToId);
        return Ok(ApiResponse<TaxAccountRecordResponse>.Ok(record));
    }

    #endregion
}

public class AssignStaffRequest
{
    public Guid AssignedToId { get; set; }
}
