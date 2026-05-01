using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Shared;
using AuditApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<PaymentResponse>>>> GetPayments([FromQuery] PaginationParams @params)
        => Ok(ApiResponse<PaginatedResult<PaymentResponse>>.Ok(await _paymentService.GetPaymentsAsync(@params)));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound(ApiResponse<PaymentResponse>.Fail("Payment not found."));
        return Ok(ApiResponse<PaymentResponse>.Ok(payment));
    }

    [HttpGet("by-record/{type}/{id}")]
    public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetPaymentsByReference(string type, Guid id)
        => Ok(ApiResponse<List<PaymentResponse>>.Ok(await _paymentService.GetPaymentsByReferenceAsync(id, type)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> CreatePayment(CreatePaymentRequest request)
    {
        var payment = await _paymentService.CreatePaymentAsync(request);
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, ApiResponse<PaymentResponse>.Ok(payment));
    }

    [HttpPatch("cheques/{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateChequeStatus(Guid id, [FromBody] Domain.Enums.ChequeStatus status)
    {
        await _paymentService.UpdateChequeStatusAsync(id, status);
        return Ok(ApiResponse<object>.Ok(null));
    }
}

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class NexoraController : ControllerBase
{
    private readonly INexoraAppService _nexoraService;

    public NexoraController(INexoraAppService nexoraService)
    {
        _nexoraService = nexoraService;
    }

    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<List<NexoraServiceResponse>>>> GetServices()
        => Ok(ApiResponse<List<NexoraServiceResponse>>.Ok(await _nexoraService.GetActiveServicesAsync()));

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NexoraRequestResponse>>>> GetRequests([FromQuery] PaginationParams @params)
        => Ok(ApiResponse<PaginatedResult<NexoraRequestResponse>>.Ok(await _nexoraService.GetRequestsAsync(@params)));

    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<NexoraRequestResponse>>> CreateRequest(CreateNexoraRequest request)
    {
        var result = await _nexoraService.CreateRequestAsync(request);
        return Ok(ApiResponse<NexoraRequestResponse>.Ok(result));
    }
}
