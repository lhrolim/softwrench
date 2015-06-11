﻿using System.Threading;
using JetBrains.Annotations;
using Quartz.Util;
using softWrench.sW4.AUTH;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.LoginHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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

        public ActionResult Index(bool timeout=false,bool forbidden=false) {
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
            if (user != null) {
                return AuthSucceeded(userName, userTimezoneOffset, user);
            }
            return AuthFailed();
        }

        private ActionResult AuthFailed() {
            string validationMessage;
            LoginHandlerModel model;
            validationMessage = ApplicationConfiguration.LoginErrorMessage != null
                ? ApplicationConfiguration.LoginErrorMessage.Replace("\\n", "\n")
                : null;
            model = new LoginHandlerModel(true, true, validationMessage, IsHapagClient(), ClientName());
            return View(model);
        }

        private ActionResult AuthSucceeded(string userName, string userTimezoneOffset, InMemoryUser user) {
            var syncEveryTime = "true".Equals(MetadataProvider.GlobalProperty("ldap.synceverytime"));
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

        private string ClientName()
        {
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
            var userAux = _dao.FindSingleByQuery<User>(sW4.Security.Entities.User.UserByUserName, userName);
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
                    return SecurityFacade.GetInstance().LdapLogin(userAux, userTimezoneOffset);
                }
                return null;
            }

            //this will trigger default ldap auth ==> The user already exists in our database
            if (userAux.Password == null) {
                var ldapResult = _ldapManager.LdapAuth(userName, password);
                if (ldapResult.Success) {

                    return SecurityFacade.GetInstance().LdapLogin(userAux, userTimezoneOffset);
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

        private static class AuthenticationCookie {
            private const int PersistentCookieTimeoutDays = 14;

            /// <summary>
            ///     Sets a non-persistent cookie (i.e. not saved accross
            ///     browser sessions) for the Forms Authentication state.
            /// </summary>
            /// <param name="userName">The username authenticated.</param>
            /// <param name="userTimezoneOffset">The user time zone offset</param>
            /// <param name="response">Response object</param>
            public static void SetSessionCookie(string userName, string userTimezoneOffset, HttpResponseBase response) {
                //FormsAuthentication.SetAuthCookie(userName, false);

                var strb = new StringBuilder();
                strb.AppendFormat("userName={0}", userName);
                strb.AppendFormat(";userTimezoneOffset={0}", userTimezoneOffset);
                var dateToUse = ApplicationConfiguration.SystemBuildDateInMillis;
                if (ApplicationConfiguration.IsLocal()) {
                    //if local we can safely use the starttime
                    dateToUse = ApplicationConfiguration.StartTimeMillis;
                }
                strb.AppendFormat(";cookiesystemdate={0}", dateToUse);


                var cookie = FormsAuthentication.GetAuthCookie(userName, false);
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                    ticket.Expiration, ticket.IsPersistent, strb.ToString(), ticket.CookiePath);
                FormsAuthentication.RenewTicketIfOld(newTicket);
                var encTicket = FormsAuthentication.Encrypt(newTicket);

                cookie.Value = encTicket;

                response.Cookies.Add(cookie);
            }

            /// <summary>
            ///     Sets a persistent cookie (i.e. saved accross browser
            ///     sessions) for the Forms Authentication state.
            /// </summary>
            /// <param name="userName">The username authenticated.</param>
            /// <param name="response">The HTTP response to inject the cookie into.</param>
            public static void SetPersistentCookie(string userName, string userTimezoneOffset, HttpResponseBase response) {
                // TODO: set userTimezoneOffset
                var ticket = new FormsAuthenticationTicket(userName.ToLower(), true, (int)TimeSpan.FromDays(PersistentCookieTimeoutDays).TotalMinutes);
                var encryptedData = FormsAuthentication.Encrypt(ticket);

                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedData) {
                    HttpOnly = true,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL
                };

                response.Cookies.Add(cookie);
            }
        }

        internal class UserReturningData {
            public bool Found { get; set; }
            public string UserName { get; set; }
            public string OrgId { get; set; }
            public string SiteId { get; set; }

            public UserReturningData(InMemoryUser user) {
                if (user == null) {
                    Found = false;
                } else {
                    Found = true;
                    UserName = user.Login;
                    OrgId = user.OrgId;
                    SiteId = user.SiteId;
                }
            }
        }
    }
}
