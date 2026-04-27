using AuditApp.Application.Common;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public DashboardController(IApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<object>>> GetSummary()
    {
        var totalClients = await _db.Clients.CountAsync();
        var pendingTaxFilings = await _db.TaxFilings.CountAsync(f => f.PaymentStatus == "Pending");
        var pendingSecretarial = await _db.CompanyRegistrations.CountAsync(r => r.PaymentStatus == "Pending");
        
        var recentPayments = await _db.Payments
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new { p.TotalAmount, Date = p.PaymentDate, p.RecordType })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            TotalClients = totalClients,
            PendingTaxFilings = pendingTaxFilings,
            PendingSecretarial = pendingSecretarial,
            RecentPayments = recentPayments
        }));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<object>>> GetRevenue()
    {
        var totalRevenue = await _db.Payments.SumAsync(p => p.TotalAmount);
        var collected = await _db.Payments.SumAsync(p => p.PaidAmount);
        var pending = totalRevenue - collected;

        return Ok(ApiResponse<object>.Ok(new { TotalRevenue = totalRevenue, Collected = collected, Pending = pending }));
    }

    [HttpGet("records-by-status")]
    public async Task<ActionResult<ApiResponse<object>>> GetRecordsByStatus()
    {
        var taxFilings = await _db.TaxFilings.GroupBy(x => x.PaymentStatus).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        var companyRegs = await _db.CompanyRegistrations.GroupBy(x => x.PaymentStatus).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
        
        return Ok(ApiResponse<object>.Ok(new { TaxFilings = taxFilings, CompanyRegistrations = companyRegs }));
    }

    [HttpGet("branch-summary")]
    public async Task<ActionResult<ApiResponse<object>>> GetBranchSummary()
    {
        var branches = await _db.Branches
            .Select(b => new { 
                BranchId = b.Id, 
                BranchName = b.Name, 
                ClientCount = b.Clients.Count() 
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(branches));
    }
}
