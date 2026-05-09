using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;

namespace AATS.API.Controllers.Taxing
{
    [Route("api/v1/Tax/records")]
    public class TaxAccountController : BaseApiController<TaxAccountRecord>
    {
        public TaxAccountController(IRepository<TaxAccountRecord> repository) : base(repository) { }
    }

    [Route("api/v1/Tax/filings")]
    public class TaxFilingController : BaseApiController<TaxFiling>
    {
        public TaxFilingController(IRepository<TaxFiling> repository) : base(repository) { }
    }
}
