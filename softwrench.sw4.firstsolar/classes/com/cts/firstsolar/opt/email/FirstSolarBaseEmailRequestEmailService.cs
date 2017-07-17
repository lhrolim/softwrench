using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public abstract class FirstSolarBaseEmailRequestEmailService : FirstSolarBaseEmailService<IFsEmailRequest> {

        protected FirstSolarBaseEmailRequestEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade)
            : base(emailService, redirectService, appConfig, configurationFacade) {
            Log.Debug("init Log");

        }

        public override async Task<IFsEmailRequest> SendEmail(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            Validate.NotNull(request, "toSend");
            Log.InfoFormat("sending {0} email for {1} to {2}", RequestI18N(), request.Id, request.Email);

            var emailData = BuildEmailData(request, package, siteId, attachs);
            EmailService.SendEmail(emailData);

            request.Status = RequestStatus.Sent;
            request.ActualSendTime = DateTime.Now;

            return await Dao.SaveAsync(request);
        }

   
        public abstract void HandleReject(IFsEmailRequest request, WorkPackage package);
       
    }
}
