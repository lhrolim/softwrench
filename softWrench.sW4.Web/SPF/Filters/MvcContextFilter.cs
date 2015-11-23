using cts.commons.portable.Util;
using softWrench.sW4.Security.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ContextLookuper = softWrench.sW4.Web.Security.ContextLookuper;

namespace softWrench.sW4.Web.SPF.Filters {
    /// <summary>
    /// Responsible for placing the url of the html page after the execution of the method.<p/>
    /// This html will be handled in clint-side allowing to switch page sections by ajax.
    /// </summary>
    public class MvContextFilter : System.Web.Mvc.IActionFilter {

        private const string CurrentModuleKey = "currentmodule";
        private const string CurrentMetadataKey = "currentmetadata";
        private const string CurrentMetadataParameterKey = "currentmetadataparameter";

        public void OnActionExecuting(ActionExecutingContext actionContext) {
            if (!actionContext.HttpContext.User.Identity.IsAuthenticated) {
                return;
            }
            var currentModule = GetValue(actionContext, CurrentModuleKey);
            var currentMetadataId = GetValue(actionContext, CurrentMetadataKey);
            var currentMetadataParameter = GetValue(actionContext, CurrentMetadataParameterKey);
            ApplicationLookupContext appCtx = null;
            if (currentMetadataId != null) {
                appCtx = new ApplicationLookupContext { MetadataId = currentMetadataId };
            }
            var instance = ContextLookuper.GetInstance();
            instance.AddContext(new ContextHolder() { Module = currentModule, ApplicationLookupContext = appCtx, MetadataParameters= PropertyUtil.ConvertToDictionary(currentMetadataParameter)},true);
//            instance.RegisterHttpContext(actionContext.HttpContext.re);
        }

        private static string GetValue(ActionExecutingContext actionContext, string key) {
            string value = null;
            var request = actionContext.HttpContext.Request;
            var headers = request.Headers.GetValues(key);
            if (headers != null) {
                value = headers.First();
            } else {
                var values = request.QueryString.GetValues(key);
                if (values != null) {
                    value = values[0];
                } else {
                    values = request.Params.GetValues(key);
                    if (values != null) {
                        value = values[0];
                    }
                }
            }
            return "null".Equals(value) ? null : value;
        }



        public void OnActionExecuted(ActionExecutedContext filterContext) {
            //NOOP
        }
    }
}