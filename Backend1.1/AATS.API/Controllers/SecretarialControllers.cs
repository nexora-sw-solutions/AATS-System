using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers.Secretarial
{
    public abstract class SecretarialBaseController<T> : BaseApiController<T> where T : RecordBase
    {
        protected readonly IRecordService _recordService;
        protected readonly ApplicationDbContext _context;
        private readonly string _prefix;
        private readonly string _module;

        public SecretarialBaseController(IRepository<T> repository, IRecordService recordService, ApplicationDbContext context, string prefix, string module) 
            : base(repository) 
        {
            _recordService = recordService;
            _context = context;
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

            await _recordService.LogActivityAsync(record.CreatedBy ?? Guid.Empty, record.BranchId, "CREATE", _module, record.Id, $"Created {_module} {record.RecordCode}");

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, ApiResponse<T>.Ok(record));
        }

        [HttpPut("{id}")]
        public override async Task<IActionResult> Update(Guid id, T entity)
        {
            if (id != entity.Id) return BadRequest();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Use reflection or manual mapping to update properties from base RecordBase
            _context.Entry(existing).CurrentValues.SetValues(entity);
            
            // Handle specialized collections or nested objects if necessary
            // For now, SetValues handles top-level properties of the entity T
            
            await _repository.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/v1/Secretarial/company-registrations")]
    public class CompanyRegistrationController : SecretarialBaseController<CompanyRegistration>
    {
        public CompanyRegistrationController(IRepository<CompanyRegistration> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-REG", "CompanyRegistration") { }

        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<CompanyRegistration>>> GetById(Guid id)
        {
            var item = await _context.CompanyRegistrations
                .Include(x => x.Officers)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (item == null) return NotFound(ApiResponse<CompanyRegistration>.Failure("NOT_FOUND", "Item not found"));
            
            return Ok(ApiResponse<CompanyRegistration>.Ok(item));
        }

        [HttpPut("{id}")]
        public override async Task<IActionResult> Update(Guid id, CompanyRegistration entity)
        {
            if (id != entity.Id) return BadRequest();

            var existing = await _context.CompanyRegistrations
                .Include(x => x.Officers)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null) return NotFound();

            // Update simple properties
            _context.Entry(existing).CurrentValues.SetValues(entity);

            // Update officers collection
            if (entity.Officers != null)
            {
                // Remove officers not in the new list
                foreach (var officer in existing.Officers.ToList())
                {
                    if (!entity.Officers.Any(o => o.Name == officer.Name && o.Position == officer.Position))
                    {
                        _context.CompanyOfficers.Remove(officer);
                    }
                }

                // Add or update officers
                foreach (var officer in entity.Officers)
                {
                    var existingOfficer = existing.Officers.FirstOrDefault(o => o.Name == officer.Name && o.Position == officer.Position);
                    if (existingOfficer == null)
                    {
                        existing.Officers.Add(officer);
                    }
                    else
                    {
                        _context.Entry(existingOfficer).CurrentValues.SetValues(officer);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/v1/Secretarial/epf-etf")]
    public class EpfEtfController : SecretarialBaseController<EpfEtfRecord>
    {
        public EpfEtfController(IRepository<EpfEtfRecord> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-EPF", "EPF/ETF") { }
    }

    [Route("api/v1/Secretarial/trade-marks")]
    public class TradeMarkController : SecretarialBaseController<TradeMark>
    {
        public TradeMarkController(IRepository<TradeMark> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-TRD", "TradeMark") { }
    }

    [Route("api/v1/Secretarial/trade-licenses")]
    public class TradeLicenseController : SecretarialBaseController<TradeLicense>
    {
        public TradeLicenseController(IRepository<TradeLicense> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-LIC", "TradeLicense") { }
    }

    [Route("api/v1/Secretarial/import-export")]
    public class ImportExportController : SecretarialBaseController<ImportExportClearance>
    {
        public ImportExportController(IRepository<ImportExportClearance> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-IMP", "ImportExport") { }
    }

    [Route("api/v1/Secretarial/hr-consulting")]
    public class HrConsultingController : SecretarialBaseController<HrManagementConsulting>
    {
        public HrConsultingController(IRepository<HrManagementConsulting> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-HR", "HRConsulting") { }
    }

    [Route("api/v1/Secretarial/business-plans")]
    public class BusinessPlanController : SecretarialBaseController<BusinessPlanValuation>
    {
        public BusinessPlanController(IRepository<BusinessPlanValuation> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-BUS", "BusinessPlan") { }
    }

    [Route("api/v1/Secretarial/boi-registrations")]
    public class BoiRegistrationController : SecretarialBaseController<BoiRegistration>
    {
        public BoiRegistrationController(IRepository<BoiRegistration> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-BOI", "BOIRegistration") { }
    }

    [Route("api/v1/Secretarial/others")]
    public class SecretarialOthersController : SecretarialBaseController<OtherSecretarialRecord>
    {
        public SecretarialOthersController(IRepository<OtherSecretarialRecord> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-OTH", "SecretarialOthers") { }
    }
}
