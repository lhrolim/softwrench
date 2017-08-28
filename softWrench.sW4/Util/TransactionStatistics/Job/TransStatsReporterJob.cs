using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util.TransactionStatistics.Email;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.TransactionStatistics.Job {
    public class TransStatsReporterJob : ASwJob {
        private readonly TransactionStatsEmailer service;
        private readonly IConfigurationFacade configurationFacade;

        public TransStatsReporterJob(TransactionStatsEmailer service, IConfigurationFacade configurationFacade) {
            this.service = service;
            this.configurationFacade = configurationFacade;
        }

        public override string Cron() {
            //return string.Format("0 */{0} * ? * *", "5");

            //0 0 12 1/5 * ?	Fire at 12pm (noon) every 5 days every month, starting on the first day of the month
            var period = configurationFacade.Lookup<int>(ConfigurationConstants.TransactionStatsReportDuration);
            return $"0 0 12 1/{period} * ?";
        }

        public override string Description() {
            return "This job will send out a transaction statistics report as an email";
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task ExecuteJob() {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
