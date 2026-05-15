using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;

namespace AATS.API.Controllers
{
    [Route("api/v1/Nexora/requests")]
    public class NexoraRequestsController : BaseApiController<NexoraServiceRequest>
    {
        public NexoraRequestsController(IRepository<NexoraServiceRequest> repository) : base(repository) { }
    }

    [Route("api/v1/payments")]
    public class PaymentsController : BaseApiController<Payment>
    {
        public PaymentsController(IRepository<Payment> repository) : base(repository) { }
    }

    [Route("api/v1/documents")]
    public class DocumentsController : BaseApiController<Document>
    {
        public DocumentsController(IRepository<Document> repository) : base(repository) { }
    }

    [Route("api/v1/activity-logs")]
    public class ActivityLogsController : BaseApiController<ActivityLog>
    {
        public ActivityLogsController(IRepository<ActivityLog> repository) : base(repository) { }
    }
}
