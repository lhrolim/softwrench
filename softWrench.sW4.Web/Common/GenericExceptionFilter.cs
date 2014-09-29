using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using log4net;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers;

namespace softWrench.sW4.Web.Common {
    public class GenericExceptionFilter : ExceptionFilterAttribute, System.Web.Mvc.IExceptionFilter {

        private static readonly ILog Log = LogManager.GetLogger(typeof(DataController));

        public override void OnException(HttpActionExecutedContext context) {
            var e = context.Exception;
            var rootException = ExceptionUtil.DigRootException(e);
            Log.Error(rootException, e);
            var errorResponse = context.Request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorDto(rootException));
            throw new HttpResponseException(errorResponse);
        }

        public void OnException(ExceptionContext filterContext) {
            var e = filterContext.Exception;
            var rootException = ExceptionUtil.DigRootException(e);
            Log.Error(rootException, e);
            //            var errorResponse = filterContext.HttpContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorDto(rootException));
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ContentResult();
            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var model = new HandleErrorInfo(rootException, controllerName, actionName);
            filterContext.ExceptionHandled = true;
            var result = new ViewResult {
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
            filterContext.Result = result;
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 200;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }


    }
}