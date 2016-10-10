using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softWrench.sW4.Configuration.Services.Api;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.services {
    public class OffLineBatchService : ISingletonComponent {
        private readonly ISWDBHibernateDAO _swdbHibernateDAO;

        private static readonly ILog Log = LogManager.GetLogger(typeof(OffLineBatchService));

        private readonly IConfigurationFacade _configurationFacade;

        private readonly BatchItemSubmissionService _batchItemSubmissionService;

        public OffLineBatchService(ISWDBHibernateDAO swdbHibernateDAO, IConfigurationFacade configurationFacade, BatchItemSubmissionService batchItemSubmissionService) {
            _swdbHibernateDAO = swdbHibernateDAO;
            _configurationFacade = configurationFacade;
            _batchItemSubmissionService = batchItemSubmissionService;
            Log.DebugFormat("init sync log");
        }


        public Batch SubmitBatch(string application, string remoteId, JObject batchContent) {

            var minSize = _configurationFacade.Lookup<int>(OfflineConstants.AsyncBatchMinSize);

            var batch = new Batch {
                Application = application,
                RemoteId = remoteId,
                Status = BatchStatus.SUBMITTING,
                Items = ClientStateJsonConverter.GetBatchItems(batchContent)
            };
            var isSynchronous = batch.Items.Count <= minSize;
            var batchOptions = new BatchOptions {
                GenerateProblems = true,
                GenerateReport = false,
                SendEmail = false,
                Synchronous = isSynchronous
            };

            if (isSynchronous) {
                return _batchItemSubmissionService.Submit(batch, batchOptions);
            }
            _swdbHibernateDAO.Save(batch);
            //TODO: replace with rabbitMQ
            Task.Factory.NewThread(() => _batchItemSubmissionService.Submit(batch, batchOptions));
            return batch;
            //async call here
        }
        
        /// <summary>
        /// Fetches the Batches with matching RemoteId and formats them (fill in SuccessItems and Problems accordingly).
        /// </summary>
        /// <param name="remoteIds"></param>
        /// <returns></returns>
        public async Task<IList<Batch>> GetBatchesByRemoteIds(IList<string> remoteIds) {
            var batches = await _swdbHibernateDAO.FindByQueryAsync<Batch>(Batch.BatchesByRemoteId, remoteIds);
            FormatBatches(batches);
            return batches;
        }

        private void FormatBatches(IEnumerable<Batch> batches) {
            foreach (var batch in batches) {
                var items = batch.Items;
                foreach (var item in items) {
                    var problem = item.Problem;
                    if (problem == null) {
                        batch.SuccessItems.Add(item.RemoteId);
                    } else {
                        batch.Problems.Add(item.RemoteId, problem);
                    }
                }
            }
        }

    }
}
