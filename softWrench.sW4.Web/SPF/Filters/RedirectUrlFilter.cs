using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.SPF.Filters {
    /// <summary>
    /// Responsible for placing the url of the html page after the execution of the method.<p/>
    /// This html will be handled in clint-side allowing to switch page sections by ajax.
    /// </summary>
    public class RedirectUrlFilter : ActionFilterAttribute {



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
            var applicationResponse = objectContent.Value as IApplicationResponse;

            if (value != null) {
                value.AliasURL = FindAliasUrl(applicationResponse);
            }

            ClearSchemaIfCached(applicationResponse, actionExecutedContext);


            if (value == null || value is BlankApplicationResponse) {
                return;
            }

            if (value.RedirectURL == null) {
                value.RedirectURL = url;
            }



            if (value.CrudSubTemplate == null && redirectAttribute != null && redirectAttribute.CrudSubTemplate != null) {
                if (!redirectAttribute.CrudSubTemplate.StartsWith("/Content", StringComparison.CurrentCultureIgnoreCase)) {
                    redirectAttribute.CrudSubTemplate = "/Content" + redirectAttribute.CrudSubTemplate;
                }
                value.CrudSubTemplate = redirectAttribute.CrudSubTemplate;
            }
            value.TimeStamp = DateTime.UtcNow;
            //this should avoid unwanted conversions on JsonDateTimeConverter
            DateTime.SpecifyKind(value.TimeStamp, DateTimeKind.Utc);

            var actionArguments = LookupArguments(actionExecutedContext.ActionContext);
            if (actionArguments != null && !String.IsNullOrWhiteSpace(actionArguments.Title)) {
                value.Title = actionArguments.Title;
            } else if (value.Title == null && redirectAttribute != null) {
                value.Title = redirectAttribute.Title;
            }
        }

        private string FindAliasUrl(IApplicationResponse applicationResponse) {
            return applicationResponse?.Schema?.GetProperty(ApplicationSchemaPropertiesCatalog.SchemaAliasUrl);

        }

        private void ClearSchemaIfCached(IApplicationResponse applicationResponse, HttpActionExecutedContext actionExecutedContext) {
            if (applicationResponse == null || applicationResponse.Schema == null || applicationResponse.Schema.IgnoreCache) {
                // if there´s no schema to cache, just return 
                return;
            }
            var cachedSchemas = RequestUtil.GetValue(actionExecutedContext.Request, "cachedschemas");
            if (cachedSchemas != null && cachedSchemas.Contains(";" + applicationResponse.Schema.GetApplicationKey() + ";")) {
                //to reduce payload SWWEB-1317
                applicationResponse.CachedSchemaId = applicationResponse.Schema.SchemaId;
                var applicationName = applicationResponse.ApplicationName;
                applicationResponse.Schema = new ApplicationSchemaDefinition {
                    //to play safe since it might be server-side delegation methods to the schema
                    //TODO: think of a way to avoid serialization
                    ApplicationName = applicationName,
                    SchemaId = applicationResponse.Schema.SchemaId,
                    Mode = applicationResponse.Schema.Mode
                };

            }
        }

        private static IDataRequest LookupArguments(HttpActionContext actionContext) {
            var actionArguments = actionContext.ActionArguments.FirstOrDefault(t => t.Value is IDataRequest);
            return actionArguments.Value as IDataRequest;
        }

        public static string FindRedirectURL(String controllerName, SPFRedirectAttribute redirectAttribute) {
            if (redirectAttribute != null && redirectAttribute.URL != null) {
                if (redirectAttribute.Avoid) {
                    return null;
                }
                if (redirectAttribute.URL.EndsWith(".html")) {
                    //this is not required, here just to make the API more robust
                    redirectAttribute.URL = redirectAttribute.URL.Replace(".html", "");
                }
                return String.Format(SPFRedirectAttribute.ConventionPattern, redirectAttribute.URL);
            }
            return String.Format(SPFRedirectAttribute.ConventionPattern, WebAPIUtil.RemoveControllerSufix(controllerName));
        }

        private static SPFRedirectAttribute LookupAttribute(HttpActionContext actionContext) {
            var actionAttribute = actionContext.ActionDescriptor.GetCustomAttributes<SPFRedirectAttribute>().FirstOrDefault();
            return actionAttribute ??
                   actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<SPFRedirectAttribute>().FirstOrDefault();
        }
    }
}