using System;
using Common.Logging;
using Quartz;
using softWrench.sW4.Configuration;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Scheduler.Interfaces;
using softWrench.sW4.SimpleInjector.Events;
using System.Threading.Tasks;

namespace softWrench.sW4.Scheduler.Jobs {
    public class ActivityStreamPopulationJob : ASwJob {

        //private ILog _log;
        private readonly IConfigurationFacade _facade;

        public ActivityStreamPopulationJob(IConfigurationFacade facade) {
            _facade = facade;
        }

        public override string Name() {
            return "Activity stream population";
        }

        public override string Description() {
            return "Job to populate the activity stream";
        }

        public override string Cron() {
            return "0 0-59 * ? * *";
        }

        public override void ExecuteJob() {
            //_log = LogManager.GetLogger(typeof(CacheCleanupJob));
            //_log.Info(string.Format("Executed in : {0}", DateTime.Now));

            //_log.Info(string.Format("Finished in : {0}", DateTime.Now));
        }

        public override void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (RunAtStartup()) {
                Task.Factory.StartNew(DoExecute);
            }
        }

        public override bool RunAtStartup() {
            return true;
        }
    }
}
