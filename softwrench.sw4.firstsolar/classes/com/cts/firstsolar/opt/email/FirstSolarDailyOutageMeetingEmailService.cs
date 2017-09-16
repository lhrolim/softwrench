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
using NHibernate.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.PDF;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarDailyOutageMeetingEmailService : FirstSolarBaseEmailService<DailyOutageMeeting> {


        private new static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarDailyOutageMeetingEmailService));

        private const string EmailTemplate = "//Content//Customers//firstsolar//htmls//templates//meeting.html";
        private static readonly string PdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "//Content//Customers//firstsolar//htmls//templates//meetingpdf.html";


        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        [Import]
        public PdfService PdfService { get; set; }

        [Import]
        public FirstSolarCustomGlobalFedService GFedService { get; set; }

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
                Cc = emailData.Cc,
                WorkPackage = package
            };

            package.EmailStatuses.Add(await Dao.SaveAsync(emailStatus));

            dom.Status = RequestStatus.Sent;
            dom.ActualSendTime = DateTime.Now;

            return await Dao.SaveAsync(dom);
        }

        protected override string GetTemplatePath() {
            return EmailTemplate;
        }

        public override string RequestI18N() {
            return "Daily Outage Meeting";
        }

        protected override EmailData BuildEmailData(DailyOutageMeeting dom, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var to = ConfigurationFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultDailyOutageMeetingToEmailKey);
            if (string.IsNullOrEmpty(to) && !ApplicationConfiguration.IsProd()) {
                Log.WarnFormat("no daily outage email setup on the configuration section. Returning");
                return null;
            }

            // 'to' not set on config on prod load the emails from gfed
            if (string.IsNullOrEmpty(to)) {
                to = dom.Email;
            }

            var isNew = dom.ActualSendTime == null;
            var baseSubject = "[{0}] Daily Outage Meeting".Fmt(package.Wpnum);
            var sufix = isNew ? "" : " Updated";
            var subject = "{0}{1}".Fmt(baseSubject, sufix);

            var hash = BuildTemplateHash(dom, package);

            var msg = GenerateEmailBody(dom, package, siteId, hash);

            if (attachs == null) {
                attachs = new List<EmailAttachment>();
            }
            attachs.Add(BuildPdfReport(hash));

            var emailData = new EmailData(GetFrom(), to, subject, msg, attachs) { Cc = dom.Cc, BCc = "support@controltechnologysolutions.com" };
            return emailData;
        }

        public string GenerateEmailBody(DailyOutageMeeting dom, WorkPackage package, string siteId, Hash hash = null) {
            BuildTemplate();
            return Template.Render(hash ?? BuildTemplateHash(dom, package));
        }

        public string BuildPdfHtml(Hash hash) {
            var pdfTemplate = BuildTemplate(PdfTemplate);
            return pdfTemplate.Render(hash);
        }

        private EmailAttachment BuildPdfReport(Hash hash) {
            var pdfHtml = BuildPdfHtml(hash);
            var title = "Daily Outage Meeting - " + hash["facilityname"] + " - " + hash["today"];
            var rodayWithDashes = DateTime.Now.ToString("MM-dd-yy", new CultureInfo("en-US"));
            var fileName = "DailyOutageMeeting_" + hash["facilityname"] + "_" + rodayWithDashes + ".pdf";
            return EmailService.CreateAttachment(PdfService.HtmlToPdf(pdfHtml, title), fileName);
        }

        public Hash BuildTemplateHash(DailyOutageMeeting dom, WorkPackage package) {
            var woData = GetWoData(package);

            var actions = new List<object>();
            var even = false;
            package.OutageActions?.ForEach(action => {
                actions.Add(new {
                    action = action.Action,
                    actiontime = FmtDateTime(action.ActionTime),
                    assignee = action.AssigneeLabel,
                    completed = action.Completed,
                    even
                });
                even = !even;
            });

            return Hash.FromAnonymousObject(new {
                today = SafePlaceholder(FmtDate(DateTime.Now)),
                facilityname = SafePlaceholder(package.FacilityName),
                wosummary = SafePlaceholder(woData.GetStringAttribute("description")),
                outagestartdate = SafePlaceholder(FmtDateTime(package.CreatedDate)),
                estimatedcompletiondate = SafePlaceholder(FmtDateTime(package.EstimatedCompDate)),
                actualcompletiondate = SafePlaceholder(FmtDateTime(package.ActualCompDate)),
                mwhlosttotal = SafePlaceholder(package.MwhLostTotal),
                expectedmwhlost = SafePlaceholder(package.ExpectedMwhLost),
                mwhlostperday = SafePlaceholder(package.MwhLostPerDay),
                problemstatement = SafePlaceholder(package.ProblemStatement),
                meetingtime = SafePlaceholder(FmtDateTime(dom.MeetingTime)),
                mwhlost = SafePlaceholder(dom.MWHLostYesterday.ToString("0", new CultureInfo("en-US"))),
                criticalpath = SafePlaceholder(dom.CriticalPath),
                openactionitems = SafePlaceholder(dom.OpenActionItems),
                completedactionitems = SafePlaceholder(dom.CompletedActionItems),
                meetingsummary = SafePlaceholder(dom.Summary),
                wpnum = SafePlaceholder(package.Wpnum),
                actions,
//                workpackageurl = SafePlaceholder(RedirectService.GetActionUrl("FirstSolarWpGenericEmail", "DailyOutageView", "token={0}".Fmt(dom.Token)))
                workpackageurl = SafePlaceholder(RedirectService.GetApplicationUrlRoute("_workpackage", dom.WorkPackage.Id.Value, "dailyoutage"))
            });
        }
    }
}
