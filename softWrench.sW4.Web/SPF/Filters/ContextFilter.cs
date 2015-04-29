using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;
using log4net;
using softWrench.sW4.Data.API;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
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
        private const string PrintMode = "printmode";
        private const string OfflineMode = "offlinemode";

        public override void OnActionExecuting(HttpActionContext actionContext) {

            var currentModule = RequestUtil.GetValue(actionContext.Request, CurrentModuleKey);
            var currentMetadataId = RequestUtil.GetValue(actionContext.Request, CurrentMetadataKey);
            var printMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, PrintMode));
            var offlineMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, OfflineMode));
            ApplicationLookupContext appCtx = null;
            if (currentMetadataId != null) {
                appCtx = new ApplicationLookupContext { MetadataId = currentMetadataId };
            }
			var instance = ContextLookuper.GetInstance();
            instance.AddContext(new ContextHolder() { Module = currentModule, ApplicationLookupContext = appCtx,PrintMode = printMode,OfflineMode = offlineMode}, true);
            base.OnActionExecuting(actionContext);
        }



    }
}