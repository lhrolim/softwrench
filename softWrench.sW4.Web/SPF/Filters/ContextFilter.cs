using cts.commons.portable.Util;
using softWrench.sW4.Security.Context;
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
        private const string CurrentProfileKey = "currentprofile";
        private const string PrintMode = "printmode";
        private const string ScanMode = "scanmode";
        private const string OfflineMode = "offlinemode";
        private const string MockMaximo = "mockmaximo";
        private const string MockSecurity = "mocksecurity";
        private const string CurrentMetadataParameterKey = "currentmetadataparameter";

        public override void OnActionExecuting(HttpActionContext actionContext) {

            var currentModule = RequestUtil.GetValue(actionContext.Request, CurrentModuleKey);
            var currentMetadataId = RequestUtil.GetValue(actionContext.Request, CurrentMetadataKey);
            var profileAsString = RequestUtil.GetValue(actionContext.Request, CurrentProfileKey);
            var printMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, PrintMode));
            var scanMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, ScanMode));
            var offlineMode = "true".Equals(RequestUtil.GetValue(actionContext.Request, OfflineMode));
            var mockMaximo = "true".Equals(RequestUtil.GetValue(actionContext.Request, MockMaximo));
            var mockSecurity = "true".Equals(RequestUtil.GetValue(actionContext.Request, MockMaximo));
            var currentMetadataParameter = RequestUtil.GetValue(actionContext.Request, CurrentMetadataParameterKey);
            ApplicationLookupContext appCtx = null;
            if (currentMetadataId != null) {
                appCtx = new ApplicationLookupContext { MetadataId = currentMetadataId };
            }
            var instance = ContextLookuper.GetInstance();
            instance.AddContext(new ContextHolder {
                Module = currentModule,
                ApplicationLookupContext = appCtx,
                PrintMode = printMode,
                ScanMode = scanMode,
                OfflineMode = offlineMode,
                MockMaximo = mockMaximo,
                MockSecurity = mockSecurity,
                CurrentSelectedProfile = profileAsString == null ? (int?) null : int.Parse(profileAsString),
                MetadataParameters = PropertyUtil.ConvertToDictionary(currentMetadataParameter)
            }, true);

            base.OnActionExecuting(actionContext);
        }



    }
}