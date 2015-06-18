using softwrench.sw4.api.classes.application;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchSubmissionProvider : ApplicationFiltereableProvider<IBatchSubmissionConverter> {
        
        protected override IBatchSubmissionConverter LocateDefaultItem(string applicationName,string schemaId,string clientName) {
            return null;
        }
    }
}
