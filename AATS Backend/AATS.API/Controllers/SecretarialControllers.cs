using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    public class SecretarialRecordController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IRecordService _recordService;

        public SecretarialRecordController(ApplicationDbContext context, IRecordService recordService)
        {
            _context = context;
            _recordService = recordService;
        }

        protected async Task<List<AuditRecord>> GetSecretarialRecordsAsync(string category, bool enrich = true, bool includeDeleted = false)
        {
            var now = DateTime.UtcNow;
            string catNorm = category.Replace(" ", "").Replace("&", "and").ToLower();

            var query = _context.AuditRecords
                .Include(r => r.Branch)
                .Include(r => r.Client)
                .ThenInclude(c => c!.Branch)
                .Include(r => r.Officers)
                .Include(r => r.StaffMembers)
                .Where(r => r.Category.ToLower() == category.ToLower() || 
                            r.Category.ToLower().Replace(" ", "").Replace("&", "and") == catNorm);

            var all = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var toPurge = all.Where(r => r.IsDeleted && r.DeletedAt.HasValue && (now - r.DeletedAt.Value).TotalDays >= 30).ToList();
            if (toPurge.Any())
            {
                _context.AuditRecords.RemoveRange(toPurge);
                await _context.SaveChangesAsync();
                all = all.Except(toPurge).ToList();
            }

            var list = includeDeleted ? all.Where(r => r.IsDeleted).ToList() : all.Where(r => !r.IsDeleted).ToList();

            var branches = await _context.Branches.ToListAsync();
            var defaultBranchName = branches.FirstOrDefault()?.Name ?? "Central";

            foreach (var r in list)
            {
                if (string.IsNullOrWhiteSpace(r.BranchName) || r.BranchName.Equals("Unknown", StringComparison.OrdinalIgnoreCase) || r.BranchName.Equals("Unknown branch", StringComparison.OrdinalIgnoreCase))
                {
                    if (r.Branch != null && !string.IsNullOrWhiteSpace(r.Branch.Name))
                    {
                        r.BranchName = r.Branch.Name;
                    }
                    else if (r.Client?.Branch != null && !string.IsNullOrWhiteSpace(r.Client.Branch.Name))
                    {
                        r.BranchName = r.Client.Branch.Name;
                    }
                    else if (r.BranchId.HasValue && branches.Any(b => b.Id == r.BranchId.Value))
                    {
                        r.BranchName = branches.First(b => b.Id == r.BranchId.Value).Name;
                    }
                    else
                    {
                        r.BranchName = defaultBranchName;
                    }
                }
            }

            if (enrich)
            {
                var recordIds = list.Select(r => r.Id).ToList();
                var docs = await _context.SourceDocuments
                    .Where(d => recordIds.Contains(d.RecordId))
                    .ToListAsync();

                foreach (var r in list)
                {
                    r.SourceDocuments = docs.Where(d => d.RecordId == r.Id).ToList();
                }
            }

            return list;
        }

        protected async Task<AuditRecord> CreateSecretarialRecordAsync(string category, string prefix, AuditRecord record)
        {
            record.Category = category;
            record.IsDeleted = false;
            record.DeletedAt = null;
            if (string.IsNullOrEmpty(record.RecordCode))
            {
                record.RecordCode = await _recordService.GenerateRecordCodeAsync(prefix);
            }

            _context.AuditRecords.Add(record);
            await _context.SaveChangesAsync();

            if (record.Officers != null && record.Officers.Count > 0)
            {
                foreach (var off in record.Officers)
                {
                    off.RecordId = record.Id;
                    if (_context.Entry(off).State == EntityState.Detached)
                    {
                        _context.CompanyOfficers.Add(off);
                    }
                }
            }

            if (record.StaffMembers != null && record.StaffMembers.Count > 0)
            {
                foreach (var st in record.StaffMembers)
                {
                    st.RecordId = record.Id;
                    if (_context.Entry(st).State == EntityState.Detached)
                    {
                        _context.StaffMembers.Add(st);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (record.ClientId.HasValue && record.TotalPayment > 0 && record.PaymentStatus != "Paid")
            {
                await _recordService.UpdateClientBalanceAsync(record.ClientId.Value, record.TotalPayment - record.PartialAmount);
            }

            return record;
        }

        protected async Task<AuditRecord?> UpdateSecretarialRecordAsync(Guid id, AuditRecord record)
        {
            var existing = await _context.AuditRecords
                .Include(r => r.Officers)
                .Include(r => r.StaffMembers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existing == null) return null;

            record.Id = id;
            record.Category = existing.Category;
            record.RecordCode = string.IsNullOrEmpty(record.RecordCode) ? existing.RecordCode : record.RecordCode;
            record.IsDeleted = existing.IsDeleted;
            record.DeletedAt = existing.DeletedAt;

            _context.Entry(existing).CurrentValues.SetValues(record);

            if (record.Officers != null)
            {
                _context.CompanyOfficers.RemoveRange(existing.Officers);
                foreach (var off in record.Officers)
                {
                    off.RecordId = id;
                    _context.CompanyOfficers.Add(off);
                }
            }

            if (record.StaffMembers != null)
            {
                _context.StaffMembers.RemoveRange(existing.StaffMembers);
                foreach (var st in record.StaffMembers)
                {
                    st.RecordId = id;
                    _context.StaffMembers.Add(st);
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        protected async Task<bool> SoftDeleteSecretarialRecordAsync(Guid id)
        {
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            existing.Status = "Inactive";

            await _context.SaveChangesAsync();
            return true;
        }

        protected async Task<bool> RestoreSecretarialRecordAsync(Guid id)
        {
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.Status = "ACTIVE";

            await _context.SaveChangesAsync();
            return true;
        }

        protected async Task<bool> PermanentDeleteSecretarialRecordAsync(Guid id)
        {
            var existing = await _context.AuditRecords
                .Include(r => r.Officers)
                .Include(r => r.StaffMembers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existing == null) return false;

            _context.AuditRecords.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    [Route("api/v1/Secretarial/company-registrations")]
    [ApiController]
    public class CompanyRegistrationsController : SecretarialRecordController
    {
        public CompanyRegistrationsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetSecretarialRecordsAsync("Company Registration", enrich, false);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var items = await GetSecretarialRecordsAsync("Company Registration", true, true);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id);
            return item != null ? Ok(new { success = true, data = item }) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateSecretarialRecordAsync("Company Registration", "SEC-REG", record);
            return Ok(new { success = true, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord record)
        {
            var updated = await UpdateSecretarialRecordAsync(id, record);
            return updated != null ? Ok(new { success = true, data = updated }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await SoftDeleteSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await RestoreSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDelete(Guid id)
        {
            var success = await PermanentDeleteSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }
    }

    [Route("api/v1/Secretarial/epf-etf")]
    [ApiController]
    public class EpfEtfController : SecretarialRecordController
    {
        public EpfEtfController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetSecretarialRecordsAsync("EPF / ETF", enrich, false);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var items = await GetSecretarialRecordsAsync("EPF / ETF", true, true);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id);
            return item != null ? Ok(new { success = true, data = item }) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateSecretarialRecordAsync("EPF / ETF", "SEC-EPF", record);
            return Ok(new { success = true, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord record)
        {
            var updated = await UpdateSecretarialRecordAsync(id, record);
            return updated != null ? Ok(new { success = true, data = updated }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await SoftDeleteSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await RestoreSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDelete(Guid id)
        {
            var success = await PermanentDeleteSecretarialRecordAsync(id);
            return success ? Ok(new { success = true }) : NotFound();
        }
    }

    [Route("api/v1/Secretarial/trade-marks")]
    [ApiController]
    public class TradeMarksController : SecretarialRecordController
    {
        public TradeMarksController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trademark", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trademark", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trademark", "SEC-TRD", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/trade-licenses")]
    [ApiController]
    public class TradeLicensesController : SecretarialRecordController
    {
        public TradeLicensesController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trade License", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trade License", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trade License", "SEC-LIC", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/form-15")]
    [ApiController]
    public class Form15Controller : SecretarialRecordController
    {
        public Form15Controller(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Form-15", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Form-15", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Form-15", "SEC-F15", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/payroll")]
    [ApiController]
    public class PayrollController : SecretarialRecordController
    {
        public PayrollController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Payroll", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Payroll", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Payroll", "SEC-PAY", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/import-export")]
    [ApiController]
    public class ImportExportController : SecretarialRecordController
    {
        public ImportExportController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Import / Export", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Import / Export", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Import / Export", "SEC-IMP", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/hr-consulting")]
    [ApiController]
    public class HrConsultingController : SecretarialRecordController
    {
        public HrConsultingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("HR Consulting", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("HR Consulting", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("HR Consulting", "SEC-HR", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/business-plans")]
    [ApiController]
    public class BusinessPlansController : SecretarialRecordController
    {
        public BusinessPlansController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Business Plans", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Business Plans", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Business Plans", "SEC-BUS", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/boi-registrations")]
    [ApiController]
    public class BoiRegistrationsController : SecretarialRecordController
    {
        public BoiRegistrationsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("BOI Registration", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("BOI Registration", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("BOI Registration", "SEC-BOI", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }

    [Route("api/v1/Secretarial/others")]
    [ApiController]
    public class SecretarialOthersController : SecretarialRecordController
    {
        public SecretarialOthersController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Others", enrich, false) } });
        [HttpGet("deleted")] public async Task<IActionResult> GetDeleted() => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Others", true, true) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await _context.AuditRecords.Include(r => r.Officers).Include(r => r.StaffMembers).FirstOrDefaultAsync(r => r.Id == id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Others", "SEC-OTH", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var u = await UpdateSecretarialRecordAsync(id, r); return u != null ? Ok(new { success = true, data = u }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) => await SoftDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpPost("{id}/restore")] public async Task<IActionResult> Restore(Guid id) => await RestoreSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
        [HttpDelete("{id}/permanent")] public async Task<IActionResult> PermanentDelete(Guid id) => await PermanentDeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound();
    }
}
