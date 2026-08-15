using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    public class TaxRecordController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IRecordService _recordService;

        public TaxRecordController(ApplicationDbContext context, IRecordService recordService)
        {
            _context = context;
            _recordService = recordService;
        }

        protected async Task<List<TaxRecord>> GetTaxRecordsAsync(string taxType)
        {
            var list = await _context.TaxRecords
                .Where(r => r.TaxType.ToLower() == taxType.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var recordIds = list.Select(r => r.Id).ToList();
            var docs = await _context.SourceDocuments
                .Where(d => recordIds.Contains(d.RecordId))
                .ToListAsync();

            foreach (var r in list)
            {
                r.SourceDocuments = docs.Where(d => d.RecordId == r.Id).ToList();
            }

            return list;
        }

        protected async Task<TaxRecord> CreateTaxRecordAsync(string taxType, string prefix, TaxRecord record)
        {
            record.TaxType = taxType;
            if (string.IsNullOrEmpty(record.RecordCode))
            {
                record.RecordCode = await _recordService.GenerateRecordCodeAsync(prefix);
            }

            _context.TaxRecords.Add(record);
            await _context.SaveChangesAsync();

            if (record.ClientId.HasValue && record.TotalPayment > 0)
            {
                await _recordService.UpdateClientBalanceAsync(record.ClientId.Value, record.TotalPayment);
            }

            return record;
        }
    }

    [Route("api/v1/Tax/vat")]
    [ApiController]
    public class VatFilingController : TaxRecordController
    {
        public VatFilingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await GetTaxRecordsAsync("vat");
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaxRecord record) => Ok(new { success = true, data = await CreateTaxRecordAsync("vat", "TAX-VAT", record) });

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TaxRecord record)
        {
            var existing = await _context.TaxRecords.FindAsync(id);
            if (existing == null) return NotFound();
            record.Id = id;
            _context.Entry(existing).CurrentValues.SetValues(record);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = existing });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.TaxRecords.FindAsync(id);
            if (existing != null) { _context.TaxRecords.Remove(existing); await _context.SaveChangesAsync(); }
            return Ok(new { success = true });
        }
    }

    [Route("api/v1/Tax/cit")]
    [ApiController]
    public class CitFilingController : TaxRecordController
    {
        public CitFilingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("cit") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("cit", "TAX-CIT", r) });
    }

    [Route("api/v1/Tax/iit")]
    [ApiController]
    public class IitFilingController : TaxRecordController
    {
        public IitFilingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("iit") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("iit", "TAX-IIT", r) });
    }

    [Route("api/v1/Tax/sscl")]
    [ApiController]
    public class SsclFilingController : TaxRecordController
    {
        public SsclFilingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("sscl") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("sscl", "TAX-SSC", r) });
    }

    [Route("api/v1/Tax/wht")]
    [ApiController]
    public class WhtFilingController : TaxRecordController
    {
        public WhtFilingController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("wht") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("wht", "TAX-WHT", r) });
    }

    [Route("api/v1/Tax/filings")]
    [ApiController]
    public class TaxFilingsController : TaxRecordController
    {
        public TaxFilingsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("filings") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("filings", "TAX-FIL", r) });
    }

    [Route("api/v1/Tax/records")]
    [ApiController]
    public class TaxAccountRecordsController : TaxRecordController
    {
        public TaxAccountRecordsController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }
        [HttpGet] public async Task<IActionResult> GetAll() => Ok(new { success = true, data = new { items = await GetTaxRecordsAsync("records") } });
        [HttpPost] public async Task<IActionResult> Create([FromBody] TaxRecord r) => Ok(new { success = true, data = await CreateTaxRecordAsync("records", "TAX-ACC", r) });
    }
}
