using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softwrench.sw4.offlineserver.services.util;

namespace softwrench.sw4.offlineserver.services {
    public class OffLineBatchService : ISingletonComponent {
        private readonly ISWDBHibernateDAO _swdbHibernateDAO;

        private static readonly ILog Log = LogManager.GetLogger(typeof(OffLineBatchService));

        public OffLineBatchService(ISWDBHibernateDAO swdbHibernateDAO) {
            _swdbHibernateDAO = swdbHibernateDAO;
            Log.DebugFormat("init sync log");
        }


        public void SubmitBatch(string application, string remoteId, JObject batchContent) {
            var batch = new Batch {
                Application = application,
                RemoteId = remoteId,
                Status = BatchStatus.SUBMITTING
            };
            batch.Items = ClientStateJsonConverter.GetBatchItems(batchContent);
            _swdbHibernateDAO.Save(batch);
        }

    }
}
