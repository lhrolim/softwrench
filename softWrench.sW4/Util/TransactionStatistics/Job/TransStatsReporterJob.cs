using Common.Logging;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Scheduler.Jobs;
using softWrench.sW4.Util.TransactionStatistics.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.TransactionStatistics.Job {
    public class TransStatsReporterJob : ASwJob {        
        private readonly ILog _log;
        private readonly TransactionStatsEmailer service;
        private readonly IConfigurationFacade configurationFacade;

        public TransStatsReporterJob (TransactionStatsEmailer service, IConfigurationFacade configurationFacade) {
            this.service = service;
            this.configurationFacade = configurationFacade;
        }

        public override string Cron() {
            //return string.Format("0 */{0} * ? * *", "5");

            //0 0 12 1/5 * ?	Fire at 12pm (noon) every 5 days every month, starting on the first day of the month
            var period = configurationFacade.Lookup<int>(ConfigurationConstants.TransactionStatsReportDuration);
            return string.Format("0 0 12 1/{0} * ?", period);
        }

        public override string Description() {
            return "This job will send out a transaction statistics report as an email";
        }

        public override async Task ExecuteJob() {
            this.service.SendEmail();
        }

        public override string Name() {
            return "Transaction Statistics Report Emailer";
        }

        public override bool RunAtStartup() {
            return false;
        }
    }
}
