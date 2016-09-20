using JetBrains.Annotations;
using softWrench.sW4.AUTH;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Web.Controllers.Security;

namespace softWrench.sW4.Web.Controllers {
    public class SignInController : Controller {
        private readonly SecurityFacade _facade;
        private readonly LdapManager _ldapManager;
        private readonly SWDBHibernateDAO _dao;
        private readonly UserManager _userManager;


        public SignInController(SecurityFacade facade, LdapManager ldapManager, SWDBHibernateDAO dao, UserManager userManager) {
            _facade = facade;
            _ldapManager = ldapManager;
            _dao = dao;
            _userManager = userManager;
        }

        public ActionResult Index(bool timeout = false, bool forbidden = false) {
            if (User.Identity.IsAuthenticated) {
                Response.Redirect(FormsAuthentication.GetRedirectUrl(User.Identity.Name, false));
            }

            LoginHandlerModel model;
            string loginMessage = null;
            if (!IsLoginEnabled(ref loginMessage)) {
                model = new LoginHandlerModel(false, loginMessage, ClientName(), ProfileName());
            } else {
                model = new LoginHandlerModel(true, IsHapagClient(), ClientName(), ProfileName());
            }
            model.Inactivity = timeout;
            model.Forbidden = forbidden;
            return View(model);
        }

        /// <summary>
        /// Offline mobile authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="userTimezoneOffset"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SignInReturningUserData(string userName, string password, string userTimezoneOffset) {
            userName = userName.ToLower();
            var user = await GetUser(userName, password, userTimezoneOffset);

            if (user == null || (user.Active.HasValue && user.Active.Value == false)) {
                //TODO: make a different message for non active users

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

            return Json(new UserSyncData(user));
        }

        [HttpPost]
        public async Task<ActionResult> Index(string userName, string password, string userTimezoneOffset) {
            LoginHandlerModel model = null;
            var context = System.Web.HttpContext.Current; // needed due to the possible change of thread by async/await
            var validationMessage = GetValidationMessage(userName, password);
            if (!string.IsNullOrEmpty(validationMessage)) {
                model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName(), ProfileName());
                return View(model);
            }
            userName = userName.ToLower();
            var user = await GetUser(userName, password, userTimezoneOffset);
            if (user != null && user.Active == true) {
                return await AuthSucceeded(userName, userTimezoneOffset, user, context);
            }
            return AuthFailed(user);
        }


        /// <summary>
        ///  Retrieves the user merging with Maximo data.
        /// 
        ///  There are 2 main flows here, considering or not LDAP users.
        /// 
        ///  If the user is not found on SWDB, it can still be created on the fly under the presence of a flag.
        /// 
        ///  After the user is found on SWDB the corresponding Person entry is fetched from Maximo side.
        ///  
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="userTimezoneOffset"></param>
        /// <returns></returns>
        [CanBeNull]
        private async Task<InMemoryUser> GetUser(string userName, string password, string userTimezoneOffset) {
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
                    userAux = await _userManager.CreateMissingDBUser(userName);
                    if (userAux == null) {
                        //user might not exist on maximo either, in that case we should block its access
                        return null;
                    }
                    return await _facade.DoLogin(userAux, userTimezoneOffset);
                }
                return null;
            }

            if (!userAux.UserName.Equals("swadmin") && userAux.IsActive.HasValue && userAux.IsActive == false) {
                //swadmin cannot be inactive--> returning a non active user, so that we can differentiate it from a not found user on screen
                return InMemoryUser.NewAnonymousInstance(false);
            }

            //this will trigger default ldap auth ==> The user already exists in our database
            if (userAux.Password == null) {
                if (!_ldapManager.IsLdapSetup()) {
                    return null;
                }

                var ldapResult = _ldapManager.LdapAuth(userName, password);
                if (ldapResult.Success) {
                    return await _facade.DoLogin(userAux, userTimezoneOffset);
                }
                return null;
            }

            //non LDAP scenario
            var user = await _facade.LoginCheckingPassword(userAux, password, userTimezoneOffset);
            return user;

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

            var model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName(), ProfileName()) {
                UserNotActive = !active
            };
            return View(model);
        }

        private async Task<ActionResult> AuthSucceeded(string userName, string userTimezoneOffset, InMemoryUser user, HttpContext context) {
            var syncEveryTime = "true".Equals(MetadataProvider.GlobalProperty(SwUserConstants.LdapSyncAlways));
            if (syncEveryTime) {
                user.DBUser = await _userManager.SyncLdapUser(user.DBUser, _ldapManager.IsLdapSetup());
            }
            AuthenticationCookie.SetSessionCookie(userName, userTimezoneOffset, Response);

            System.Threading.Thread.CurrentPrincipal = user;
            if (_userManager.VerifyChangePassword(user)) {
                Response.Redirect("~/UserSetup/ChangePassword");
                return null;
            }
            System.Web.HttpContext.Current = context; // needed due to the possible change of thread by async/await
            Response.Redirect(FormsAuthentication.GetRedirectUrl(userName, false));
            return null;
        }

        private bool IsHapagClient() {
            return ApplicationConfiguration.ClientName == "hapag";
        }

        private string ClientName() {
            return ApplicationConfiguration.ClientName;
        }

        private string ProfileName()
        {
            return ApplicationConfiguration.Profile;
        }

        private string GetValidationMessage(string userName, string password) {
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


    }
}
