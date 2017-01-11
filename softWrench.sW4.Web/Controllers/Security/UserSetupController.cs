using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using cts.commons.persistence;
using cts.commons.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Util;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.exceptions;
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
        private readonly SecurityFacade _facade;

    

        public UserSetupController(UserManager userManager, SecurityFacade facade) {
            _userManager = userManager;
            _facade = facade;
        }


        /// <summary>
        /// This action is called upon user activation after he receives an email with a link to login into the system, and clicks that link
        /// </summary>
        /// <param name="tokenLink"></param>
        /// <returns></returns>
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
                Title = "Define Password",
                ChangePasswordScenario = false,
                Token = tokenLink,
                FullName = user.FullName,
                Username = user.UserName
            });
        }

        /// <summary>
        /// This actions is called after the user received the link, and has choosen a new password.
        /// 
        /// If the new password is accepted it will be stored and the user will be redirected to the system, logged in.
        /// 
        /// </summary>
        /// <param name="tokenLink"></param>
        /// <param name="password"></param>
        /// <param name="userTimezoneOffset"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [SPFRedirect(URL = "DefinePassword")]
        public async Task<ActionResult> DoSetPassword(string tokenLink, string password, string userTimezoneOffset) {
            Validate.NotNull(tokenLink, "tokenLink");
            Validate.NotNull(password, "password");
            bool hasExpired;
            var user = _userManager.FindUserByLink(tokenLink, out hasExpired);
            //context cannot be invoked inside async method
            var context = System.Web.HttpContext.Current;
            if (user == null) {
                throw new SecurityException(WrongLinkException);
            }

            if (hasExpired) {
                throw new SecurityException(ExpiredLinkException);
            }
            try {
                await _userManager.ActivateAndDefinePassword(user, password);
                await AfterPasswordSet(user, password, userTimezoneOffset,context);
            } catch (PasswordException.PasswordHistoryException) {
                return View("DefinePassword", new DefinePasswordModel {
                    RepeatedPassword = true,
                    Title = "Define Password",
                    ChangePasswordScenario = false,
                    Token = tokenLink,
                    FullName = user.FullName,
                    Username = user.UserName
                });
            }

            return null;
        }

        /// <summary>
        /// This actions handles the scenario where the user successfully logged in but was asked to change his password due to the changepassword policy out of the configuration system.
        /// 
        /// This is the initial action that will redirect him to the form
        /// 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Authorize]
        [System.Web.Http.HttpGet]
        public async Task<ActionResult> ChangePassword() {
            var user = SecurityFacade.CurrentUser();

            // avoids a changing the password without the flag set
            if (!await VerifyChangePassword(user)) {
                return null;
            }

            return View("DefinePassword", new DefinePasswordModel {
                Title = "Expired Password",
                ChangePasswordScenario = true,
                FullName = user.FullName,
                Username = user.Login
            });
        }

        [System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        [SPFRedirect(URL = "ChangePassword")]
        public async Task<ActionResult> DoChangePassword([FromBody]string password, [FromUri]string userTimezoneOffset) {
            Validate.NotNull(password, "password");
            var memoryUser = SecurityFacade.CurrentUser();

            // avoids a changing the password without the flag set
            if (!await VerifyChangePassword(memoryUser)) {
                return null;
            }
            //context cannot be invoked inside async method
            var context = System.Web.HttpContext.Current;
            var user = memoryUser.DBUser;
            try {
                await _userManager.ActivateAndDefinePassword(user, password);
                await AfterPasswordSet(user, password, userTimezoneOffset, context);
            } catch (PasswordException.PasswordHistoryException) {
                return View("DefinePassword", new DefinePasswordModel {
                    Title = "Expired Password",
                    ChangePasswordScenario = true,
                    FullName = user.FullName,
                    Username = user.UserName,
                    RepeatedPassword = true
                });
            }
            return null;
        }

        [System.Web.Http.Authorize]
        [System.Web.Http.HttpPost]
        public JObject VerifySamePassword(string password) {
            var user = SecurityFacade.CurrentUser();
            var samePassword = AuthUtils.GetSha1HashData(password).Equals(user.DBUser.Password);
            var json = new JObject();
            json["samePassword"] = samePassword.ToString().ToLower();
            return json;
        }

        private async Task<bool> VerifyChangePassword(InMemoryUser user) {
            // avoids a changing the password without the flag set
            if (await _userManager.VerifyChangePassword(user)) {
                return true;
            }
            Response.Redirect(FormsAuthentication.GetRedirectUrl(user.Login, false));
            return false;
        }

        private async Task AfterPasswordSet(SwUser user, string password, string userTimezoneOffset, HttpContext context) {
            var inMemoryUser = await _facade.LoginCheckingPassword(user, password, userTimezoneOffset);
            //logining in the user and redirecting him to home page

            AuthenticationCookie.SetSessionCookie(user.UserName, userTimezoneOffset, Response);

            System.Web.HttpContext.Current = context; // async method run in another thread so this is needed
            FormsAuthentication.RedirectFromLoginPage(user.UserName, false);

            Thread.CurrentPrincipal = inMemoryUser;
        }

        protected override void OnException(ExceptionContext filterContext) {
            var exception = filterContext.Exception;
            if (!(exception is SecurityException)) {
                base.OnException(filterContext);
                return;
            }

            filterContext.HttpContext.Response.Clear();

            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult() {
                ViewName = "~/Views/UserSetup/Error.cshtml",
                ViewData = new ViewDataDictionary(new UserSetupError(exception))
            };

            filterContext.HttpContext.Response.End();
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
        private UserLinkManager _userLinkManager;
        private readonly ISWDBHibernateDAO _swdao;
        private readonly IMaximoHibernateDAO _maxdao;

        private const string application = "person";
        private const string schemaId = "newPersonDetail";

        public UserSetupWebApiController(UserManager userManager, UserSetupEmailService userSetupEmailService,
            ISWDBHibernateDAO swdao, IMaximoHibernateDAO maxdao) {
            _userManager = userManager;
            _userSetupEmailService = userSetupEmailService;
            _swdao = swdao;
            _maxdao = maxdao;
        }

        [System.Web.Http.HttpPost]
        public async Task<IGenericResponseResult> ForgotPassword([FromUri]string userNameOrEmail) {
            Validate.NotNull(userNameOrEmail, "userNameOrEmail");
            var exception = await _userManager.ForgotPassword(userNameOrEmail);
            if (exception != null) {
                throw new SecurityException(exception);
            }
            return null;
        }

        [System.Web.Http.HttpPost]
        public async Task<IGenericResponseResult> SendActivationEmail([FromUri]int userId, [FromUri]string email) {
            await _userManager.SendActivationEmail(userId, email);
            return null;
        }


        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public async Task ForceResetPassword([FromUri]string username) {
            await _userManager.ForcePasswordReset(username);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public async Task UnLock([FromUri]string username) {
            await _userManager.Unlock(username);
        }


        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public async Task<UserActivationLink> Activate([FromUri]int userId) {
            return await _userManager.Activate(userId);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public async Task Inactivate([FromUri]int userId) {
            await _userManager.InActivate(userId);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        public async Task<IGenericResponseResult> NewUserRegistration([FromBody]JObject json) {
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
            var personDataSet = DataSetProvider.GetInstance().LookupDataSet(application, schemaId);
            await personDataSet.Execute(applicationMetadata, json, operationRequest);

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
                .Cast<IDictionary<string, object>>()
                .Select(e => e["emailaddress"].ToString());
        }

    }

}
