using softwrench.sw4.api.classes.application;
using softwrench.sW4.audit.Interfaces;

namespace softwrench.sW4.audit.classes.Services.Batch {

    public class AuditPostBatchHandlerProvider : ApplicationFiltereableProvider<IAuditPostBatchHandler> {

        private readonly AuditPostBatchHandler _defaultHandler;

        public AuditPostBatchHandlerProvider(AuditPostBatchHandler defaultHandler) {
            _defaultHandler = defaultHandler;
        }

        protected override IAuditPostBatchHandler LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return _defaultHandler;
        }
    }
}
