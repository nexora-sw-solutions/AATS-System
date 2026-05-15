using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;

namespace AATS.API.Controllers.Auditing
{
    [Route("api/v1/Audit/assurance")]
    public class AuditAssuranceController : AuditBaseController<AuditAssuranceRecord>
    {
        public AuditAssuranceController(IRepository<AuditAssuranceRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-ASR", "AuditAssurance") { }
    }

    [Route("api/v1/Audit/forensic")]
    public class ForensicAuditController : AuditBaseController<ForensicAuditRecord>
    {
        public ForensicAuditController(IRepository<ForensicAuditRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-FOR", "ForensicAudit") { }
    }

    [Route("api/v1/Audit/internal")]
    public class InternalAuditController : AuditBaseController<InternalAuditRecord>
    {
        public InternalAuditController(IRepository<InternalAuditRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-INT", "InternalAudit") { }
    }

    [Route("api/v1/Audit/internal-control")]
    public class InternalControlController : AuditBaseController<InternalControlRecord>
    {
        public InternalControlController(IRepository<InternalControlRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-CTL", "InternalControl") { }
    }

    [Route("api/v1/Audit/management-accounts")]
    public class ManagementAccountController : AuditBaseController<ManagementAccountRecord>
    {
        public ManagementAccountController(IRepository<ManagementAccountRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-MGT", "ManagementAccount") { }
    }

    [Route("api/v1/Audit/others")]
    public class AuditOthersController : AuditBaseController<OtherAuditRecord>
    {
        public AuditOthersController(IRepository<OtherAuditRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "AUD-OTH", "AuditOthers") { }
    }

    public abstract class AuditBaseController<T> : BaseApiController<T> where T : RecordBase
    {
        protected readonly IRecordService _recordService;
        private readonly string _prefix;
        private readonly string _module;

        public AuditBaseController(IRepository<T> repository, IRecordService recordService, string prefix, string module) 
            : base(repository) 
        {
            _recordService = recordService;
            _prefix = prefix;
            _module = module;
        }

        [HttpPost]
        public override async Task<ActionResult<ApiResponse<T>>> Create(T record)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var branchNameClaim = User.FindFirst("BranchName")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                record.CreatedBy = userId;
            }

            if (!string.IsNullOrEmpty(branchNameClaim))
            {
                record.BranchName = branchNameClaim;
            }

            record.RecordCode = await _recordService.GenerateRecordCodeAsync(_prefix);
            
            await _repository.AddAsync(record);
            await _repository.SaveChangesAsync();

            if (record.ClientId.HasValue)
            {
                await _recordService.UpdateClientBalanceAsync(record.ClientId.Value, record.TotalPayment);
            }
            
            await _recordService.ProcessChequeDetailsAsync(record, _module);
            
            await _recordService.LogActivityAsync(record.CreatedBy , record.BranchId , "CREATE", _module, record.Id, $"Created {_module} {record.RecordCode}");

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, ApiResponse<T>.Ok(record));
        }
    }
}
