using softwrench.sw4.api.classes.application;
using softwrench.sW4.audit.classes.Services.Batch.Data;

namespace softwrench.sW4.audit.Interfaces {

    public interface IAuditPostBatchHandler : IApplicationFiltereable {

        /// <summary>
        /// Operates on the data generated after a BatchItem submit to Maximo.
        /// </summary>
        /// <param name="data"></param>
        void HandlePostBatchAuditData(AuditPostBatchData data);

    }
}
