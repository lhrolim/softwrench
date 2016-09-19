using log4net;
using softWrench.sW4.Security.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;


namespace softWrench.sW4.Web.Common {
    public class LogFilter : ActionFilterAttribute {

        private static readonly ILog Log = LogManager.GetLogger("LogFilter");
        private const string LogInfo = "LogInfo";

        public override void OnActionExecuting(HttpActionContext actionExecutingContext) {
            base.OnActionExecuting(actionExecutingContext);
            var loggedUser = SecurityFacade.CurrentUser();
            var userName = loggedUser == null? "Anonymous" : loggedUser.Login;
            var logInfo = new LogInfo(userName, DateTime.Now, actionExecutingContext.Request.RequestUri,
                actionExecutingContext.ActionDescriptor.ActionName);
            HttpContext.Current.Items.Add(LogInfo, logInfo);
            if (!Log.IsDebugEnabled) {
                return;
            }
            var query = actionExecutingContext.Request.GetQueryNameValuePairs();
            Log.Debug(query);
            var content = actionExecutingContext.Request.Content;
            var jsonContent = content.ReadAsStringAsync().Result;
            if (!string.IsNullOrEmpty(jsonContent)) {
                Log.Debug(jsonContent);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {
            base.OnActionExecuted(actionExecutedContext);
            var logInfo = HttpContext.Current.Items.Contains(LogInfo) ? HttpContext.Current.Items[LogInfo] as LogInfo : null;
            if (logInfo == null) {
                Log.Warn("unable to find initial LogInfo");
            } else {
                logInfo.EndDateTime = DateTime.Now;
                var response = actionExecutedContext.Response;
                logInfo.HttpStatusCode = response != null ? response.StatusCode : HttpStatusCode.InternalServerError;
                Log.Info(logInfo.ToString());
            }
        }


    }
}
