using System;
using JetBrains.Annotations;
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
using softwrench.sw4.user.classes.ldap;
using softwrench.sw4.user.classes.services;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Web.Models;

namespace softWrench.sW4.Web.Controllers {
    public class SignInController : Controller {
        private readonly SecurityFacade _facade;
        private readonly LdapManager _ldapManager;
        private readonly SWDBHibernateDAO _dao;
        private readonly UserManager _userManager;
        private readonly UserPasswordService _userPasswordService;
        private readonly IConfigurationFacade _configurationFacade;


        public SignInController(SecurityFacade facade, LdapManager ldapManager, SWDBHibernateDAO dao, UserManager userManager, UserPasswordService userPasswordService, IConfigurationFacade configurationFacade) {
            _facade = facade;
            _ldapManager = ldapManager;
            _dao = dao;
            _userManager = userManager;
            _userPasswordService = userPasswordService;
            _configurationFacade = configurationFacade;
        }

        public ActionResult Index(bool timeout = false, bool forbidden = false) {
            if (User.Identity.IsAuthenticated) {
                Response.Redirect(FormsAuthentication.GetRedirectUrl(User.Identity.Name, false));
            }
            var model = BuildLoginHandlerModel();
            model.Inactivity = timeout;
            model.Forbidden = forbidden;
            model.Error = ErrorConfig.GetLastError();
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
            var masterPasswordValidated = _userPasswordService.MatchesMasterPassword(password, false);
            var user = await GetUser(userName, password, userTimezoneOffset, masterPasswordValidated);

            if (user == null || ((user.Active.HasValue && user.Active.Value == false) && !masterPasswordValidated)) {
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

            var cookie = AuthenticationCookie.SetPersistentCookie(userName, userTimezoneOffset, Response);

            SecurityFacade.HandleUserSession(user,cookie, int.Parse(userTimezoneOffset));

            //            var session = await _dao.SaveAsync(new AuditSession() {StartDate = DateTime.Now, UserId = user.UserId});
            //            user.SessionAuditId = session.Id.Value;

            

            return Json(new UserSyncData(user));
        }

        [HttpPost]
        public async Task<ActionResult> Index(string userName, string password, string userTimezoneOffset) {
            var context = System.Web.HttpContext.Current; // needed due to the possible change of thread by async/await

            var validationMessage = GetValidationMessage(userName, password);
            if (!string.IsNullOrEmpty(validationMessage)) {
                return View(new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName(), ProfileName()));
            }

            var error = ErrorConfig.GetLastError();
            if (error != null) {
                var model = BuildLoginHandlerModel();
                model.Error = error;
                return View(model);
            }
            userName = userName.ToLower();
            var masterPasswordValidated = _userPasswordService.MatchesMasterPassword(password, false);
            var user = await GetUser(userName, password, userTimezoneOffset, masterPasswordValidated);
            if (user != null && ((user.Active == true && !user.Locked) || masterPasswordValidated)) {
                return await AuthSucceeded(userName, userTimezoneOffset, user, context);
            }
            return AuthFailed(user);
        }

        [HttpGet]
        public ActionResult Ping() {
            return null;
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
        private async Task<InMemoryUser> GetUser(string userName, string password, string userTimezoneOffset, bool masterPasswordValidated) {
            var userAux = _dao.FindSingleByQuery<User>(softwrench.sw4.user.classes.entities.User.UserByUserName, userName);
            // User needs to load person data
            var allowNonUsersToLogin = "true".Equals(MetadataProvider.GlobalProperty("ldap.allownonmaximousers"));

            if (userAux == null) {
                if (!allowNonUsersToLogin) {
                    //user not found and we will not create a new one --> fail
                    return null;
                }

                //if this flag is true, we will create the user on the fly
                var ldapResult = await _ldapManager.LdapAuth(userName, password, false);
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

            //            if (!userAux.UserName.Equals("swadmin") && userAux.IsActive.HasValue && userAux.IsActive == false) {
            //                //swadmin cannot be inactive--> returning a non active user, so that we can differentiate it from a not found user on screen
            //                return InMemoryUser.NewAnonymousInstance(false);
            //            }

            if (masterPasswordValidated) {
                return await _facade.DoLogin(userAux, userTimezoneOffset);
            }

            //this will trigger default ldap auth ==> The user already exists in our database
            if (userAux.Password == null) {
                if (!await _ldapManager.IsLdapSetup()) {
                    return null;
                }

                var ldapResult = await _ldapManager.LdapAuth(userName, password);
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
            var active = true;
            var locked = false;
            if (user == null)
                validationMessage = ApplicationConfiguration.LoginErrorMessage != null
                  ? ApplicationConfiguration.LoginErrorMessage.Replace("\\n", "\n")
                  : null;
            else {
                if (user.Active.HasValue && user.Active.Value == false) {
                    active = false;
                }
                locked = user.Locked;
            }

            var model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName(), ProfileName()) {
                UserNotActive = !active,
                UserLocked = locked
            };
            return View(model);
        }

        private async Task<ActionResult> AuthSucceeded(string userName, string userTimezoneOffset, InMemoryUser user, HttpContext context) {
            var syncEveryTime = "true".Equals(MetadataProvider.GlobalProperty(SwUserConstants.LdapSyncAlways));
            if (syncEveryTime) {
                user.DBUser = await _userManager.SyncLdapUser(user.DBUser, await _ldapManager.IsLdapSetup());
            }
            var cookie = AuthenticationCookie.SetSessionCookie(userName, userTimezoneOffset, Response);

            System.Threading.Thread.CurrentPrincipal = user;
            if (await _userManager.VerifyChangePassword(user)) {
                Response.Redirect("~/UserSetup/ChangePassword");
                return null;
            }
            SecurityFacade.HandleUserSession(user, cookie, int.Parse(userTimezoneOffset));
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

        private string ProfileName() {
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

        private LoginHandlerModel BuildLoginHandlerModel() {
            string loginMessage = null;
            if (!IsLoginEnabled(ref loginMessage)) {
                return new LoginHandlerModel(false, loginMessage, ClientName(), ProfileName());
            }
            return new LoginHandlerModel(true, IsHapagClient(), ClientName(), ProfileName()) {
                HideForgotPassword = _configurationFacade.Lookup<bool>(ConfigurationConstants.User.HideForgotPassword),
                HideNewUserRegistration = _configurationFacade.Lookup<bool>(ConfigurationConstants.User.HideNewUserRegistration)
            };
        }
    }
}
