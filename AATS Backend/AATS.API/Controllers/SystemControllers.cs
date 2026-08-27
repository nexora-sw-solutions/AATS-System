using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;
using AATS.Application.Common.Interfaces;

namespace AATS.API.Controllers
{
    [Route("api/v1/Nexora/requests")]
    [ApiController]
    public class NexoraRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public NexoraRequestsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.NexoraRequests.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NexoraRequest entity)
        {
            _context.NexoraRequests.Add(entity);
            await _context.SaveChangesAsync();

            try
            {
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 650px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #ffffff;'>
                            <h2 style='color: #1e40af; border-bottom: 2px solid #1e40af; padding-bottom: 10px; margin-top: 0;'>New Nexora Service Application</h2>
                            <p>A new Nexora service application has been submitted through the AATS portal.</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; background-color: #f8fafc; width: 30%;'>Client Name</td>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0;'>{entity.ClientName ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; background-color: #f8fafc;'>Service Requested</td>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; color: #0284c7;'>{entity.ServiceType ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; background-color: #f8fafc;'>Status</td>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0;'>{entity.Status ?? "PENDING"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; background-color: #f8fafc;'>Submission Date</td>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0;'>{DateTime.Now:MMM dd, yyyy HH:mm:ss}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0; font-weight: bold; background-color: #f8fafc;'>Additional Details</td>
                                    <td style='padding: 10px; border: 1px solid #e2e8f0;'>{entity.Details ?? "None provided"}</td>
                                </tr>
                            </table>

                            <p style='margin-top: 25px; font-size: 12px; color: #64748b;'>This email was generated automatically by the AATS Nexora Portal System.</p>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync("nexora280@gmail.com", $"New Nexora Application: {entity.ClientName ?? "Client"} - {entity.ServiceType ?? "Service"}", body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send Nexora email to nexora280@gmail.com: {ex.Message}");
            }

            return Ok(new { success = true, data = entity });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] NexoraRequest entity)
        {
            var existing = await _context.NexoraRequests.FindAsync(id);
            if (existing == null) return NotFound();
            entity.Id = id;
            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = existing });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _context.NexoraRequests.FindAsync(id);
            if (existing != null) { _context.NexoraRequests.Remove(existing); await _context.SaveChangesAsync(); }
            return Ok(new { success = true });
        }
    }

    [Route("api/v1/activity-logs")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.ActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .Select(log => new
                {
                    id = log.Id,
                    createdAt = log.CreatedAt,
                    action = log.Action,
                    module = log.Module,
                    description = log.Description,
                    user = new { username = log.UserName ?? "System" },
                    branch = new { name = log.BranchName ?? "Central" }
                })
                .ToListAsync();

            return Ok(new { success = true, data = new { items, totalCount = items.Count }, error = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ActivityLog log)
        {
            if (log.CreatedAt == default) log.CreatedAt = DateTime.UtcNow;
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = log });
        }
    }
}
