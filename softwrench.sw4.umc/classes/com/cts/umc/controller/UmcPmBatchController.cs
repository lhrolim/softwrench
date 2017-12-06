using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Web.Http;
using cts.commons.web.Attributes;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.SPF;

namespace softwrench.sw4.umc.classes.com.cts.umc.controller {

    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class UmcPmBatchController : ApiController {

        [Import]
        public MultiItemBatchSubmissionService BatchSubmissionService { get; set; }

        [HttpPost]
        public IApplicationResponse SubmitBatch(BatchSubmissionData batchData) {
            if (batchData.Alias == null) {
                batchData.Alias = "pm_bulk_update " + DateTime.Now.ToShortDateString();
            }

            string ids;
            var json = BuildJsonData(batchData, out ids);
            var targetResult = BatchSubmissionService.CreateAndSubmit("pm", "viewdetail", json, ids, batchData.Alias, new BatchOptions {
                GenerateProblems = true,
                SendEmail = false,
                Synchronous = false,
                GenerateReport = true
            });
            return new GenericApplicationResponse { ResultObject = new { batchid = targetResult.UserId } };
        }

        private static JObject BuildJsonData(BatchSubmissionData batchData, out string itemIds) {
            var array = new JArray();
            var ids = new List<string>();
            batchData.Data.ForEach(data => {
                var pmjson = new JObject {
                    { "pmuid", data["id"] },
                    { "pmnum", data["pmnum"] },
                    { "DESCRIPTION_LONGDESCRIPTION", batchData.LongDescription },
                    { "siteid", "UMCSITE"},
                    { "orgid", "UMCORG"}
                };
                ids.Add(data["id"]);
                array.Add(pmjson);
            });
            itemIds = string.Join(",", ids);
            return new JObject { { "#pmlist_", array } };
        }

        public class BatchSubmissionData {

            public List<IDictionary<string, string>> Data {
                get; set;
            }

            public string LongDescription {
                get; set;
            }

            public string Alias {
                get; set;
            }
        }
    }
}
