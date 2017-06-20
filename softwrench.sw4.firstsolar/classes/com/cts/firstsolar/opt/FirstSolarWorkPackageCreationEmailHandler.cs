using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {

    public class FirstSolarWorkPackageCreationEmailHandler : FirstSolarBaseEmailService {

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        public FirstSolarWorkPackageCreationEmailHandler(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade)
            : base(emailService, redirectService, appConfig, configurationFacade) {
        }


        public async Task SendCreationEmails(WorkPackage wp) {
            if (!string.IsNullOrEmpty(wp.InterConnectDocs)) {
                var toEmail = await ConfigFacade.LookupAsync<string>(FirstSolarOptConfigurations.DefaultOptInterOutageToEmailKey);
                if (string.IsNullOrEmpty(toEmail)) {
                    return;
                }



            }

        }



        public override void HandleReject(IFsEmailRequest request, WorkPackage package) {
            // nothing done on wp reject
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//workpackageemail.html";
        }

        public override string RequestI18N() {
            return "Work Package";
        }

        protected override EmailData BuildEmailData(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            throw new NotImplementedException();
        }
    }
}
