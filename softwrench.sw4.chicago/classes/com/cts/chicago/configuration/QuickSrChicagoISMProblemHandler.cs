using System;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Email;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration {
    public class QuickSrChicagoISMProblemHandler : ChicagoISMProblemHandler {

        public QuickSrChicagoISMProblemHandler(IConfigurationFacade configurationFacade, EmailService emailService) : base(configurationFacade, emailService) {
        }



        public override string ApplicationName() {
            return "quickservicerequest";
        }



    }
}
