using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cts.commons.simpleinjector;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Controllers;

namespace softWrench.sW4.Web.Models {
    public static class ErrorConfig {

        public const string LastErrorKey = "SwError";

        public static void Handle(HttpContext context, Exception ex) {
            // Only handles custom errors from non ajax requests and non mobile of http codes 400 or higher
            // Ajax exceptions are handled on GenericExceptionFilter
            if (context.Response.StatusCode >= 400 && !"true".Equals(context.Request.Headers["isajax"]) && !IsMobile(context.Request)) {
                Show(context, context.Response.StatusCode, ex);
            } else {
                SetLastError(null);
            }
        }

        private static bool IsMobile(HttpRequest request) {
            return request.Path.Contains("/Mobile");
        }

        public static ErrorDto GetLastError() {
            return CallContext.LogicalGetData(LastErrorKey) as ErrorDto;
        }

        private static void SetLastError(ErrorDto error) {
            CallContext.LogicalSetData(LastErrorKey, error);
        }

        private static void Show(HttpContext context, int code, Exception ex) {
            context.Response.Clear();

            var wrapper = new HttpContextWrapper(context);

            ErrorDto error;
            if (code == 404) {
                error = new ErrorDto { ErrorMessage = "Page not found." };
            } else if (ex == null) {
                error = new ErrorDto { ErrorMessage = "Unknown Error with http code " + code + "." };
            } else {
                error = new ErrorDto(ex);
                error.ErrorMessage = "Error with http code " + code + ": " + error.ErrorMessage;
            }
            SetLastError(error);

            var user = SecurityFacade.CurrentUser();
            var noUser = user == null || "anonymous".Equals(user.Login);

            if (noUser) {
                context.Server.TransferRequest("~/SignIn");
                return;
            }

            var isNoMenu = IsNomenuController(wrapper);

            var controller = isNoMenu ? (IController)SimpleInjectorGenericFactory.Instance.GetObject<NoMenuErrorController>() : SimpleInjectorGenericFactory.Instance.GetObject<ErrorController>();
            var routeData = new RouteData();

            routeData.Values["controller"] = isNoMenu ? "NoMenuError" : "Error";
            routeData.Values["action"] = "ErrorFallback";

            controller.Execute(new RequestContext(wrapper, routeData));
        }

        private static bool IsNomenuController(HttpContextBase context) {
            var urlRouteData = RouteTable.Routes.GetRouteData(context);
            if (urlRouteData == null) {
                return false;
            }
            var controllerName = urlRouteData.Values["controller"].ToString();
            var requestContext = new RequestContext(context, urlRouteData);
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();
            var controller = (ControllerBase)controllerFactory.CreateController(requestContext, controllerName);
            var noMenuAttribute = Attribute.GetCustomAttribute(controller.GetType(), typeof(NoMenuController));
            return noMenuAttribute != null;
        }
    }
}