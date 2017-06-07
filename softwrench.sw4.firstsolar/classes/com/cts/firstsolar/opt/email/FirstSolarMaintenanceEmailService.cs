using System;
using System.Collections.Generic;
using System.Linq;
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
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarMaintenanceEmailService : FirstSolarBaseEmailService {

        public FirstSolarMaintenanceEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }

        public override string GenerateEmailBody(IFsEmailRequest request, WorkPackage package, string siteId) {
            var engRequest = request as MaintenanceEngineering;

            BuildTemplate();

            var user = SecurityFacade.CurrentUser();
            var woId = package.WorkorderId;
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();
            var woData = AsyncHelper.RunSync(() => dataset.GetWorkorderRelatedData(user, woId));

            var acceptUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=approved".Fmt(engRequest.Token));
            var rejectUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=rejected".Fmt(engRequest.Token));
            var pendingUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=pending".Fmt(engRequest.Token));
            var packageUrl = RedirectService.GetApplicationUrl("_WorkPackage", "adetail", "input", package.Id + "");

            var tier = "";
            if (!string.IsNullOrEmpty(package.Tier)) {
                tier = "Tier " + package.Tier.Substring(package.Tier.Length - 1);
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

            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    acceptUrl,
                    rejectUrl,
                    pendingUrl,
                    packageUrl,
                    wonum = package.Wonum,
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
                    engineer = engRequest.Engineer,
                    reason = engRequest.Reason
                }));
            return msg;
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//maintenanceengineeremail.html";
        }

        protected override string GetEmailSubjectMsg(IFsEmailRequest request, WorkPackage package, string siteId) {
            var me = request as MaintenanceEngineering;
            return me == null ? "[First Solar] Maintenance Engineering Request" : 
                "[First Solar] Maintenance Engineering Request ({0}, {1})".Fmt(FmtDate(package.CreatedDate), siteId);
        }

        public override string RequestI18N() {
            return "Maintenance Engineering";
        }

        protected override string GetSendTo(IFsEmailRequest request, WorkPackage package, string siteId) {
            var isProdOrUat = ApplicationConfiguration.Profile.Contains("uat") || ApplicationConfiguration.Profile.Contains("prod");
            return isProdOrUat ? request.Email + ",omengineering@firstsolar.com" : request.Email;
        }
    }
}
