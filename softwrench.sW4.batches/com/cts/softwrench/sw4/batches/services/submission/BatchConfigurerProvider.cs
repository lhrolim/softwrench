using softwrench.sw4.api.classes.application;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.batch.api.services;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchConfigurerProvider : ApplicationFiltereableProvider<IBatchOptionProvider>, IBatchOptionProvider {

        protected override IBatchOptionProvider LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return this;
        }


        public string ApplicationName() {
            return null;
        }

        public string ClientFilter() {
            return null;
        }

        public string SchemaId() {
            return null;
        }

        public BatchOptions GenerateOptions(IBatch batch) {
            return new BatchOptions() {
                GenerateProblems = false,
                SendEmail = false,
                Synchronous = true
            };
        }
    }
}
