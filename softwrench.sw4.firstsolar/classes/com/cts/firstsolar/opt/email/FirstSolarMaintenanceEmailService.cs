using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using DotLiquid;
using NHibernate.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarMaintenanceEmailService : FirstSolarBaseEmailRequestEmailService {

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        private const string SupervisorEmailQuery = "select emailaddress from email where personid = ?";

        private Template _rejectTemplate;

        public FirstSolarMaintenanceEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade) : base(emailService, redirectService, appConfig, configurationFacade) {
            Log.Debug("init Log");
        }

        protected override EmailData BuildEmailData(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var to = ApplicationConfiguration.IsProd() ? request.Email + ",omengineering@firstsolar.com" : request.Email;

            var me = request as MaintenanceEngineering;
            var subject = me == null ? "[First Solar] Maintenance Engineering Request" :
                "[First Solar] Maintenance Engineering Request ({0}, {1})".Fmt(FmtDate(package.CreatedDate), siteId);

            var woData = GetWoData(package);
            var msg = GenerateEmailBody(request, package, siteId, woData);
            var emailData = new EmailData(GetFrom(), to, subject, msg, attachs);

            if (!string.IsNullOrEmpty(package.InterConnectDocs) && !"na".Equals(package.InterConnectDocs) && ApplicationConfiguration.IsProd()) {
                emailData.Cc = "fsocoperator@firstsolar.com";
            }

            return emailData;
        }

        public string GenerateRejectEmailBody(MaintenanceEngineering me, WorkPackage package, string siteId = null, AttributeHolder woData = null) {
            if (_rejectTemplate == null || AppConfig.IsLocal()) {
                var path = AppDomain.CurrentDomain.BaseDirectory + "//Content//Customers//firstsolar//htmls//templates//maintenanceengrejectemail.html";
                var templateContent = File.ReadAllText(path);
                _rejectTemplate = Template.Parse(templateContent);
            }
            return _rejectTemplate.Render(GenerateEmailHash(me, package, siteId, woData));
        }

        public override void HandleReject(IFsEmailRequest request, WorkPackage package) {
            if (!ApplicationConfiguration.IsProd()) {
                return;
            }

            var me = request as MaintenanceEngineering;
            var woData = GetWoData(package);
            var to = GetSupervisorEmail(woData);

            if (string.IsNullOrEmpty(to)) {
                return;
            }

            var siteId = woData.GetStringAttribute("siteid");
            var subject = "[First Solar] Maintenance Engineering Request Rejected ({0}, {1})".Fmt(FmtDate(package.CreatedDate), siteId);
            var msg = GenerateRejectEmailBody(me, package, siteId, woData);
            var emailData = new EmailData(GetFrom(), to, subject, msg);
            EmailService.SendEmail(emailData);
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//maintenanceengineeremail.html";
        }

        public override string RequestI18N() {
            return "Maintenance Engineering";
        }

        private Hash GenerateEmailHash(MaintenanceEngineering me, WorkPackage package, string siteId = null, AttributeHolder woData = null) {
            var acceptUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=approved".Fmt(me.Token));
            var rejectUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=rejected".Fmt(me.Token));
            var pendingUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=pending".Fmt(me.Token));
            var packageUrl = RedirectService.GetApplicationUrl("_WorkPackage", "adetail", "input", package.Id + "");

            var tier = "";
            if (!string.IsNullOrEmpty(package.Tier)) {
                tier = "Tier " + package.Tier.Substring(package.Tier.Length - 1);
            }

            if (woData == null) {
                woData = GetWoData(package);
            }
            if (siteId == null) {
                siteId = woData.GetStringAttribute("siteid");
            }

            var woOutReq = woData.GetIntAttribute("outreq");
            var outagereq = woOutReq != null && woOutReq.Value > 0 ? "yes" : "no";

            var outageTypeList = new List<string>();
            if (package.OutagesList != null && package.OutagesList.Any()) {
                package.OutagesList.ForEach(gen => {
                    switch (gen.Value) {
                        case "1":
                            outageTypeList.Add("1. Intercompany Outage");
                            break;
                        case "2":
                            outageTypeList.Add("2. Curtailment/Grid Control Outage");
                            break;
                        case "3":
                            outageTypeList.Add("3. Change Control Outage");
                            break;
                        case "4":
                            outageTypeList.Add("4. Production/Relay Outage");
                            break;
                        case "5":
                            outageTypeList.Add("5. Telemetry Outage");
                            break;
                        case "6":
                            outageTypeList.Add("6. Met-Station Outage");
                            break;
                        case "7":
                            outageTypeList.Add("7. Non-Coordinated Outage");
                            break;
                    }
                });
            }
            var outagetype = string.Join(", ", outageTypeList);


            var interdocs = "N/A";
            switch (package.InterConnectDocs) {
                case "switching":
                    interdocs = "Switching Order (ONLY)";
                    break;
                case "source":
                    interdocs = "Source of Power Clearance";
                    break;
            }

            return Hash.FromAnonymousObject(new
            {
                acceptUrl,
                rejectUrl,
                pendingUrl,
                packageUrl,
                wpnum = package.Wpnum,
                summary = woData.GetAttribute("description"),
                description = woData.GetAttribute("ld_.ldtext"),
                worktype = woData.GetAttribute("worktype"),
                siteid = siteId,
                schedstart = FmtDate(woData.GetAttribute("schedstart") as DateTime?),
                tier,
                outagereq,
                outagetype,
                interdocs,
                operationprocedured = package.OutageType,
                headerurl = GetHeaderURL(),
                engineer = me.Engineer,
                reason = me.Reason
            });
        }

        public string GenerateEmailBody(IFsEmailRequest request, WorkPackage package, string siteId, AttributeHolder woData = null) {
            var engRequest = request as MaintenanceEngineering;
            BuildTemplate();
            return Template.Render(GenerateEmailHash(engRequest, package, siteId, woData));
        }

        private static DataMap GetWoData(WorkPackage package) {
            var user = SecurityFacade.CurrentUser();
            var woId = package.WorkorderId;
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();
            return AsyncHelper.RunSync(() => dataset.GetWorkorderRelatedData(user, woId));
        }

        private string GetSupervisorEmail(AttributeHolder woData) {
            var supervisor = woData.GetStringAttribute("supervisor");
            if (string.IsNullOrEmpty(supervisor)) {
                return null;
            }
            var emails = MaxDao.FindByNativeQuery(SupervisorEmailQuery, supervisor);
            if (emails == null || !emails.Any()) {
                return null;
            }

            var emailRow = emails.First();
            return emailRow["emailaddress"];
        }
    }
}
