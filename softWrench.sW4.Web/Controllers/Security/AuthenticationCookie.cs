using System;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers.Security {
    public class AuthenticationCookie {


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
            cookie.Path = HostingEnvironment.ApplicationVirtualPath;
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
}