using System;
using Common.Logging;
using Quartz;
using softWrench.sW4.Configuration;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Scheduler.Interfaces;

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
            
        }

        public override bool RunAtStartup() {
            return false;
        }
    }
}
