using log4net;
using softWrench.sW4.Security.Services;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace softWrench.sW4.Web.Common.Log {
    public class MVCLogFilter : ActionFilterAttribute {

        private static readonly ILog Log = LogManager.GetLogger("LogFilter");
        private const string LogInfo = "LogInfo";

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            base.OnActionExecuting(filterContext);
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
                return;
            }
            var userName = SecurityFacade.CurrentUser().Login;
            var request = filterContext.HttpContext.Request;
            var controller = filterContext.Controller;
            var routeData = request.RequestContext.RouteData;
            var sb = new StringBuilder();
            for (int i = 0; i < request.Headers.Count; i++)
                sb.AppendFormat("{0}={1};", request.Headers.Keys[i],
                                            request.Headers[i]);

            var logInfo = new Log.LogInfo() {
                RequestDate = DateTime.Now,
                RequestType = request.RequestType,
                Url = request.RawUrl,
                IPAddress = request.UserHostAddress,
                Controller = (string)routeData.Values["controller"],
                Action = (string)routeData.Values["action"],
                RequestHeader = sb.ToString(),
                UserName = userName
            };

            if (!HttpContext.Current.Items.Contains(LogInfo)) {
                HttpContext.Current.Items.Add(LogInfo, logInfo);
            }

            if (!Log.IsDebugEnabled) {
                filterContext.HttpContext.Response.Filter = new CapturingResponseFilter(filterContext.HttpContext.Response.Filter, logInfo);
                return;
            }
            var query = request.QueryString;
            Log.Debug(query);
            using (var reader = new StreamReader(request.InputStream)) {
                try {
                    request.InputStream.Position = 0;
                    logInfo.RequestBody = reader.ReadToEnd();
                } catch (Exception) {
                    logInfo.RequestBody = string.Empty;
                    //log errors
                } finally {
                    request.InputStream.Position = 0;
                }
            }
            Log.Debug(logInfo.RequestBody);
            filterContext.HttpContext.Response.Filter = new CapturingResponseFilter(filterContext.HttpContext.Response.Filter, logInfo);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext) {
            base.OnResultExecuted(filterContext);
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
                return;            
            }
            var response = filterContext.HttpContext.Response;
            var sb = new StringBuilder();
            for (int i = 0; i < response.Headers.Count; i++) {
                sb.AppendFormat("{0}={1};", response.Headers.Keys[i], response.Headers[i]);
            }
            var filter = (CapturingResponseFilter)filterContext.HttpContext.Response.Filter;
            var item = filter.AccessLogItem;
            if (item == null) {
                Log.Warn("unable to find initial LogInfo");
            } else {
                item.ResponseDate = DateTime.Now;
                item.ResponseHeader = sb.ToString();
                Log.Info(item.ToString());
            }
        }
    }
}