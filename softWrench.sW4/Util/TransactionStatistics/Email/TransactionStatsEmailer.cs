using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.TransactionStatistics.Email {
    public class TransactionStatsEmailer : ISingletonComponent {
        private const string NoReplySendFrom = "noreply@controltechnologysolutions.com";
        private const string MailSubject = "Transaction Statistics Report";
        private readonly IEmailService emailService;
        private readonly RedirectService redirectService;
        private readonly IApplicationConfiguration appConfig;
        private readonly TransactionStatisticsService txService;
        private readonly IConfigurationFacade configurationFacade;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatsEmailer"/> class.
        /// </summary>
        /// <param name="emailService">The email service reference</param>
        /// <param name="redirectService"></param>
        public TransactionStatsEmailer(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, TransactionStatisticsService txService, IConfigurationFacade configurationFacade) {
            this.emailService = emailService;
            this.redirectService = redirectService;
            this.appConfig = appConfig;
            this.txService = txService;
            this.configurationFacade = configurationFacade;
        }

        /// <summary>
        /// Email the metadata file that has been changed.
        /// </summary>        
        public void SendEmail() {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//txstatisticsreporttemplate.html";
            var period = configurationFacade.Lookup<int>(ConfigurationConstants.TransactionStatsReportDuration);
            var now = DateTime.Now;
            var periodFrom = now.AddDays(period * -1).ToString();
            var periodTo = now.ToString();

            var queryString = string.Format("fromDateFilter={0}&toDateFilter={1}", periodFrom, periodTo);

            var transactionOverview = this.txService.GetTransactionsOverview(now.ToUniversalTime().AddDays(period * -1), now.ToUniversalTime());

            var hash = Hash.FromAnonymousObject(new {
                customer = this.appConfig.GetClientKey(),
                reporturl = this.redirectService.GetActionUrl("TransactionStatsReport", "GetReport", queryString),
                logincount = transactionOverview.Item1,
                totaltx = transactionOverview.Item2,
                periodfrom = periodFrom,
                periodto = periodTo
            });            

            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);
            var emailBody = template.Render(hash);

            var sendTo = configurationFacade.Lookup<string>(ConfigurationConstants.MetadataChangeReportEmailId);

            var email = new EmailData(NoReplySendFrom, 
                sendTo, 
                string.Format("[softWrench {0} - {1}] Transaction Statistics Report", this.appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                emailBody, 
                null);

            this.emailService.SendEmail(email);
        }        
    }
}
