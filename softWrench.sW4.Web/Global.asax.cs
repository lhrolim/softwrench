using System.Globalization;
using log4net;
using Microsoft.Web.Mvc;
using Newtonsoft.Json.Serialization;
using NHibernate.Context;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;
using softWrench.sW4.Web.App_Start;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.DB_Migration;
using softWrench.sW4.Web.Formatting;
using softWrench.sW4.Web.SimpleInjector;
using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;


namespace softWrench.sW4.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication {

        private static readonly ILog Log = LogManager.GetLogger(typeof(WebApiApplication));

        protected void Application_Start(object sender, EventArgs args) {

            Console.SetOut(new System.IO.StreamWriter(System.IO.Stream.Null));
            Console.SetError(new System.IO.StreamWriter(System.IO.Stream.Null));

            var before = Stopwatch.StartNew();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ClientAwareRazorViewEngine());
            ViewEngines.Engines.Add(new FixedWebFormViewEngine()); // to render the reports user controls (.ascx)            

            ConfigureLogging();
            AreaRegistration.RegisterAllAreas();
            MetadataProvider.DoInit();
            EnableJsonCamelCasing();
            RegisterDataMapFormatter();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            new MigratorExecutor("SWDB").Migrate(runner => runner.MigrateUp());
            ManagedWebSessionContext.Bind(
             System.Web.HttpContext.Current,
             SWDBHibernateDAO.SessionManager.SessionFactory.OpenSession());
            SecurityFacade.InitSecurity();

            var container = SimpleInjectorScanner.InitDIController();
            var dispatcher = (IEventDispatcher)container.GetInstance(typeof(IEventDispatcher));
            dispatcher.Dispatch(new ApplicationStartedEvent());
            Log.Info(LoggingUtil.BaseDurationMessage("**************App started in {0}*************", before));
            ApplicationConfiguration.StartDate = DateTime.Now;
        }

        private static void ConfigureLogging() {
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("*****Starting web app****************");
        }

        private static void RegisterDataMapFormatter() {
            var index = GlobalConfiguration
                .Configuration
                .Formatters
                .IndexOf(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

            if (index == -1) {
                index = GlobalConfiguration
                    .Configuration
                    .Formatters
                    .Count;
            }

            GlobalConfiguration
                .Configuration
                .Formatters
                .Insert(index, new DataMapXmlFormatter());
        }

        public void FormsAuthentication_OnAuthenticate(object sender, FormsAuthenticationEventArgs args) {
            if (!FormsAuthentication.CookiesSupported) {
                throw new HttpException("Cookieless Forms Authentication is not " +
                                        "supported for this application.");
            }
            if (Request.Cookies[FormsAuthentication.FormsCookieName] == null) {
                return;
            }
            try {
                var ticket = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value);
                if (ticket == null) {
                    return;
                }

                //
                //                if (HasApplicationVersionChangedSinceCookieIssued(ticket)) {
                //                    //if the cookie was setted on a different system version, if though it might still be valid, let´s force a new login
                //                    return; // Not authorised
                //                }
                if (ticket.Expiration < DateTime.Now) {
                    FormsAuthentication.SignOut();
                    throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }
            } catch {
                //error handling
            }
        }

        //        private bool HasApplicationVersionChangedSinceCookieIssued(FormsAuthenticationTicket ticket) {
        //            var userData = ticket.UserData;
        //            var ticketDict = PropertyUtil.ConvertToDictionary(userData);
        //            string cookieVersion;
        //            ticketDict.TryGetValue("cookiesystemdate", out cookieVersion);
        //            if (ApplicationConfiguration.IsLocal()) {
        //                return ApplicationConfiguration.StartTimeMillis.ToString(CultureInfo.InvariantCulture) != cookieVersion;
        //            }
        //            return ApplicationConfiguration.SystemBuildDateInMillis.ToString(CultureInfo.InvariantCulture) != cookieVersion;
        //        }

        //        protected void Application_BeginRequest(
        //           object sender, EventArgs e) {
        //            ManagedWebSessionContext.Bind(
        //                System.Web.HttpContext.Current,
        //                SWDBHibernateDAO.SessionManager.SessionFactory.OpenSession());
        //        }
        //
        protected void Application_EndRequest(object sender, EventArgs e) {
            if (Response.ContentType == "text/html") {
                //fix back button browser bug that would "relogin" users (would show page that was cached)
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();
            }
            var context = new HttpContextWrapper(Context);
            if (Context.Response.StatusCode == 302 && Context.Response.RedirectLocation.Contains("/SignIn")) {
                if ("~/Signout/SignOutClosePage".Equals(Request.AppRelativeCurrentExecutionFilePath, StringComparison.CurrentCultureIgnoreCase)) {
                    Context.Response.RedirectLocation += "&closepage=true";
                    return;
                }
                if (!"~/Signout/SignOut".Equals(Request.AppRelativeCurrentExecutionFilePath, StringComparison.CurrentCultureIgnoreCase)) {
                    if (Request.Params["HTTP_REFERER"] != null) {
                        Context.Response.RedirectLocation += "&timeout=true";
                    }
                }


            }

        }

        /// <summary>
        /// this is used for allowing to place a user with more data then simply a username on the current request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e) {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null) {
                var inMemoryUser = SecurityFacade.CurrentUser();
                HttpContext.Current.User = inMemoryUser;

            }
        }

        private static void EnableJsonCamelCasing() {
            var jsonFormatter = GlobalConfiguration
                .Configuration
                .Formatters
                .JsonFormatter;

            jsonFormatter
                .SerializerSettings
                .ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}