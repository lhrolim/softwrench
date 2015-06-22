using System;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softWrench.sW4.Configuration.Services.Api;
using softwrench.sw4.offlineserver.services.util;

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

            var minSize = _configurationFacade.Lookup<Int32>(OfflineConstants.AsyncBatchMinSize);

            var batch = new Batch {
                Application = application,
                RemoteId = remoteId,
                Status = BatchStatus.SUBMITTING
            };
            batch.Items = ClientStateJsonConverter.GetBatchItems(batchContent);
            if (batch.Items.Count < minSize) {
                return _batchItemSubmissionService.Submit(batch, new BatchOptions {
                    GenerateProblems = true,
                    GenerateReport = false,
                    SendEmail = false,
                    Synchronous = true
                });
            }
            _swdbHibernateDAO.Save(batch);
            return batch;
            //async call here
        }

    }
}
