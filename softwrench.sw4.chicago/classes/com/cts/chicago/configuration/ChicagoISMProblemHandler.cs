using System;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Email;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration {
    public class ChicagoISMProblemHandler : IProblemHandler {

        public const string HandlerName = "ismrestsync";

        private readonly EmailService _emailService;
        private readonly IConfigurationFacade _configurationFacade;

        public ChicagoISMProblemHandler(IConfigurationFacade configurationFacade, EmailService emailService) {
            _configurationFacade = configurationFacade;
            _emailService = emailService;
        }

        public void OnProblemRegister(Problem problem) {
            var sendTo = _configurationFacade.Lookup<string>(ChicagoConfigurationSetup.ProblemEmails);
            if (string.IsNullOrEmpty(sendTo)) {
                return;
            }
            var emailData = new EmailData("donotreply@controltechnologysolutions.com", sendTo, "[Chicago Problem]", "");
            _emailService.SendEmailAsync(emailData);
        }

        public void OnProblemSolved() {

        }

        public bool DelegateToMainApplication() {
            return true;
        }

        public string ProblemHandler() {
            return HandlerName;
        }

        public virtual string ApplicationName() {
            return "servicerequest";
        }

        public string ClientName() {
            return "chicago";
        }
    }
}
