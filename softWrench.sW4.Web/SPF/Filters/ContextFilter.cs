using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;
using log4net;
using softWrench.sW4.Data.API;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using softwrench.sw4.Hapag.Data;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Util;
using ContextLookuper = softWrench.sW4.Web.Security.ContextLookuper;

namespace softWrench.sW4.Web.SPF.Filters {
    /// <summary>
    /// Responsible for placing the url of the html page after the execution of the method.<p/>
    /// This html will be handled in clint-side allowing to switch page sections by ajax.
    /// </summary>
    public class ContextFilter : ActionFilterAttribute {

        private const string CurrentModuleKey = "currentmodule";
        private const string CurrentMetadataKey = "currentmetadata";
        private const string CurrentMetadataParameterKey = "currentmetadataparameter";
        private const string PrintMode = "printmode";

        public override void OnActionExecuting(HttpActionContext actionContext) {

            var currentModule = RequestUtil.GetValue(actionContext.Request, CurrentModuleKey);
            var currentUSer = SecurityFacade.CurrentUser();
            if (currentModule != null && currentUSer != null) {

                FunctionalRole fr;
                Enum.TryParse(currentModule, true, out fr);
                if (!currentUSer.IsInRole(fr.ToString())) {
                    throw new InvalidOperationException(
                        "this user is not allowed for this role. Please contact your administrator");
                }
            }

            var currentMetadataId = RequestUtil.GetValue(actionContext.Request, CurrentMetadataKey);
            var currentMetadataParameter = RequestUtil.GetValue(actionContext.Request, CurrentMetadataParameterKey);
            var printMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, PrintMode));
            ApplicationLookupContext appCtx = null;
            if (currentMetadataId != null) {
                appCtx = new ApplicationLookupContext {
                    MetadataId = currentMetadataId
                };
            }
            ContextLookuper.AddContext(new ContextHolder() {
                Module = currentModule, ApplicationLookupContext = appCtx, PrintMode = printMode, MetadataParameters = PropertyUtil.ConvertToDictionary(currentMetadataParameter)
            }, true);
            base.OnActionExecuting(actionContext);
        }



    }
}