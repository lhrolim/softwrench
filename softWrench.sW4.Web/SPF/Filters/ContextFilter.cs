using System;
using System.Collections.Generic;
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
        private const string ConstrainedProfilesKey = "constrainedprofiles";
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
            var constrainedProfilesSt = RequestUtil.GetValue(actionContext.Request, ConstrainedProfilesKey);
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
            var constrainedProfiles = new HashSet<int?>();

            if (constrainedProfilesSt != null) {
                var profilesToLimit = constrainedProfilesSt.Split(',');
                foreach (var profile in profilesToLimit) {
                    constrainedProfiles.Add(Convert.ToInt32(profile));
                }
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
                CurrentSelectedProfile = string.IsNullOrEmpty(profileAsString) ? (int?)null : int.Parse(profileAsString),
                ConstrainedProfiles = constrainedProfiles,
                MetadataParameters = PropertyUtil.ConvertToDictionary(currentMetadataParameter)
            });

            base.OnActionExecuting(actionContext);
        }



    }
}