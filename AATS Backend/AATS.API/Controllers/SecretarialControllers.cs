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

        protected async Task<List<AuditRecord>> GetSecretarialRecordsAsync(string category, bool enrich = true)
        {
            var list = await _context.AuditRecords
                .Where(r => r.Category.ToLower() == category.ToLower())
                .Include(r => r.Officers)
                .Include(r => r.StaffMembers)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

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
    }

    [Route("api/v1/Secretarial/company-registrations")]
    [ApiController]
    public class CompanyRegistrationsController : SecretarialRecordController
    {
        public CompanyRegistrationsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetSecretarialRecordsAsync("Company Registration", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.AuditRecords.Include(r => r.Officers).FirstOrDefaultAsync(r => r.Id == id);
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
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing == null) return NotFound();
            record.Id = id;
            _context.Entry(existing).CurrentValues.SetValues(record);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = existing });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing != null) { _context.AuditRecords.Remove(existing); await _context.SaveChangesAsync(); }
            return Ok(new { success = true });
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
            var items = await GetSecretarialRecordsAsync("EPF / ETF", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
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
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing == null) return NotFound();
            record.Id = id;
            _context.Entry(existing).CurrentValues.SetValues(record);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = existing });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.AuditRecords.FindAsync(id);
            if (existing != null) { _context.AuditRecords.Remove(existing); await _context.SaveChangesAsync(); }
            return Ok(new { success = true });
        }
    }

    [Route("api/v1/Secretarial/trade-marks")]
    [ApiController]
    public class TradeMarksController : SecretarialRecordController
    {
        public TradeMarksController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trademark", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trademark", "SEC-TRD", r) });
    }

    [Route("api/v1/Secretarial/trade-licenses")]
    [ApiController]
    public class TradeLicensesController : SecretarialRecordController
    {
        public TradeLicensesController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trade License", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trade License", "SEC-LIC", r) });
    }

    [Route("api/v1/Secretarial/form-15")]
    [ApiController]
    public class Form15Controller : SecretarialRecordController
    {
        public Form15Controller(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Form-15", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Form-15", "SEC-F15", r) });
    }

    [Route("api/v1/Secretarial/payroll")]
    [ApiController]
    public class PayrollController : SecretarialRecordController
    {
        public PayrollController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Payroll", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Payroll", "SEC-PAY", r) });
    }

    [Route("api/v1/Secretarial/import-export")]
    [ApiController]
    public class ImportExportController : SecretarialRecordController
    {
        public ImportExportController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Import / Export", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Import / Export", "SEC-IMP", r) });
    }

    [Route("api/v1/Secretarial/hr-consulting")]
    [ApiController]
    public class HrConsultingController : SecretarialRecordController
    {
        public HrConsultingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("HR Consulting", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("HR Consulting", "SEC-HR", r) });
    }

    [Route("api/v1/Secretarial/business-plans")]
    [ApiController]
    public class BusinessPlansController : SecretarialRecordController
    {
        public BusinessPlansController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Business Plans", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Business Plans", "SEC-BUS", r) });
    }

    [Route("api/v1/Secretarial/boi-registrations")]
    [ApiController]
    public class BoiRegistrationsController : SecretarialRecordController
    {
        public BoiRegistrationsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("BOI Registration", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("BOI Registration", "SEC-BOI", r) });
    }

    [Route("api/v1/Secretarial/others")]
    [ApiController]
    public class SecretarialOthersController : SecretarialRecordController
    {
        public SecretarialOthersController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Others", enrich) } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Others", "SEC-OTH", r) });
    }
}
