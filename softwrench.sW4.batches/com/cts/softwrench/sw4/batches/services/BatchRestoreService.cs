using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services {

    public class BatchRestoreService : ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private readonly ISWDBHibernateDAO _dao;

        private IBatchSubmissionService _submissionService;

        public BatchRestoreService(IBatchSubmissionService submissionService, ISWDBHibernateDAO dao) {
            this._submissionService = submissionService;
            _dao = dao;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var submittingBatches = _dao.FindByQuery<MultiItemBatch>(MultiItemBatch.SubmittingBatches);

            TaskScheduler syncContextScheduler;
            if (SynchronizationContext.Current != null) {
                syncContextScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            } else {
                // If there is no SyncContext for this thread (e.g. we are in a unit test
                // or console scenario instead of running in an app), then just use the
                // default scheduler because there is no UI thread to sync with.
                syncContextScheduler = TaskScheduler.Current;
            }


            Task.Factory.StartNew(() => Thread.Sleep(30 * 1000))
                .ContinueWith((t) => {
                    foreach (var batch in submittingBatches) {
                        _submissionService.Submit(batch);
                    }
                }, syncContextScheduler);
        }

        public int Order {
            get { return 1000; }
        }
    }
}
