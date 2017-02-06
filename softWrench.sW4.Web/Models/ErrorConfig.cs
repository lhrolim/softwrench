using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cts.commons.simpleinjector;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Controllers;

namespace softWrench.sW4.Web.Models {
    public static class ErrorConfig {

        public const string LastErrorKey = "SwError";

        public static void Handle(HttpContext context, Exception ex) {
            // Only handles custom errors from non ajax requests of http codes 400 or higher
            // Ajax exceptions are handled on GenericExceptionFilter
            if (context.Response.StatusCode >= 400 && !"true".Equals(context.Request.Headers["isajax"])) {
                Show(context, context.Response.StatusCode, ex);
            } else {
                SetLastError(null);
            }
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

            var controller = SimpleInjectorGenericFactory.Instance.GetObject<RouteController>() as IController;
            var routeData = new RouteData();

            routeData.Values["controller"] = "Route";
            routeData.Values["action"] = "ErrorFallback";

            controller.Execute(new RequestContext(wrapper, routeData));
        }
    }
}