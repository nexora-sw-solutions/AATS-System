using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected async Task<AuditRecord?> GetSecretarialRecordByIdAsync(Guid id)
        {
            var item = await _context.AuditRecords
                .Include(r => r.Officers)
                .Include(r => r.StaffMembers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (item != null)
            {
                var docs = await _context.SourceDocuments
                    .Where(d => d.RecordId == id)
                    .ToListAsync();
                item.SourceDocuments = docs;
            }

            return item;
        }

        protected async Task<AuditRecord> CreateSecretarialRecordAsync(string category, string prefix, AuditRecord record)
        {
            record.Category = category;
            if (string.IsNullOrEmpty(record.RecordCode))
            {
                record.RecordCode = await _recordService.GenerateRecordCodeAsync(prefix);
            }

            var officers = record.Officers?.ToList();
            var staffMembers = record.StaffMembers?.ToList();
            var sourceDocs = record.SourceDocuments?.ToList();

            record.Officers = null;
            record.StaffMembers = null;
            record.SourceDocuments = null;

            _context.AuditRecords.Add(record);
            await _context.SaveChangesAsync();

            if (officers != null && officers.Count > 0)
            {
                foreach (var off in officers)
                {
                    off.Id = Guid.NewGuid();
                    off.RecordId = record.Id;
                    _context.CompanyOfficers.Add(off);
                }
            }

            if (staffMembers != null && staffMembers.Count > 0)
            {
                foreach (var st in staffMembers)
                {
                    st.Id = Guid.NewGuid();
                    st.RecordId = record.Id;
                    _context.StaffMembers.Add(st);
                }
            }

            if (sourceDocs != null && sourceDocs.Count > 0)
            {
                foreach (var doc in sourceDocs)
                {
                    doc.RecordId = record.Id;
                    if (doc.Id == Guid.Empty) doc.Id = Guid.NewGuid();
                    _context.SourceDocuments.Add(doc);
                }
            }

            await _context.SaveChangesAsync();

            record.Officers = officers;
            record.StaffMembers = staffMembers;
            record.SourceDocuments = sourceDocs;

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
            _context.Entry(existing).CurrentValues.SetValues(record);

            if (record.Officers != null)
            {
                if (existing.Officers != null && existing.Officers.Any())
                {
                    _context.CompanyOfficers.RemoveRange(existing.Officers);
                }
                foreach (var off in record.Officers)
                {
                    off.Id = Guid.NewGuid();
                    off.RecordId = existing.Id;
                    _context.CompanyOfficers.Add(off);
                }
            }

            if (record.StaffMembers != null)
            {
                if (existing.StaffMembers != null && existing.StaffMembers.Any())
                {
                    _context.StaffMembers.RemoveRange(existing.StaffMembers);
                }
                foreach (var st in record.StaffMembers)
                {
                    st.Id = Guid.NewGuid();
                    st.RecordId = existing.Id;
                    _context.StaffMembers.Add(st);
                }
            }

            if (record.SourceDocuments != null)
            {
                foreach (var doc in record.SourceDocuments)
                {
                    doc.RecordId = existing.Id;
                    if (doc.Id == Guid.Empty) doc.Id = Guid.NewGuid();
                    var existingDoc = await _context.SourceDocuments.FirstOrDefaultAsync(d => d.Id == doc.Id);
                    if (existingDoc == null)
                    {
                        _context.SourceDocuments.Add(doc);
                    }
                    else
                    {
                        _context.Entry(existingDoc).CurrentValues.SetValues(doc);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        protected async Task<bool> DeleteSecretarialRecordAsync(Guid id)
        {
            var existing = await _context.AuditRecords.FindAsync(id);
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

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Company Registration", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Company Registration", "SEC-REG", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/epf-etf")]
    [ApiController]
    public class EpfEtfController : SecretarialRecordController
    {
        public EpfEtfController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("EPF / ETF", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("EPF / ETF", "SEC-EPF", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/trade-marks")]
    [ApiController]
    public class TradeMarksController : SecretarialRecordController
    {
        public TradeMarksController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trademark", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trademark", "SEC-TRD", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/trade-licenses")]
    [ApiController]
    public class TradeLicensesController : SecretarialRecordController
    {
        public TradeLicensesController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Trade License", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Trade License", "SEC-LIC", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/form-15")]
    [ApiController]
    public class Form15Controller : SecretarialRecordController
    {
        public Form15Controller(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Form-15", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Form-15", "SEC-F15", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/payroll")]
    [ApiController]
    public class PayrollController : SecretarialRecordController
    {
        public PayrollController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Payroll", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Payroll", "SEC-PAY", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/import-export")]
    [ApiController]
    public class ImportExportController : SecretarialRecordController
    {
        public ImportExportController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Import / Export", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Import / Export", "SEC-IMP", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/hr-consulting")]
    [ApiController]
    public class HrConsultingController : SecretarialRecordController
    {
        public HrConsultingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("HR Consulting", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("HR Consulting", "SEC-HR", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/business-plans")]
    [ApiController]
    public class BusinessPlansController : SecretarialRecordController
    {
        public BusinessPlansController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Business Plans", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Business Plans", "SEC-BUS", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/boi-registrations")]
    [ApiController]
    public class BoiRegistrationsController : SecretarialRecordController
    {
        public BoiRegistrationsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("BOI Registration", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("BOI Registration", "SEC-BOI", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }

    [Route("api/v1/Secretarial/others")]
    [ApiController]
    public class SecretarialOthersController : SecretarialRecordController
    {
        public SecretarialOthersController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] bool enrich = true) => Ok(new { success = true, data = new { items = await GetSecretarialRecordsAsync("Others", enrich) } });
        [HttpGet("{id}")] public async Task<IActionResult> GetById(Guid id) { var item = await GetSecretarialRecordByIdAsync(id); return item != null ? Ok(new { success = true, data = item }) : NotFound(); }
        [HttpPost] public async Task<IActionResult> Create([FromBody] AuditRecord r) => Ok(new { success = true, data = await CreateSecretarialRecordAsync("Others", "SEC-OTH", r) });
        [HttpPut("{id}")] public async Task<IActionResult> Update(Guid id, [FromBody] AuditRecord r) { var updated = await UpdateSecretarialRecordAsync(id, r); return updated != null ? Ok(new { success = true, data = updated }) : NotFound(); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(Guid id) { return await DeleteSecretarialRecordAsync(id) ? Ok(new { success = true }) : NotFound(); }
    }
}
