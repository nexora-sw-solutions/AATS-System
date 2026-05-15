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
            
            await _recordService.ProcessChequeDetailsAsync(record, _module);

            await _recordService.LogActivityAsync(record.CreatedBy , record.BranchId , "CREATE", _module, record.Id, $"Created {_module} {record.RecordCode}");

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, ApiResponse<T>.Ok(record));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<T>>> Update(Guid id, T entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<T>.Failure("INVALID_ID", "ID mismatch"));

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<T>.Failure("NOT_FOUND", "Item not found"));

            // Preserve CreatedBy if not provided in the update
            if (entity.CreatedBy == null || entity.CreatedBy == Guid.Empty)
            {
                entity.CreatedBy = existing.CreatedBy;
            }

            // Use reflection or manual mapping to update properties from base RecordBase
            _context.Entry(existing).CurrentValues.SetValues(entity);
            
            await _repository.SaveChangesAsync();
            
            await _recordService.ProcessChequeDetailsAsync(existing, _module);
            
            return Ok(ApiResponse<T>.Ok(existing));
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
        public override async Task<ActionResult<ApiResponse<CompanyRegistration>>> Update(Guid id, CompanyRegistration entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<CompanyRegistration>.Failure("INVALID_ID", "ID mismatch"));

            var existing = await _context.CompanyRegistrations
                .Include(x => x.Officers)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null) return NotFound(ApiResponse<CompanyRegistration>.Failure("NOT_FOUND", "Item not found"));

            // Preserve immutable fields
            entity.CreatedBy = existing.CreatedBy;
            if (string.IsNullOrEmpty(entity.RecordCode))
            {
                entity.RecordCode = existing.RecordCode;
            }

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
                    var existingOfficer = officer.Id != Guid.Empty 
                        ? existing.Officers.FirstOrDefault(o => o.Id == officer.Id)
                        : null;

                    if (existingOfficer == null)
                    {
                        var newOfficer = new CompanyOfficer
                        {
                            Id = Guid.NewGuid(),
                            CompanyRegistrationId = id,
                            Name = officer.Name,
                            Position = officer.Position,
                            OfficerType = officer.OfficerType,
                            NicNumber = officer.NicNumber,
                            SharePercentage = officer.SharePercentage,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = existing.CreatedBy
                        };
                        existing.Officers.Add(newOfficer);
                        _context.Entry(newOfficer).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        Console.WriteLine($"[DEBUG] Forced state to Added for new officer: {newOfficer.Id}");
                    }
                    else
                    {
                        _context.Entry(existingOfficer).CurrentValues.SetValues(officer);
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            await _recordService.ProcessChequeDetailsAsync(existing, "CompanyRegistration");
            
            return Ok(ApiResponse<CompanyRegistration>.Ok(existing));
        }
    }

    [Route("api/v1/Secretarial/epf-etf")]
    public class EpfEtfController : SecretarialBaseController<EpfEtfRecord>
    {
        public EpfEtfController(IRepository<EpfEtfRecord> repository, IRecordService recordService, ApplicationDbContext context) 
            : base(repository, recordService, context, "SEC-EPF", "EPF/ETF") { }

        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<EpfEtfRecord>>> GetById(Guid id)
        {
            var item = await _context.EpfEtfRecords
                .Include(x => x.StaffMembers)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (item == null) return NotFound(ApiResponse<EpfEtfRecord>.Failure("NOT_FOUND", "Item not found"));
            
            return Ok(ApiResponse<EpfEtfRecord>.Ok(item));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<EpfEtfRecord>>> Update(Guid id, EpfEtfRecord entity)
        {
            try
            {
                if (id != entity.Id)
                {
                    Console.WriteLine($"[ERROR] ID mismatch in EPF/ETF Update. URL ID: {id}, Entity ID: {entity.Id}");
                    return BadRequest(ApiResponse<EpfEtfRecord>.Failure("INVALID_ID", "ID mismatch"));
                }

                var existing = await _context.EpfEtfRecords
                    .Include(x => x.StaffMembers)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null) return NotFound(ApiResponse<EpfEtfRecord>.Failure("NOT_FOUND", "Item not found"));

                // Preserve immutable fields
                entity.CreatedBy = existing.CreatedBy;
                if (string.IsNullOrEmpty(entity.RecordCode))
                {
                    entity.RecordCode = existing.RecordCode;
                }

                // Update simple properties
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;

                // Update StaffMembers collection
                if (entity.StaffMembers != null)
                {
                    // Remove members not in the new list
                    foreach (var member in existing.StaffMembers.ToList())
                    {
                        bool existsInNew = entity.StaffMembers.Any(m => 
                            (m.Id != Guid.Empty && m.Id == member.Id) || 
                            (m.StaffCode == member.StaffCode && m.Name == member.Name));
                            
                        if (!existsInNew)
                        {
                            _context.EpfEtfStaffMembers.Remove(member);
                        }
                    }

                    // Add or update members
                    foreach (var member in entity.StaffMembers)
                    {
                        var existingMember = member.Id != Guid.Empty 
                            ? existing.StaffMembers.FirstOrDefault(m => m.Id == member.Id)
                            : null;

                        if (existingMember == null)
                        {
                            Console.WriteLine($"[DEBUG] Adding new staff: {member.StaffCode} - {member.Name}");
                            var newMember = new EpfEtfStaffMember
                            {
                                Id = Guid.NewGuid(),
                                EpfEtfRecordId = id,
                                StaffCode = member.StaffCode,
                                Name = member.Name,
                                Phone = member.Phone,
                                ProcessStatus = member.ProcessStatus,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                CreatedBy = existing.CreatedBy
                            };
                            existing.StaffMembers.Add(newMember);
                            _context.Entry(newMember).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                            Console.WriteLine($"[DEBUG] Forced state to Added for new staff: {newMember.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Updating existing staff: {member.Id} ({member.StaffCode})");
                            // Manually update fields to avoid overwriting ID or record reference
                            existingMember.StaffCode = member.StaffCode;
                            existingMember.Name = member.Name;
                            existingMember.Phone = member.Phone;
                            existingMember.ProcessStatus = member.ProcessStatus;
                            existingMember.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                // Only auto-update the count if actual staff members were provided in the request
                // This allows manual entry to persist if the staff list tracking is not used
                if (entity.StaffMembers != null && entity.StaffMembers.Any())
                {
                    existing.NumberOfStaff = existing.StaffMembers.Count;
                    existing.NoOfStaffs = existing.StaffMembers.Count;
                }

                Console.WriteLine($"[DEBUG] Saving EPF/ETF record {id}. Staff count: {existing.StaffMembers.Count}");
                await _context.SaveChangesAsync();
                await _recordService.ProcessChequeDetailsAsync(existing, "EPF/ETF");
                
                return Ok(ApiResponse<EpfEtfRecord>.Ok(existing));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EpfEtfRecord>.Failure("SERVER_ERROR", ex.Message));
            }
        }
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
