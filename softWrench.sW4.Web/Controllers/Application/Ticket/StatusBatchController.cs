using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.Util;
using cts.commons.web.Attributes;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers.Application.Ticket {

    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class StatusBatchController : ApiController {

        private readonly BatchItemSubmissionService _batchService;

        public StatusBatchController(BatchItemSubmissionService batchService) : base() {
            _batchService = batchService;
        }

        [HttpPost]
        public IGenericResponseResult ChangeStatus([FromUri] string application, [FromBody]IEnumerable<IDictionary<string, object>> items) {
            var batch = Batch.TransientInstance(application, SecurityFacade.CurrentUser());
            batch.Platform = ClientPlatform.Web;
            batch.Items = items.Select(i => new BatchItem() {
                Schema = "editdetail",
                Application = application,
                Fields = i,
                Status = BatchStatus.SUBMITTING,
                Operation = "crud_update",
                UpdateDate = DateTime.Now,
            })
            .ToHashedSet();

            var result = _batchService.Submit(batch, new BatchOptions() { Synchronous = true });
            var response = new BlankApplicationResponse {
                SuccessMessage = string.Format("Changed the status of {0} {1}s successfully.", result.TargetResults.Count, application)
            };
            return response;
        }

    }
}
