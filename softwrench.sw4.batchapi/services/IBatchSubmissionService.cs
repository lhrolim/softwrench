using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sw4.batch.api.services {
    public interface IBatchSubmissionService : ISingletonComponent {
        TargetResult Submit(MultiItemBatch multiItemBatch, JObject jsonOb = null, BatchOptions options = null);

        TargetResult CreateAndSubmit(string application, string schema, JObject datamap, string itemIds = "", string alias = null);

        /// <summary>
        /// Invokes the list of operations in parallel, using the number of threads provided as the BatchOption parameter.
        /// 
        /// There are 2 ways to execute this method, either by passing a list of Datamaps and an optional operationName, or a complete list of OperationWrappers.
        /// 
        /// A Default problem would be generated if a problemKey is informed
        /// 
        /// No Batch information will be persisted, other than eventual problems generated
        /// 
        /// </summary>
        /// <param name="adapter"></param>
        void SubmitTransientBatch(TransientBatchOperationData adapter);
    }
}