using softWrench.sW4.Data.API;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace softWrench.sW4.Web.SPF.Filters {
    /// <summary>
    /// Responsible for placing the url of the html page after the execution of the method.<p/>
    /// This html will be handled in clint-side allowing to switch page sections by ajax.
    /// </summary>
    public class RedirectUrlFilter : ActionFilterAttribute {

        private const string ConventionPattern = "/Content/Controller/{0}.html";

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext.Exception != null) {
                //if has exception, will be handled by GenericExceptionFilter
                return;
            }

            if (!typeof(IGenericResponseResult).IsAssignableFrom(actionExecutedContext.ActionContext.ActionDescriptor.ReturnType)) {
                //do nothing case it´s not returning a IGenericResponseResult instance
                return;
            }
            var redirectAttribute = LookupAttribute(actionExecutedContext.ActionContext);
            var controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var url = FindRedirectURL(controllerName, redirectAttribute);
            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent == null) {
                //void method case
                return;
            }
            var value = objectContent.Value as IGenericResponseResult;
            if (value == null) {
                return;
            }

            if (value.RedirectURL == null) {
                value.RedirectURL = url;
            }
            if (value.CrudSubTemplate == null && redirectAttribute != null && redirectAttribute.CrudSubTemplate!=null) {
                if (!redirectAttribute.CrudSubTemplate.StartsWith("/Content")) {
                    redirectAttribute.CrudSubTemplate = "/Content" + redirectAttribute.CrudSubTemplate;
                }
                value.CrudSubTemplate = redirectAttribute.CrudSubTemplate;
            }
            value.TimeStamp = DateTime.Now;

            var actionArguments = LookupArguments(actionExecutedContext.ActionContext);
            if (actionArguments != null && !String.IsNullOrWhiteSpace(actionArguments.Title)) {
                value.Title = actionArguments.Title;
            } else if (value.Title == null && redirectAttribute != null) {
                value.Title = redirectAttribute.Title;
            }
        }

        private static IDataRequest LookupArguments(HttpActionContext actionContext) {
            var actionArguments = actionContext.ActionArguments.FirstOrDefault(t => t.Value is IDataRequest);
            return actionArguments.Value as IDataRequest;
        }

        public static string FindRedirectURL(String controllerName, SPFRedirectAttribute redirectAttribute) {
            if (redirectAttribute != null && redirectAttribute.URL != null) {
                return redirectAttribute.Avoid ? null : String.Format(ConventionPattern, redirectAttribute.URL);
            }
            return String.Format(ConventionPattern, WebAPIUtil.RemoveControllerSufix(controllerName));
        }

        private static SPFRedirectAttribute LookupAttribute(HttpActionContext actionContext) {
            var actionAttribute = actionContext.ActionDescriptor.GetCustomAttributes<SPFRedirectAttribute>().FirstOrDefault();
            return actionAttribute ??
                   actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<SPFRedirectAttribute>().FirstOrDefault();
        }
    }
}