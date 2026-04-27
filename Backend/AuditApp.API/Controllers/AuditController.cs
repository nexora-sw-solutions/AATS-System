using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Audit;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    // ── Audit & Assurance ──────────────────────────────────────────

    [HttpGet("assurance")]
    public async Task<IActionResult> GetAssurance([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.AuditAssuranceRecord>(p)));

    [HttpGet("assurance/{id}")]
    public async Task<IActionResult> GetAssuranceById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.AuditAssuranceRecord>(id));

    [HttpPost("assurance")]
    public async Task<IActionResult> CreateAssurance(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.AuditAssuranceRecord>(req)));

    [HttpPut("assurance/{id}")]
    public async Task<IActionResult> UpdateAssurance(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.AuditAssuranceRecord>(id, req)));

    [HttpDelete("assurance/{id}")]
    public async Task<IActionResult> DeleteAssurance(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.AuditAssuranceRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("assurance/{id}/process")]
    public async Task<IActionResult> UpdateAssuranceProcess(Guid id, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateProcessAsync<Domain.Entities.AuditAssuranceRecord>(id, req.Process)));

    [HttpPatch("assurance/{id}/payment")]
    public async Task<IActionResult> UpdateAssurancePayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.AuditAssuranceRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Forensic Audit ─────────────────────────────────────────────

    [HttpGet("forensic")]
    public async Task<IActionResult> GetForensic([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.ForensicAuditRecord>(p)));

    [HttpGet("forensic/{id}")]
    public async Task<IActionResult> GetForensicById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.ForensicAuditRecord>(id));

    [HttpPost("forensic")]
    public async Task<IActionResult> CreateForensic(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.ForensicAuditRecord>(req)));

    [HttpPut("forensic/{id}")]
    public async Task<IActionResult> UpdateForensic(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.ForensicAuditRecord>(id, req)));

    [HttpDelete("forensic/{id}")]
    public async Task<IActionResult> DeleteForensic(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.ForensicAuditRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("forensic/{id}/process")]
    public async Task<IActionResult> UpdateForensicProcess(Guid id, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateProcessAsync<Domain.Entities.ForensicAuditRecord>(id, req.Process)));

    [HttpPatch("forensic/{id}/payment")]
    public async Task<IActionResult> UpdateForensicPayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.ForensicAuditRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Internal Audit ─────────────────────────────────────────────

    [HttpGet("internal")]
    public async Task<IActionResult> GetInternal([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.InternalAuditRecord>(p)));

    [HttpGet("internal/{id}")]
    public async Task<IActionResult> GetInternalById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.InternalAuditRecord>(id));

    [HttpPost("internal")]
    public async Task<IActionResult> CreateInternal(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.InternalAuditRecord>(req)));

    [HttpPut("internal/{id}")]
    public async Task<IActionResult> UpdateInternal(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.InternalAuditRecord>(id, req)));

    [HttpDelete("internal/{id}")]
    public async Task<IActionResult> DeleteInternal(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.InternalAuditRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("internal/{id}/process")]
    public async Task<IActionResult> UpdateInternalProcess(Guid id, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateProcessAsync<Domain.Entities.InternalAuditRecord>(id, req.Process)));

    [HttpPatch("internal/{id}/payment")]
    public async Task<IActionResult> UpdateInternalPayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.InternalAuditRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Management Accounts ────────────────────────────────────────

    [HttpGet("management-accounts")]
    public async Task<IActionResult> GetManagementAccounts([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.ManagementAccountRecord>(p)));

    [HttpGet("management-accounts/{id}")]
    public async Task<IActionResult> GetManagementAccountById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.ManagementAccountRecord>(id));

    [HttpPost("management-accounts")]
    public async Task<IActionResult> CreateManagementAccount(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.ManagementAccountRecord>(req)));

    [HttpPut("management-accounts/{id}")]
    public async Task<IActionResult> UpdateManagementAccount(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.ManagementAccountRecord>(id, req)));

    [HttpDelete("management-accounts/{id}")]
    public async Task<IActionResult> DeleteManagementAccount(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.ManagementAccountRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("management-accounts/{id}/process")]
    public async Task<IActionResult> UpdateManagementAccountProcess(Guid id, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateProcessAsync<Domain.Entities.ManagementAccountRecord>(id, req.Process)));

    [HttpPatch("management-accounts/{id}/payment")]
    public async Task<IActionResult> UpdateManagementAccountPayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.ManagementAccountRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Internal Control ───────────────────────────────────────────

    [HttpGet("internal-control")]
    public async Task<IActionResult> GetInternalControl([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.InternalControlRecord>(p)));

    [HttpGet("internal-control/{id}")]
    public async Task<IActionResult> GetInternalControlById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.InternalControlRecord>(id));

    [HttpPost("internal-control")]
    public async Task<IActionResult> CreateInternalControl(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.InternalControlRecord>(req)));

    [HttpPut("internal-control/{id}")]
    public async Task<IActionResult> UpdateInternalControl(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.InternalControlRecord>(id, req)));

    [HttpDelete("internal-control/{id}")]
    public async Task<IActionResult> DeleteInternalControl(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.InternalControlRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("internal-control/{id}/process")]
    public async Task<IActionResult> UpdateInternalControlProcess(Guid id, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateProcessAsync<Domain.Entities.InternalControlRecord>(id, req.Process)));

    [HttpPatch("internal-control/{id}/payment")]
    public async Task<IActionResult> UpdateInternalControlPayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.InternalControlRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Others ─────────────────────────────────────────────────────

    [HttpGet("others")]
    public async Task<IActionResult> GetOthers([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<AuditRecordResponse>>.Ok(await _auditService.GetRecordsAsync<Domain.Entities.OtherAuditRecord>(p)));

    [HttpGet("others/{id}")]
    public async Task<IActionResult> GetOthersById(Guid id)
        => OkOrNotFound(await _auditService.GetRecordByIdAsync<Domain.Entities.OtherAuditRecord>(id));

    [HttpPost("others")]
    public async Task<IActionResult> CreateOthers(CreateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.CreateRecordAsync<Domain.Entities.OtherAuditRecord>(req)));

    [HttpPut("others/{id}")]
    public async Task<IActionResult> UpdateOthers(Guid id, UpdateAuditRecordRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdateRecordAsync<Domain.Entities.OtherAuditRecord>(id, req)));

    [HttpDelete("others/{id}")]
    public async Task<IActionResult> DeleteOthers(Guid id)
    { await _auditService.DeleteRecordAsync<Domain.Entities.OtherAuditRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("others/{id}/payment")]
    public async Task<IActionResult> UpdateOthersPayment(Guid id, [FromBody] UpdatePaymentDetailsRequest req)
        => Ok(ApiResponse<AuditRecordResponse>.Ok(await _auditService.UpdatePaymentAsync<Domain.Entities.OtherAuditRecord>(id, req.PaymentStatus, req.PaymentOption, req.SubTotal, req.TotalPayment, req.PartialAmount)));

    // ── Shared helpers ─────────────────────────────────────────────

    private IActionResult OkOrNotFound(AuditRecordResponse? record)
        => record != null
            ? Ok(ApiResponse<AuditRecordResponse>.Ok(record))
            : NotFound(ApiResponse<AuditRecordResponse>.Fail("Record not found."));
}

// ── Shared DTOs for PATCH endpoints ────────────────────────────

public class UpdateProcessRequest
{
    public string Process { get; set; } = string.Empty;
}

public class UpdatePaymentDetailsRequest
{
    public string? PaymentStatus { get; set; }
    public string? PaymentOption { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TotalPayment { get; set; }
    public decimal? PartialAmount { get; set; }
}
