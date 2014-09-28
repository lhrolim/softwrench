﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Providers.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Common {
    public class ClientAwareRazorViewEngine : RazorViewEngine {

        private const string ClientPattern = "~/Content/Customers/{0}/html";
        private const string DefaultPattern = "~/Views";
        private const string DefaultLayout = "~/Views/Shared/_Layout.cshtml";
        private const string ClientLayoutPattern = "~/Content/Customers/{0}/html/Shared/_Layout.cshtml";

        public ClientAwareRazorViewEngine() {
            ViewLocationFormats = new string[] {
                "%1/{1}/{0}.cshtml"
            };
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {

            if (String.IsNullOrEmpty(masterPath)) {
                masterPath = FetchMasterPath(controllerContext);
            }
            var clientPath = String.Format(ClientPattern, ApplicationConfiguration.ClientName);

            if (base.FileExists(controllerContext,
                viewPath.Replace("%1", clientPath))) {
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

        private string FetchMasterPath(ControllerContext controllerContext) {
            var clientLayout = String.Format(ClientLayoutPattern, ApplicationConfiguration.ClientName);
            if (!controllerContext.HttpContext.User.Identity.IsAuthenticated ||
                controllerContext.Controller is softWrench.sW4.Web.Controllers.ReportController) {
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