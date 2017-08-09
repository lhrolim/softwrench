using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util.TransactionStatistics.Email {
    public class TransactionStatsEmailer : ISingletonComponent {
        private const string MailSubject = "Transaction Statistics Report";
        private readonly IEmailService _emailService;
        private readonly RedirectService _redirectService;
        private readonly IApplicationConfiguration _appConfig;
        private readonly TransactionStatisticsService _txService;
        private readonly IConfigurationFacade _configurationFacade;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStatsEmailer"/> class.
        /// </summary>
        /// <param name="emailService">The email service reference</param>
        /// <param name="redirectService"></param>
        public TransactionStatsEmailer(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, TransactionStatisticsService txService, IConfigurationFacade configurationFacade) {
            _emailService = emailService;
            _redirectService = redirectService;
            _appConfig = appConfig;
            _txService = txService;
            _configurationFacade = configurationFacade;
        }

        /// <summary>
        /// Email the metadata file that has been changed.
        /// </summary>        
        public void SendEmail() {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//txstatisticsreporttemplate.html";
            var period = _configurationFacade.Lookup<int>(ConfigurationConstants.TransactionStatsReportDuration);
            var now = DateTime.Now;
            var periodFrom = now.AddDays(period * -1).ToString(CultureInfo.InvariantCulture);
            var periodTo = now.ToString(CultureInfo.InvariantCulture);

            var queryString = $"fromDateFilter={periodFrom}&toDateFilter={periodTo}";

            var transactionOverview = _txService.GetTransactionsOverview(now.ToUniversalTime().AddDays(period * -1), now.ToUniversalTime());

            var hash = Hash.FromAnonymousObject(new {
                customer = _appConfig.GetClientKey(),
                reporturl = _redirectService.GetActionUrl("TransactionStatsReport", "GetReport", queryString),
                logincount = transactionOverview.Item1,
                totaltx = transactionOverview.Item2,
                periodfrom = periodFrom,
                periodto = periodTo
            });            

            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);
            var emailBody = template.Render(hash);

            var sendTo = _configurationFacade.Lookup<string>(ConfigurationConstants.MetadataChangeReportEmailId);
            var sendFrom = _configurationFacade.Lookup<string>(ConfigurationConstants.Email.DefaultFromEmail);

            var email = new EmailData(sendFrom, 
                sendTo,
                $"[softWrench {_appConfig.GetClientKey()} - {ApplicationConfiguration.Profile}] Transaction Statistics Report",
                emailBody);

            _emailService.SendEmail(email);
        }        
    }
}
