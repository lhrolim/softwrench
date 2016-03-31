using JetBrains.Annotations;
using softWrench.sW4.AUTH;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.LoginHandler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Web.Controllers.Security;

namespace softWrench.sW4.Web.Controllers {
    public class SignInController : Controller {
        private readonly IConfigurationFacade _facade;
        private readonly LdapManager _ldapManager;
        private readonly SWDBHibernateDAO _dao;


        public SignInController(IConfigurationFacade facade, LdapManager ldapManager, SWDBHibernateDAO dao) {
            _facade = facade;
            _ldapManager = ldapManager;
            _dao = dao;
        }

        public ActionResult Index(bool timeout = false, bool forbidden = false) {
            if (User.Identity.IsAuthenticated) {
                Response.Redirect(FormsAuthentication.GetRedirectUrl(User.Identity.Name, false));
            }

            LoginHandlerModel model;
            string loginMessage = null;
            if (!IsLoginEnabled(ref loginMessage)) {
                model = new LoginHandlerModel(false, loginMessage, ClientName());
            } else {
                model = new LoginHandlerModel(true, IsHapagClient(), ClientName());
            }
            model.Inactivity = timeout;
            model.Forbidden = forbidden;
            return View(model);
        }

        private static bool IsLoginEnabled([CanBeNull] ref string loginMessage) {
            var enabled = true;

            var path = ApplicationConfiguration.ServiceItLoginPath;
            if (string.IsNullOrEmpty(path)) {
                return true;
            }
            string loginMessageAux = null;
            if (System.IO.File.Exists(@path)) {
                var data = new Dictionary<string, string>();
                foreach (var row in System.IO.File.ReadAllLines(@path)) {
                    data.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
                    string isLoginEnabled;
                    data.TryGetValue("isLoginEnabled", out isLoginEnabled);
                    bool.TryParse(isLoginEnabled, out enabled);
                    data.TryGetValue("loginMessage", out loginMessageAux);
                    if (enabled == false && !string.IsNullOrEmpty(loginMessageAux)) {
                        break;
                    }
                }
            }
            loginMessage = loginMessageAux;
            return enabled;
        }


        [HttpPost]
        public ActionResult Index(string userName, string password, string userTimezoneOffset) {
            LoginHandlerModel model = null;
            var validationMessage = GetValidationMessage(userName, password);
            if (!string.IsNullOrEmpty(validationMessage)) {
                model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName());
                return View(model);
            }
            userName = userName.ToLower();
            var user = GetUser(userName, password, userTimezoneOffset);
            if (user != null /*&& user.Active == true*/) {
                return AuthSucceeded(userName, userTimezoneOffset, user);
            }
            return AuthFailed(user);
        }

        private ActionResult AuthFailed(InMemoryUser user) {
            string validationMessage = null;
            bool active = true;
            if (user != null && user.Active.HasValue && user.Active.Value == false) {
                active = false;
            } else {
                validationMessage = ApplicationConfiguration.LoginErrorMessage != null
                    ? ApplicationConfiguration.LoginErrorMessage.Replace("\\n", "\n")
                    : null;
            }

            var model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName()) {
                UserNotActive = !active
            };
            return View(model);
        }

        private ActionResult AuthSucceeded(string userName, string userTimezoneOffset, InMemoryUser user) {
            var syncEveryTime = "true".Equals(MetadataProvider.GlobalProperty(SwUserConstants.LdapSyncAlways));
            if (syncEveryTime) {
                user.DBUser = UserManager.SyncLdapUser(user.DBUser, _ldapManager.IsLdapSetup());
            }
            AuthenticationCookie.SetSessionCookie(userName, userTimezoneOffset, Response);
            Response.Redirect(FormsAuthentication.GetRedirectUrl(userName, false));
            System.Threading.Thread.CurrentPrincipal = user;
            return null;
        }

        private bool IsHapagClient() {
            return ApplicationConfiguration.ClientName == "hapag";
        }

        private string ClientName() {
            return ApplicationConfiguration.ClientName;
        }

        public string GetValidationMessage(string userName, string password) {
            var userNameMessage = ApplicationConfiguration.LoginUserNameMessage != null
                ? ApplicationConfiguration.LoginUserNameMessage.Replace("\\n", "\n")
                : null;
            var passwordMessage = ApplicationConfiguration.LoginPasswordMessage != null
                ? ApplicationConfiguration.LoginPasswordMessage.Replace("\\n", "\n")
                : null;
            if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userNameMessage)) {
                return userNameMessage;
            }
            if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(passwordMessage)) {
                return passwordMessage;
            }
            return null;
        }

        private InMemoryUser GetUser(string userName, string password, string userTimezoneOffset) {
            var userAux = _dao.FindSingleByQuery<User>(softwrench.sw4.user.classes.entities.User.UserByUserName, userName);
            // User needs to load person data
            var allowNonUsersToLogin = "true".Equals(MetadataProvider.GlobalProperty("ldap.allownonmaximousers"));
            if (userAux == null) {
                if (!allowNonUsersToLogin) {
                    //user not found and we will not create a new one --> fail
                    return null;
                }

                //if this flag is true, we will create the user on the fly
                var ldapResult = _ldapManager.LdapAuth(userName, password);
                if (ldapResult.Success) {
                    userAux = UserManager.CreateMissingDBUser(userName);
                    if (userAux == null) {
                        //user might not exist on maximo either, in that case we should block its access
                        return null;
                    }
                    return SecurityFacade.GetInstance().DoLogin(userAux, userTimezoneOffset);
                }
                return null;
            }

            if (!userAux.IsActive) {
                return InMemoryUser.NewAnonymousInstance(false);
            }

            //this will trigger default ldap auth ==> The user already exists in our database
            if (userAux.Password == null) {
                if (!_ldapManager.IsLdapSetup()) {
                    return null;
                }

                var ldapResult = _ldapManager.LdapAuth(userName, password);
                if (ldapResult.Success) {
                    return SecurityFacade.GetInstance().DoLogin(userAux, userTimezoneOffset);
                }
                return null;
            }



            var user = SecurityFacade.GetInstance().Login(userAux, password, userTimezoneOffset);
            return user;

        }

        [HttpPost]
        public ActionResult SignInReturningUserData(string userName, string password, string userTimezoneOffset) {
            userName = userName.ToLower();
            var user = GetUser(userName, password, userTimezoneOffset);


            if (user == null) {
                // We must end the response right now
                // otherwise the forms auth module will
                // intercept the response "in-flight"
                // and swap it for a 302.
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.End();

                // This result will never be
                // written to the response.
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            AuthenticationCookie.SetPersistentCookie(userName, userTimezoneOffset, Response);

            return Json(new UserReturningData(user));
        }



        internal class UserReturningData {
            public bool Found {
                get; set;
            }
            public string UserName {
                get; set;
            }
            public string OrgId {
                get; set;
            }
            public string SiteId {
                get; set;
            }

            public long? UserTimezoneOffset {
                get; set;
            }

            public UserReturningData(InMemoryUser user) {
                if (user == null) {
                    Found = false;
                } else {
                    Found = true;
                    UserName = user.Login;
                    OrgId = user.OrgId;
                    SiteId = user.SiteId;
                    UserTimezoneOffset = user.TimezoneOffset;
                }
            }
        }
    }
}
