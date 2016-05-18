using softwrench.sw4.api.classes.application;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchSubmissionProvider : ApplicationFiltereableProvider<IBatchSubmissionConverter<ApplicationMetadata,OperationWrapper>> {

        protected override IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper> LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return null;
        }
    }
}
