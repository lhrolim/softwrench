using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using cts.commons.persistence;
using cts.commons.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sw4.Shared2.Util;
using softwrench.sw4.user.classes.services.setup;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.UserSetup;
using SecurityException = System.Security.SecurityException;
using SwUser = softwrench.sw4.user.classes.entities.User;

namespace softWrench.sW4.Web.Controllers.Security {

    /// <summary>
    /// A user controller which contains methods specifically oriented for setup, that do not require any authentication.
    /// </summary>
    public class UserSetupController : Controller {

        private const string WrongLinkException = "The link you are trying to access is not valid: Either the user is already active or the link provided is incorrect.";
        private const string ExpiredLinkException = "The link has expired, please contact your administrator and ask for a new link";

        private readonly UserManager _userManager;

        public UserSetupController(UserManager userManager) {
            _userManager = userManager;
        }

        [System.Web.Http.HttpGet]
        public ActionResult DefinePassword(string tokenLink) {
            Validate.NotNull(tokenLink, "tokenLink");
            bool hasExpired;
            var user = _userManager.FindUserByLink(tokenLink, out hasExpired);
            if (user == null) {
                throw new SecurityException(WrongLinkException);
            }
            if (hasExpired) {
                throw new SecurityException(ExpiredLinkException);
            }
            return View(new DefinePasswordModel {
                Token = tokenLink,
                FullName = user.FullName
            });
        }


        [System.Web.Http.HttpPost]
        [SPFRedirect(URL = "DefinePassword")]
        public ActionResult DoSetPassword(string tokenLink, string password, string userTimezoneOffset) {
            Validate.NotNull(tokenLink, "tokenLink");
            Validate.NotNull(password, "password");
            bool hasExpired;
            var user = _userManager.FindUserByLink(tokenLink, out hasExpired);
            if (user == null) {
                throw new SecurityException(WrongLinkException);
            }

            if (hasExpired) {
                throw new SecurityException(ExpiredLinkException);
            }
            _userManager.ActivateAndDefinePassword(user, password);

            //logining in the user and redirecting him to home page
            var inMemoryUser = SecurityFacade.GetInstance().Login(user, password, userTimezoneOffset);
            AuthenticationCookie.SetSessionCookie(user.UserName, userTimezoneOffset, Response);
            FormsAuthentication.RedirectFromLoginPage(user.UserName, false);
            Thread.CurrentPrincipal = inMemoryUser;

            return null;
        }

        protected override void OnException(ExceptionContext filterContext) {
            var exception = filterContext.Exception;
            if (!(exception is SecurityException)) {
                base.OnException(filterContext);
                return;
            }
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult() {
                ViewName = "~/Views/UserSetup/Error.cshtml",
                ViewData = new ViewDataDictionary(new UserSetupError(exception))
            };
        }
    }

    /// <summary>
    /// Same idea, but using webapi for AJAX calls...
    /// 
    /// TODO: unify both
    /// </summary>
    public class UserSetupWebApiController : ApiController {

        private readonly UserManager _userManager;
        private readonly UserSetupEmailService _userSetupEmailService;
        private readonly ISWDBHibernateDAO _swdao;
        private readonly IMaximoHibernateDAO _maxdao;

        public UserSetupWebApiController(UserManager userManager, UserSetupEmailService userSetupEmailService, 
            ISWDBHibernateDAO swdao, IMaximoHibernateDAO maxdao) {
            _userManager = userManager;
            _userSetupEmailService = userSetupEmailService;
            _swdao = swdao;
            _maxdao = maxdao;
        }

        [System.Web.Http.HttpPost]
        public IGenericResponseResult ForgotPassword([FromUri]string userNameOrEmail) {
            Validate.NotNull(userNameOrEmail, "userNameOrEmail");
            var exception =_userManager.ForgotPassword(userNameOrEmail);
            if (exception != null){
                throw new SecurityException(exception);
            }
            return null;
        }

        [System.Web.Http.HttpPost]
        public IGenericResponseResult SendActivationEmail([FromUri]int userId, [FromUri]string email) {
            _userManager.SendActivationEmail(userId, email);
            return null;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        public IGenericResponseResult NewUserRegistration([FromBody]JObject json) {
            var approvers = _swdao.FindByQuery<SwUser>(SwUser.UserByProfile, "approver");
            if (!approvers.Any()) {
                throw new InvalidOperationException(
                    "Registration can't be processed: There are no approvers registered in the system. Please contact your system administrator or support team.");
            }
            var personIds = approvers.Select(a => a.UserName.ToUpper());
            var approverEmails = PrimaryEmails(personIds).Where(e => !string.IsNullOrEmpty(e)).ToList();

            if (!approverEmails.Any()) {
                throw new InvalidOperationException(
                    "Registration can't be processed: There are no approvers with a primary email registered in the system. Please contact your system administrator or support team.");
            }

            const string application = "person";
            const string schemaId = "newPersonDetail";
            
            // pre-signin action --> use anonymous user
            var user = InMemoryUser.NewAnonymousInstance(); 

            // create user
            var username = json.GetValue("personid").Value<string>();
            var operationRequest = new OperationDataRequest {
                ApplicationName = application,
                Id = username,
                MockMaximo = MockingUtils.IsMockingMaximoModeActive(json),
                Operation = OperationConstants.CRUD_CREATE,
                Platform = ClientPlatform.Web
            };
            var schemaKey = SchemaUtil.GetSchemaKeyFromString(schemaId, ClientPlatform.Web);
            var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(schemaKey, user, ClientPlatform.Web);
            DataSetProvider.GetInstance()
                .LookupDataSet(application, schemaId)
                .Execute(applicationMetadata, json, operationRequest);

            // request user activation to the approvers
            var firstname = json.GetValue("firstname").Value<string>();
            var lastname = json.GetValue("lastname").Value<string>();
            _userSetupEmailService.NewUserApprovalRequestEmail(username, firstname, lastname, approverEmails);

            // send notification email to the user 
            // (fire-and-forget for performance: Since the user will receive the message in the response anyway --> failure is an option)
            var email = json.GetValue("#primaryemail").Value<string>();
            const string notification = "Your registration request has been submitted. You will receive an activation email shortly.";
            _userSetupEmailService.GenericMessageEmail(email, "[softWrench] Registration", notification, true);

            return new BlankApplicationResponse() {
                SuccessMessage = notification
            };
        }

        private IEnumerable<string> PrimaryEmails(IEnumerable<string> personIds) {
            var personIdsList = personIds as List<string> ?? personIds.ToList();
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("personIds", personIdsList));

            return _maxdao.FindByNativeQuery(@"select emailaddress 
                                                from email 
                                                where emailaddress is not null and isprimary = 1 and personid in (:personIds)", 
                                                parameters)
                .Cast<IDictionary<string,object>>()
                .Select(e => e["emailaddress"].ToString());
        }

    }

}