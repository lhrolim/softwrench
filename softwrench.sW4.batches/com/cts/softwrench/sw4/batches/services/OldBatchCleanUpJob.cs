using System.Threading.Tasks;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services {
    class OldBatchCleanUpJob : ASwJob {

        private readonly SWDBHibernateDAO _dao;

        public OldBatchCleanUpJob(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public override string Name() {
            return "Old Batches Cleaner";
        }

        public override string Description() {
            return "Cleans up batches older than a configured amount of time";
        }

        public override string Cron() {
            //run every midnight
            return "0 0 0 * * ?";
        }

        public override async Task ExecuteJob() {
            var reports = _dao.FindAll<BatchReport>(typeof(BatchReport));
            foreach (var report in reports) {
                var updateDate = report.OriginalMultiItemBatch.UpdateDate;
                if (updateDate.IsOlderThan(15).Days()) {
                    await _dao.DeleteAsync(report);
                    await _dao.DeleteAsync(report.OriginalMultiItemBatch);
                }
            }
        }

        public override bool RunAtStartup() {
            return true;
        }
    }
}
