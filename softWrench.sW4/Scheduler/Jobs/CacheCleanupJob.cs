using System;
using Common.Logging;
using Quartz;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Scheduler.Interfaces;

namespace softWrench.sW4.Scheduler.Jobs {
    public class CacheCleanupJob : ISwJob {

        private ILog _log;
        private readonly IConfigurationFacade _facade;

        public CacheCleanupJob(IConfigurationFacade facade) {
            _facade = facade;
        }

        public void Execute(IJobExecutionContext context) {
            ExecuteJob();
        }

        public string Name() {
            return "Cache cleanup";
        }

        public string Description() {
            return "Job to cache clear";
        }

        public string Cron() {
            return "0 15 10 15 * ?";
        }

        public void ExecuteJob() {
            _log = LogManager.GetLogger(typeof(CacheCleanupJob));
            _log.Info(string.Format("Executed in : {0}", DateTime.Now));

            _log.Info(string.Format("Finished in : {0}", DateTime.Now));
        }

        public bool IsScheduled {
            get; set;
        }

        public bool IsEnabled {
            get {
                return false;
            }
        }

        public ILog JobLog() {
            return _log;
        }

        public void OnJobSchedule() {

        }

        public bool RunAtStartup() {
            return false;
        }
    }
}
