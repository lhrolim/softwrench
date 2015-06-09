﻿using System.Configuration;
using cts.commons.portable.Util;
using cts.commons.Util;
using log4net;
using log4net.Config;
using Microsoft.Web.Mvc;
using Newtonsoft.Json.Serialization;
using NHibernate.Context;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.log4net;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
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

    public class WebApiApplication : System.Web.HttpApplication, ISWEventListener<ClientChangeEvent>, ISWEventListener<ClearCacheEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(WebApiApplication));

        protected void Application_Start(object sender, EventArgs args) {
            DoStartApplication(false);
        }

        private static void DoStartApplication(bool changeClient) {

            var before = Stopwatch.StartNew();
            if (!changeClient) {
                ViewEngines.Engines.Clear();
                ViewEngines.Engines.Add(new ClientAwareRazorViewEngine());
                ViewEngines.Engines.Add(new FixedWebFormViewEngine());
                // to render the reports user controls (.ascx)            
                ConfigureLogging();
                AreaRegistration.RegisterAllAreas();
                EnableJsonCamelCasing();
                RegisterDataMapFormatter();

                WebApiConfig.Register(GlobalConfiguration.Configuration);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
              
            }
            MetadataProvider.DoInit();
            new MigratorExecutor("SWDB").Migrate(runner => runner.MigrateUp());
            if (!changeClient) {
                var container = SimpleInjectorScanner.InitDIController();
                var dispatcher = (IEventDispatcher)container.GetInstance(typeof(IEventDispatcher));
                dispatcher.Dispatch(new ApplicationStartedEvent());
                ManagedWebSessionContext.Bind(System.Web.HttpContext.Current, SWDBHibernateDAO.SessionManager.SessionFactory.OpenSession());
            }
            SecurityFacade.InitSecurity();
            Log.Info(LoggingUtil.BaseDurationMessage("**************App started in {0}*************", before));
            ApplicationConfiguration.StartTimeMillis = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private static void ConfigureLogging() {
            XmlConfigurator.Configure();
            if (ApplicationConfiguration.IsLocal()) {
                //Log4NetUtil.ChangeLevel("DEFAULT_LOG", "DEBUG", null);
                //Log4NetUtil.ChangeLevel("softwrench", "DEBUG", null);
            } else if (!ApplicationConfiguration.IsDev()) {
                Log4NetUtil.ChangeLevel("MAXIMO.SQL", "WARN", null);
                Log4NetUtil.ChangeLevel("SWDB.SQL", "WARN", null);
            }

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
                var ticket = FormsAuthentication.Decrypt(
                    Request.Cookies[FormsAuthentication.FormsCookieName].Value);
                if (ticket == null) {
                    return; // Not authorised
                }
                if (ticket.Expiration < DateTime.Now) {
                    FormsAuthentication.SignOut();
                    throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }
            } catch {
                //error handling
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e) {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            
            
            if (Request.UrlReferrer != null){
                //this is for ripple development where CORS is enabled.
                //TODO: review if these settings are really needed into production,or how to do it the right way,since it might represent a security leak
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "http://"+Request.UrlReferrer.Authority);
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");    
            }
            

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS") {
                //These headers are handling the "pre-flight" OPTIONS call sent by the browser
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept , offlineMode");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e) {
            var context = new HttpContextWrapper(Context);
            if (Context.Response.StatusCode == 302 && Context.Response.RedirectLocation.Contains("/SignIn")) {
                //302 ==> not allowed
                //Context.Response.RedirectLocation.Contains("/SignIn") --> are we redirecting to login

                //set in HomeController if the user has no permissions on the application
                var isForbidden = Context.Response.RedirectLocation.Contains("forbidden=true");
                if ("~/Signout/SignOut".Equals(Request.AppRelativeCurrentExecutionFilePath,
                    StringComparison.CurrentCultureIgnoreCase)) {
                    //this means that we´re coming from the logout action --> nothing to do
                    return;
                }
                if (Request.Params["HTTP_REFERER"] == null) {
                    //the HTTP_REFERER is null on the first time the app loads. Removing this check would cause wrong behaviour on first time access
                    return;
                }

                if (!isForbidden) {
                    //already marked as forbidden, let´s not mess the messages
                    Context.Response.RedirectLocation += "&timeout=true";
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

        public void HandleEvent(ClientChangeEvent eventToDispatch) {
            var oldKey = ConfigurationManager.AppSettings["clientkey"];
            if (ConfigurationManager.AppSettings["originalclientkey"] == null) {
                ConfigurationManager.AppSettings["originalclientkey"] = oldKey;
            }
            var newKey = eventToDispatch.ClientKey;
            if (eventToDispatch.Restore) {
                newKey = ConfigurationManager.AppSettings["originalclientkey"];
            }
            if (oldKey == newKey) {
                throw new InvalidOperationException("client key not changed");
            }
            ConfigurationManager.AppSettings["clientkey"] = newKey;
            try {
                DoStartApplication(true);
            } catch (Exception e) {
                ConfigurationManager.AppSettings["clientkey"] = oldKey;
                DoStartApplication(true);
                throw new Exception("Error: could not modify client restoring to {0}".Fmt(oldKey), e);
            }
        }

        public void HandleEvent(ClearCacheEvent eventToDispatch) {
            
        }
    }
}
