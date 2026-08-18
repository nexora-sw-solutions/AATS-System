using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    public class AuditRecordController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IRecordService _recordService;

        public AuditRecordController(ApplicationDbContext context, IRecordService recordService)
        {
            _context = context;
            _recordService = recordService;
        }

        protected async Task<List<AuditRecord>> GetRecordsByCategoryAsync(string category, bool enrich = true)
        {
            var query = _context.AuditRecords
                .Include(r => r.Branch)
                .Include(r => r.Client)
                .ThenInclude(c => c.Branch)
                .Where(r => r.Category.ToLower() == category.ToLower());
            var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

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

        protected async Task<AuditRecord?> CreateRecordAsync(string category, string prefix, AuditRecord record)
        {
            record.Category = category;
            if (string.IsNullOrEmpty(record.RecordCode))
            {
                record.RecordCode = await _recordService.GenerateRecordCodeAsync(prefix);
            }

            if (string.IsNullOrWhiteSpace(record.BranchName) || record.BranchName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                if (record.BranchId.HasValue)
                {
                    var branch = await _context.Branches.FindAsync(record.BranchId.Value);
                    if (branch != null) record.BranchName = branch.Name;
                }
                else if (record.ClientId.HasValue)
                {
                    var client = await _context.Clients.Include(c => c.Branch).FirstOrDefaultAsync(c => c.Id == record.ClientId.Value);
                    if (client != null)
                    {
                        record.BranchId = client.BranchId;
                        record.BranchName = client.Branch?.Name ?? "Central";
                    }
                }

                if (string.IsNullOrWhiteSpace(record.BranchName))
                {
                    var defaultBranch = await _context.Branches.FirstOrDefaultAsync();
                    if (defaultBranch != null)
                    {
                        record.BranchId = defaultBranch.Id;
                        record.BranchName = defaultBranch.Name;
                    }
                }
            }

            _context.AuditRecords.Add(record);
            await _context.SaveChangesAsync();

            if (record.ClientId.HasValue && record.TotalPayment > 0 && record.PaymentStatus != "Paid")
            {
                await _recordService.UpdateClientBalanceAsync(record.ClientId.Value, record.TotalPayment - record.PartialAmount);
            }

            return record;
        }
    }

    [Route("api/v1/Audit/assurance")]
    [ApiController]
    public class AuditAssuranceController : AuditRecordController
    {
        public AuditAssuranceController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Assurance", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.AuditRecords.FirstOrDefaultAsync(r => r.Id == id);
            return item != null ? Ok(new { success = true, data = item }) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Assurance", "AUD-ASR", record);
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
            if (existing != null)
            {
                _context.AuditRecords.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }
    }

    [Route("api/v1/Audit/forensic")]
    [ApiController]
    public class ForensicAuditController : AuditRecordController
    {
        public ForensicAuditController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Forensic Audit", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Forensic Audit", "AUD-FOR", record);
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

    [Route("api/v1/Audit/internal")]
    [ApiController]
    public class InternalAuditController : AuditRecordController
    {
        public InternalAuditController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Internal Audit", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Internal Audit", "AUD-INT", record);
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

    [Route("api/v1/Audit/internal-control")]
    [ApiController]
    public class InternalControlController : AuditRecordController
    {
        public InternalControlController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Internal Control", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Internal Control", "AUD-CTL", record);
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

    [Route("api/v1/Audit/management-accounts")]
    [ApiController]
    public class ManagementAccountController : AuditRecordController
    {
        public ManagementAccountController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Management Accounting", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Management Accounting", "AUD-MGT", record);
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

    [Route("api/v1/Audit/others")]
    [ApiController]
    public class AuditOthersController : AuditRecordController
    {
        public AuditOthersController(ApplicationDbContext context, IRecordService recordService) : base(context, recordService) { }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool enrich = true)
        {
            var items = await GetRecordsByCategoryAsync("Others", enrich);
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AuditRecord record)
        {
            var created = await CreateRecordAsync("Others", "AUD-OTH", record);
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
}
