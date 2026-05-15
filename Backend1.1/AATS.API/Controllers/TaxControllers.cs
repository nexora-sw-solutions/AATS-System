using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;

namespace AATS.API.Controllers.Taxing
{
    [Route("api/v1/Tax/records")]
    public class TaxAccountController : TaxBaseController<TaxAccountRecord>
    {
        public TaxAccountController(IRepository<TaxAccountRecord> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-ACC", "TaxAccount") { }
    }

    [Route("api/v1/Tax/filings")]
    public class TaxFilingController : TaxBaseController<TaxFiling>
    {
        public TaxFilingController(IRepository<TaxFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-FIL", "TaxFiling") { }
    }

    [Route("api/v1/Tax/vat")]
    public class VatFilingController : TaxBaseController<VatFiling>
    {
        public VatFilingController(IRepository<VatFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-VAT", "VatFiling") { }
    }

    [Route("api/v1/Tax/cit")]
    public class CitFilingController : TaxBaseController<CitFiling>
    {
        public CitFilingController(IRepository<CitFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-CIT", "CitFiling") { }
    }

    [Route("api/v1/Tax/iit")]
    public class IitFilingController : TaxBaseController<IitFiling>
    {
        public IitFilingController(IRepository<IitFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-IIT", "IitFiling") { }
    }

    [Route("api/v1/Tax/sscl")]
    public class SsclFilingController : TaxBaseController<SsclFiling>
    {
        public SsclFilingController(IRepository<SsclFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-SSC", "SsclFiling") { }
    }

    [Route("api/v1/Tax/wht")]
    public class WhtFilingController : TaxBaseController<WhtFiling>
    {
        public WhtFilingController(IRepository<WhtFiling> repository, IRecordService recordService) 
            : base(repository, recordService, "TAX-WHT", "WhtFiling") { }
    }

    public abstract class TaxBaseController<T> : BaseApiController<T> where T : RecordBase
    {
        protected readonly IRecordService _recordService;
        private readonly string _prefix;
        private readonly string _module;

        public TaxBaseController(IRepository<T> repository, IRecordService recordService, string prefix, string module) 
            : base(repository) 
        {
            _recordService = recordService;
            _prefix = prefix;
            _module = module;
        }

        [HttpPost]
        public override async Task<ActionResult<ApiResponse<T>>> Create([FromBody] T record)
        {
            try
            {
                Console.WriteLine($"[DEBUG] TaxBaseController.Create started for {_module}");
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var branchNameClaim = User.FindFirst("BranchName")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    record.CreatedBy = userId;
                }

                if (!string.IsNullOrEmpty(branchNameClaim) && string.IsNullOrEmpty(record.BranchName))
                {
                    record.BranchName = branchNameClaim;
                }

                if (string.IsNullOrEmpty(record.RecordCode))
                {
                    record.RecordCode = await _recordService.GenerateRecordCodeAsync(_prefix);
                }
                
                await _repository.AddAsync(record);
                await _repository.SaveChangesAsync();
                Console.WriteLine($"[DEBUG] Tax record saved with ID: {record.Id}");

                if (record.ClientId.HasValue)
                {
                    await _recordService.UpdateClientBalanceAsync(record.ClientId.Value, record.TotalPayment);
                }
                
                await _recordService.ProcessChequeDetailsAsync(record, _module);
                
                await _recordService.LogActivityAsync(record.CreatedBy , record.BranchId , "CREATE", _module, record.Id, $"Created {_module} {record.RecordCode}");

                return CreatedAtAction(nameof(GetById), new { id = record.Id }, ApiResponse<T>.Ok(record));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in TaxBaseController.Create: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, ApiResponse<T>.Failure("SERVER_ERROR", ex.Message));
            }
        }
    }
}
