using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using log4net;
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
            if (e is UnauthorizedException) {
                return HttpStatusCode.Unauthorized;
            }
            return HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Builds a custom ErrorDto depending on the exception.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ErrorDto BuildErrorDto(Exception e) {
            if (e is MaximoException) {
                var maximoException = (MaximoException)e;
                var dto = new ErrorDto(maximoException) {
                    OutlineInformation = maximoException.OutlineInformation,
                    ErrorStack = maximoException.FullStackTrace,
                    FullStack = maximoException.FullStackTrace
                };
                return dto;
            }
            return new ErrorDto(e);
        }

        public override void OnException(HttpActionExecutedContext context) {
            var e = context.Exception;
            var rootException = ExceptionUtil.DigRootException(e);
            Log.Error(rootException, e);
            var errorDto = BuildErrorDto(rootException);
            if (e is AfterCreationException)
            {
                errorDto.ResultObject = ((AfterCreationException) e).ResultObject;
            }

            var errorResponse = context.Request.CreateResponse(CodeOf(rootException), errorDto);
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
            filterContext.HttpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}