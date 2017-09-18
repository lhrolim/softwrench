using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using log4net;
using softwrench.sw4.api.classes.exception;
using softwrench.sw4.offlineserver.model.exception;
using softWrench.sW4.Data.Persistence.Engine.Exception;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Common {
    public class GenericExceptionFilter : ExceptionFilterAttribute, System.Web.Mvc.IExceptionFilter {

        private static readonly ILog Log = LogManager.GetLogger(typeof(GenericExceptionFilter));

        /// <summary>
        /// Determines what http status code should be used depending on the exception.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private HttpStatusCode CodeOf(Exception e) {
            if (e is IStatusCodeException) {
                return ((IStatusCodeException)e).StatusCode;
            }

            if (e is UnauthorizedException) {
                return HttpStatusCode.Unauthorized;
            }
            return HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Builds a custom ErrorDto depending on the exception.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="rootException"></param>
        /// <returns></returns>
        private ErrorDto BuildErrorDto(Exception e, Exception rootException) {
            var maximoException = rootException as MaximoException;
            ErrorDto dto = null;
            if (maximoException != null) {
                dto = new ErrorDto(maximoException) {
                    OutlineInformation = maximoException.OutlineInformation,
                    ErrorStack = maximoException.FullStackTrace,
                    FullStack = maximoException.FullStackTrace
                };
            } else if (rootException is IOfflineSyncException) {
                dto = new OfflineErrorDto(rootException);
            } else {
                dto = new ErrorDto(rootException);
            }

            var afterCreationException = e as AfterCreationException;
            if (afterCreationException != null) {
                dto.ResultObject = afterCreationException.ResultObject;
            }

            return dto;
        }

        public override void OnException(HttpActionExecutedContext context) {
            var e = context.Exception;
            var rootException = ExceptionUtil.DigRootException(e);
            Log.Error(rootException, e);
            var errorDto = BuildErrorDto(e, rootException);
            var errorResponse = context.Request.CreateResponse(CodeOf(rootException), errorDto);
            throw new HttpResponseException(errorResponse);
        }

        public void OnException(ExceptionContext filterContext) {
            var e = filterContext.Exception;
            var rootException = ExceptionUtil.DigRootException(e);
            Log.Error(rootException, e);

            //            filterContext.ExceptionHandled = true;
            //            filterContext.HttpContext.Response.Clear();
            //            filterContext.HttpContext.Response.StatusCode = (int)CodeOf(rootException);
            //            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

            if (filterContext.HttpContext.Request.IsAjaxRequest()) {
                // if Ajax: json response
                var errorDto = BuildErrorDto(e, rootException);
                var result = new JsonResult() {
                    Data = errorDto,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.Result = result;

            } else {
                // not ajax: view result (html)
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(rootException, controllerName, actionName);
                var result = new ViewResult {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
                filterContext.Result = result;
            }
            //            filterContext.HttpContext.Response.End();
        }
    }
}