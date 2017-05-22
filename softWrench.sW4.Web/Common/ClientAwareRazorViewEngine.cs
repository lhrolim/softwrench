﻿using System;
using System.Web.Mvc;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers.Security;
using softWrench.sW4.Web.Controllers;

namespace softWrench.sW4.Web.Common {
    public class ClientAwareRazorViewEngine : RazorViewEngine {

        private const string ClientPattern = "~/Content/Customers/{0}/htmls";
        private const string DefaultPattern = "~/Views";
        private const string DefaultLayout = "~/Views/Shared/_Layout.cshtml";
        private const string NoMenuLayout = "~/Views/Shared/_NoMenuLayout.cshtml";
        private const string RawLayout = "~/Views/Shared/Raw.cshtml";
        private const string ClientLayoutPattern = "~/Content/Customers/{0}/htmls/Shared/_Layout.cshtml";

        public ClientAwareRazorViewEngine() {
            ViewLocationFormats = new string[] {
                "%1/{1}/{0}.cshtml"
            };
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {

            if (String.IsNullOrEmpty(masterPath)) {
                masterPath = FetchMasterPath(controllerContext, viewPath);
            }
            var clientPath = string.Format(ClientPattern, ApplicationConfiguration.ClientName);


            var pathExists = base.FileExists(controllerContext,viewPath.Replace("%1", clientPath));

            if (pathExists) {
                return base.CreateView(
                    controllerContext,
                    viewPath.Replace("%1", clientPath),
                    masterPath.Replace("%1", clientPath)
                    );
            }

            return base.CreateView(
                controllerContext,
                viewPath.Replace("%1", DefaultPattern),
                masterPath.Replace("%1", DefaultPattern)
                );

        }

        private string FetchMasterPath(ControllerContext controllerContext, string viewPath) {
            var clientLayout = string.Format(ClientLayoutPattern, ApplicationConfiguration.ClientName);

            var noMenuAttribute = Attribute.GetCustomAttribute(controllerContext.Controller.GetType(), typeof(NoMenuController));
            if (noMenuAttribute != null) {
                return NoMenuLayout;
            }

            var rawAttribute = Attribute.GetCustomAttribute(controllerContext.Controller.GetType(), typeof(RawController));
            if (rawAttribute != null) {
                return RawLayout;
            }
            if (!controllerContext.HttpContext.User.Identity.IsAuthenticated ||
                controllerContext.Controller is softWrench.sW4.Web.Controllers.ReportController) {
                return "";

            }
            if (viewPath.Contains("SignIn")) {
                //FIX for HAP-780
                return "";
            }


            if (base.FileExists(controllerContext, clientLayout)) {
                return clientLayout;
            }
            return DefaultLayout;
        }


        protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
            return base.FileExists(controllerContext, virtualPath.Replace("%1", String.Format(ClientPattern, ApplicationConfiguration.ClientName)))
                || base.FileExists(controllerContext, virtualPath.Replace("%1", DefaultPattern));
        }

        //        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
        //            if (controllerContext == null) {
        //                return new ViewEngineResult(new string[] { });
        //            }
        //
        //            var viewEngineResult = base.FindView(controllerContext, viewName, masterName, useCache);
        //            return viewEngineResult;
        //        }


    }
}