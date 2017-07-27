using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarDailyOutageMeetingEmailService : FirstSolarBaseEmailService<DailyOutageMeeting> {


        private new static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarDailyOutageMeetingEmailService));

        [Import]
        public SWDBHibernateDAO DAO {
            get; set;
        }

        public FirstSolarDailyOutageMeetingEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade) : base(emailService, redirectService, appConfig, configurationFacade) {
            Log.Debug("init Log");
        }

        public override async Task<DailyOutageMeeting> SendEmail(DailyOutageMeeting dom, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            Validate.NotNull(dom, "toSend");
            var emailData = BuildEmailData(dom, package, siteId, attachs);
            if (emailData == null) {
                Log.InfoFormat("failed to sent {0} email for {1} to {2}, probably missing '/FirstSolar/OPT/DefaultDailyOutageMeetingToEmail' config", RequestI18N(), dom.Id);
                dom.Status = RequestStatus.Error;
                return await Dao.SaveAsync(dom);
            }

            Log.InfoFormat("sending {0} email for {1} to {2}", RequestI18N(), dom.Id, emailData.SendTo);

            EmailService.SendEmail(emailData);

            var emailStatus = new WorkPackageEmailStatus {
                Email = emailData.SendTo,
                Operation = softWrench.sW4.Data.Persistence.Operation.OperationConstants.CRUD_UPDATE,
                Qualifier = "dailyoutagemeeting",
                SendDate = DateTime.Now,
                WorkPackage = package
            };

            package.EmailStatuses.Add(await Dao.SaveAsync(emailStatus));

            dom.Status = RequestStatus.Sent;
            dom.ActualSendTime = DateTime.Now;

            return await Dao.SaveAsync(dom);
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//meeting.html";
        }

        public override string RequestI18N() {
            return "Daily Outage Meeting";
        }

        protected override EmailData BuildEmailData(DailyOutageMeeting dom, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var to = ConfigurationFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultDailyOutageMeetingToEmailKey);
            if (string.IsNullOrEmpty(to)) {
                Log.WarnFormat("no daily outage email setup on the configuration section. Returning");
                return null;
            }

            var isNew = dom.ActualSendTime == null;
            var baseSubject = "[{0}] Daily Outage Meeting".Fmt(package.Wpnum);
            var sufix = isNew ? "" : " Updated";
            var subject = "{0}{1}".Fmt(baseSubject, sufix);
            var msg = GenerateEmailBody(dom, package, siteId);
            var emailData = new EmailData(GetFrom(), to, subject, msg, attachs) { Cc = dom.Cc };
            return emailData;
        }

        public string GenerateEmailBody(DailyOutageMeeting dom, WorkPackage package, string siteId) {

            BuildTemplate();

            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    headerurl = GetHeaderURL(),
                    outagestartdate = FmtDate(package.CreatedDate),
                    estimatedcompletiondate = FmtDate(package.EstimatedCompDate),
                    actualcompletiondate = FmtDate(package.ActualCompDate),
                    mwhlosttotal = package.MwhLostTotal,
                    expectedmwhlost = package.ExpectedMwhLost,
                    mwhlostperday = package.MwhLostPerDay,
                    problemstatement = package.ProblemStatement,
                    meetingtime = FmtDate(dom.MeetingTime),
                    mwhlost = dom.MWHLostYesterday.ToString("0", new CultureInfo("en-US")),
                    criticalpath = dom.CriticalPath,
                    openactionitems = dom.OpenActionItems,
                    completedactionitems = dom.CompletedActionItems,
                    meetingsummary = dom.Summary,
                    wpnum = package.Wpnum,
                    workpackageurl = RedirectService.GetActionUrl("FirstSolarWpGenericEmail", "DailyOutageView", "token={0}".Fmt(dom.Token))
                }));
            return msg;
        }
    }
}
