using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services {
    public interface IBatchSubmissionService : ISingletonComponent {
        TargetResult Submit(MultiItemBatch multiItemBatch, JObject jsonOb = null, BatchOptions options = null);

        TargetResult CreateAndSubmit(string application, string schema, JObject datamap, string itemIds = "", string alias = null);
    }
}