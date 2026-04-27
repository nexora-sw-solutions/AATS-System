using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Secretarial;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SecretarialController : ControllerBase
{
    private readonly ISecretarialService _secretarialService;

    public SecretarialController(ISecretarialService secretarialService)
    {
        _secretarialService = secretarialService;
    }

    // ── Company Registrations ──────────────────────────────────────

    [HttpGet("company-registrations")]
    public async Task<IActionResult> GetCompanyRegistrations([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<CompanyRegistrationResponse>>.Ok(await _secretarialService.GetCompanyRegistrationsAsync(p)));

    [HttpGet("company-registrations/{id}")]
    public async Task<IActionResult> GetCompanyRegistration(Guid id)
    {
        var r = await _secretarialService.GetCompanyRegistrationByIdAsync(id);
        return r != null ? Ok(ApiResponse<CompanyRegistrationResponse>.Ok(r)) : NotFound(ApiResponse<CompanyRegistrationResponse>.Fail("Not found."));
    }

    [HttpPost("company-registrations")]
    public async Task<IActionResult> CreateCompanyRegistration(CreateCompanyRegistrationRequest req)
    {
        var r = await _secretarialService.CreateCompanyRegistrationAsync(req);
        return CreatedAtAction(nameof(GetCompanyRegistration), new { id = r.Id }, ApiResponse<CompanyRegistrationResponse>.Ok(r));
    }

    [HttpPut("company-registrations/{id}")]
    public async Task<IActionResult> UpdateCompanyRegistration(Guid id, UpdateCompanyRegistrationRequest req)
        => Ok(ApiResponse<CompanyRegistrationResponse>.Ok(await _secretarialService.UpdateCompanyRegistrationAsync(id, req)));

    [HttpDelete("company-registrations/{id}")]
    public async Task<IActionResult> DeleteCompanyRegistration(Guid id)
    { await _secretarialService.DeleteCompanyRegistrationAsync(id); return Ok(ApiResponse<object>.Ok(null)); }

    // ── Company Officers (nested) ──────────────────────────────────

    [HttpGet("company-registrations/{id}/officers")]
    public async Task<IActionResult> GetOfficers(Guid id)
        => Ok(ApiResponse<List<CompanyOfficerResponse>>.Ok(await _secretarialService.GetOfficersAsync(id)));

    [HttpPost("company-registrations/{id}/officers")]
    public async Task<IActionResult> AddOfficer(Guid id, CreateCompanyOfficerRequest req)
        => Ok(ApiResponse<CompanyOfficerResponse>.Ok(await _secretarialService.AddOfficerAsync(id, req)));

    [HttpPut("company-registrations/{id}/officers/{officerId}")]
    public async Task<IActionResult> UpdateOfficer(Guid id, Guid officerId, UpdateCompanyOfficerRequest req)
        => Ok(ApiResponse<CompanyOfficerResponse>.Ok(await _secretarialService.UpdateOfficerAsync(id, officerId, req)));

    [HttpDelete("company-registrations/{id}/officers/{officerId}")]
    public async Task<IActionResult> DeleteOfficer(Guid id, Guid officerId)
    { await _secretarialService.DeleteOfficerAsync(id, officerId); return Ok(ApiResponse<object>.Ok(null)); }

    // ── EPF/ETF ────────────────────────────────────────────────────

    [HttpGet("epf-etf")]
    public async Task<IActionResult> GetEpfEtfRecords([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<EpfEtfRecordResponse>>.Ok(await _secretarialService.GetEpfEtfRecordsAsync(p)));

    [HttpGet("epf-etf/{id}")]
    public async Task<IActionResult> GetEpfEtfRecord(Guid id)
    {
        var r = await _secretarialService.GetEpfEtfRecordByIdAsync(id);
        return r != null ? Ok(ApiResponse<EpfEtfRecordResponse>.Ok(r)) : NotFound(ApiResponse<EpfEtfRecordResponse>.Fail("Not found."));
    }

    [HttpPost("epf-etf")]
    public async Task<IActionResult> CreateEpfEtfRecord(CreateEpfEtfRecordRequest req)
    {
        var r = await _secretarialService.CreateEpfEtfRecordAsync(req);
        return CreatedAtAction(nameof(GetEpfEtfRecord), new { id = r.Id }, ApiResponse<EpfEtfRecordResponse>.Ok(r));
    }

    [HttpPut("epf-etf/{id}")]
    public async Task<IActionResult> UpdateEpfEtfRecord(Guid id, UpdateEpfEtfRecordRequest req)
        => Ok(ApiResponse<EpfEtfRecordResponse>.Ok(await _secretarialService.UpdateEpfEtfRecordAsync(id, req)));

    [HttpDelete("epf-etf/{id}")]
    public async Task<IActionResult> DeleteEpfEtfRecord(Guid id)
    { await _secretarialService.DeleteEpfEtfRecordAsync(id); return Ok(ApiResponse<object>.Ok(null)); }

    // ── EPF/ETF Staff (nested) ─────────────────────────────────────

    [HttpGet("epf-etf/{id}/staff")]
    public async Task<IActionResult> GetStaff(Guid id)
        => Ok(ApiResponse<List<EpfEtfStaffResponse>>.Ok(await _secretarialService.GetStaffAsync(id)));

    [HttpPost("epf-etf/{id}/staff")]
    public async Task<IActionResult> AddStaff(Guid id, CreateEpfEtfStaffRequest req)
        => Ok(ApiResponse<EpfEtfStaffResponse>.Ok(await _secretarialService.AddStaffAsync(id, req)));

    [HttpPut("epf-etf/{id}/staff/{staffId}")]
    public async Task<IActionResult> UpdateStaff(Guid id, Guid staffId, UpdateEpfEtfStaffRequest req)
        => Ok(ApiResponse<EpfEtfStaffResponse>.Ok(await _secretarialService.UpdateStaffAsync(id, staffId, req)));

    [HttpDelete("epf-etf/{id}/staff/{staffId}")]
    public async Task<IActionResult> DeleteStaff(Guid id, Guid staffId)
    { await _secretarialService.DeleteStaffAsync(id, staffId); return Ok(ApiResponse<object>.Ok(null)); }

    [HttpPatch("epf-etf/{id}/staff/{staffId}/process")]
    public async Task<IActionResult> UpdateStaffProcess(Guid id, Guid staffId, [FromBody] UpdateProcessRequest req)
        => Ok(ApiResponse<EpfEtfStaffResponse>.Ok(await _secretarialService.UpdateStaffProcessAsync(id, staffId, req.Process)));

    // ── Generic Secretarial Modules ────────────────────────────────

    // Trade Marks
    [HttpGet("trade-marks")]
    public async Task<IActionResult> GetTradeMarks([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.TradeMark>(p)));
    [HttpGet("trade-marks/{id}")]
    public async Task<IActionResult> GetTradeMarkById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.TradeMark>(id));
    [HttpPost("trade-marks")]
    public async Task<IActionResult> CreateTradeMark(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.TradeMark>(req)));
    [HttpPut("trade-marks/{id}")]
    public async Task<IActionResult> UpdateTradeMark(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.TradeMark>(id, req)));
    [HttpDelete("trade-marks/{id}")]
    public async Task<IActionResult> DeleteTradeMark(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.TradeMark>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // Trade Licenses
    [HttpGet("trade-licenses")]
    public async Task<IActionResult> GetTradeLicenses([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.TradeLicense>(p)));
    [HttpGet("trade-licenses/{id}")]
    public async Task<IActionResult> GetTradeLicenseById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.TradeLicense>(id));
    [HttpPost("trade-licenses")]
    public async Task<IActionResult> CreateTradeLicense(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.TradeLicense>(req)));
    [HttpPut("trade-licenses/{id}")]
    public async Task<IActionResult> UpdateTradeLicense(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.TradeLicense>(id, req)));
    [HttpDelete("trade-licenses/{id}")]
    public async Task<IActionResult> DeleteTradeLicense(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.TradeLicense>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // Import/Export
    [HttpGet("import-export")]
    public async Task<IActionResult> GetImportExport([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.ImportExportClearance>(p)));
    [HttpGet("import-export/{id}")]
    public async Task<IActionResult> GetImportExportById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.ImportExportClearance>(id));
    [HttpPost("import-export")]
    public async Task<IActionResult> CreateImportExport(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.ImportExportClearance>(req)));
    [HttpPut("import-export/{id}")]
    public async Task<IActionResult> UpdateImportExport(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.ImportExportClearance>(id, req)));
    [HttpDelete("import-export/{id}")]
    public async Task<IActionResult> DeleteImportExport(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.ImportExportClearance>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // HR Consulting
    [HttpGet("hr-consulting")]
    public async Task<IActionResult> GetHrConsulting([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.HrManagementConsulting>(p)));
    [HttpGet("hr-consulting/{id}")]
    public async Task<IActionResult> GetHrConsultingById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.HrManagementConsulting>(id));
    [HttpPost("hr-consulting")]
    public async Task<IActionResult> CreateHrConsulting(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.HrManagementConsulting>(req)));
    [HttpPut("hr-consulting/{id}")]
    public async Task<IActionResult> UpdateHrConsulting(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.HrManagementConsulting>(id, req)));
    [HttpDelete("hr-consulting/{id}")]
    public async Task<IActionResult> DeleteHrConsulting(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.HrManagementConsulting>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // Business Plans
    [HttpGet("business-plans")]
    public async Task<IActionResult> GetBusinessPlans([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.BusinessPlanValuation>(p)));
    [HttpGet("business-plans/{id}")]
    public async Task<IActionResult> GetBusinessPlanById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.BusinessPlanValuation>(id));
    [HttpPost("business-plans")]
    public async Task<IActionResult> CreateBusinessPlan(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.BusinessPlanValuation>(req)));
    [HttpPut("business-plans/{id}")]
    public async Task<IActionResult> UpdateBusinessPlan(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.BusinessPlanValuation>(id, req)));
    [HttpDelete("business-plans/{id}")]
    public async Task<IActionResult> DeleteBusinessPlan(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.BusinessPlanValuation>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // BOI Registrations
    [HttpGet("boi-registrations")]
    public async Task<IActionResult> GetBoiRegistrations([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.BoiRegistration>(p)));
    [HttpGet("boi-registrations/{id}")]
    public async Task<IActionResult> GetBoiRegistrationById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.BoiRegistration>(id));
    [HttpPost("boi-registrations")]
    public async Task<IActionResult> CreateBoiRegistration(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.BoiRegistration>(req)));
    [HttpPut("boi-registrations/{id}")]
    public async Task<IActionResult> UpdateBoiRegistration(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.BoiRegistration>(id, req)));
    [HttpDelete("boi-registrations/{id}")]
    public async Task<IActionResult> DeleteBoiRegistration(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.BoiRegistration>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // Others
    [HttpGet("others")]
    public async Task<IActionResult> GetOthers([FromQuery] PaginationParams p)
        => Ok(ApiResponse<PaginatedResult<SecretarialRecordResponse>>.Ok(await _secretarialService.GetRecordsAsync<Domain.Entities.OtherSecretarialRecord>(p)));
    [HttpGet("others/{id}")]
    public async Task<IActionResult> GetOthersById(Guid id)
        => OkOrNotFound(await _secretarialService.GetRecordByIdAsync<Domain.Entities.OtherSecretarialRecord>(id));
    [HttpPost("others")]
    public async Task<IActionResult> CreateOther(CreateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.CreateRecordAsync<Domain.Entities.OtherSecretarialRecord>(req)));
    [HttpPut("others/{id}")]
    public async Task<IActionResult> UpdateOther(Guid id, UpdateSecretarialRecordRequest req)
        => Ok(ApiResponse<SecretarialRecordResponse>.Ok(await _secretarialService.UpdateRecordAsync<Domain.Entities.OtherSecretarialRecord>(id, req)));
    [HttpDelete("others/{id}")]
    public async Task<IActionResult> DeleteOther(Guid id)
    { await _secretarialService.DeleteRecordAsync<Domain.Entities.OtherSecretarialRecord>(id); return Ok(ApiResponse<object>.Ok(null)); }

    // ── Helper ─────────────────────────────────────────────────────

    private IActionResult OkOrNotFound(SecretarialRecordResponse? record)
        => record != null
            ? Ok(ApiResponse<SecretarialRecordResponse>.Ok(record))
            : NotFound(ApiResponse<SecretarialRecordResponse>.Fail("Record not found."));
}
