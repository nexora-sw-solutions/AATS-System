using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var totalSecretarial = await _context.AuditRecords.CountAsync();
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalSecretarialRecords = totalSecretarial
                    },
                    error = (object?)null
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, error = new { message = ex.Message } });
            }
        }
    }
}
