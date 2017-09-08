using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using NHibernate.Util;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Web.Models.Home {
    public class RouteService : ISingletonComponent, ISWEventListener<RefreshMetadataEvent> {

        private static readonly Dictionary<string, string> ApplicationDecorationDict = new Dictionary<string, string>{
            {"user", "person"},
            {"sr", "servicerequest"},
            {"quicksr", "quickservicerequest"},
            {"wo", "workorder"},
            {"securitygroup", "_UserProfile"},
            {"error", "_SoftwrenchError"}
        };

        private static readonly Dictionary<string, string> CustomPathDict = new Dictionary<string, string>
        {
            {"about", "api/generic/Configuration/About"},
            {"dashboard", "api/generic/DashBoard/LoadPreferred"}
        };

        private const string MyProfileBaseURL = "api/data/person?userid={0}&applicationName=person&key[schemaId]=myprofiledetail&key[mode]=input&key[platform]=web&currentSchemaKey=myprofiledetail";

        private readonly IMemoryContextLookuper _lookuper;

        // info of the main schemas used to route from url
        private RouteInfo _routeInfo = null;
        // controls wich clients already have the route info
        // made to avoid send the info every home request
        private readonly IList<string> _routeInfoClients = new List<string>();
        // uses datetime to clear the list after 24h
        private DateTime _routeInfoClientsTime = DateTime.Now;

        public RouteService(IMemoryContextLookuper lookuper) {
            _lookuper = lookuper;
            GetRouteInfo();
        }

        public RouteInfo GetRouteInfo() {
            // uses a inner methos to enable both a virtual method and the use on constructor
            return InnerGetRouteInfo();
        }

        public virtual RouteInfo GetRouteInfo(HttpRequestBase request) {
            var swCookie = request.Cookies["swcookie"];
            if (swCookie == null) {
                return null;
            }

            if ((DateTime.Now - _routeInfoClientsTime).TotalHours > 24) {
                _routeInfoClientsTime = DateTime.Now;
                _routeInfoClients.Clear();
            }

            var client = swCookie.Value;
            if (string.IsNullOrWhiteSpace(client) || _routeInfoClients.Contains(client)) {
                return null;
            }
            _routeInfoClients.Add(client);
            return InnerGetRouteInfo();
        }

        public virtual void ResetRouteInfo() {
            _routeInfo = null;
            InnerGetRouteInfo();
            _routeInfoClients.Clear();
            _routeInfoClientsTime = DateTime.Now;
        }

        public virtual void HandleEvent(RefreshMetadataEvent eventToDispatch) {
            ResetRouteInfo();
        }

        public virtual string UndecorateApplication(string application) {
            var dict = GetApplicationDecorationDict();
            return dict.ContainsKey(application) ? dict[application] : application;
        }

        public virtual string CustomPath(string application, InMemoryUser user) {
            var dict = GetCustomPathDict();
            if ("myprofile".Equals(application)) {
                return string.Format(MyProfileBaseURL, user.Login);
            }
            return dict.ContainsKey(application.ToLowerInvariant()) ? dict[application.ToLowerInvariant()] : null;
        }

        // to enable child classes alter the dict
        protected virtual Dictionary<string, string> GetApplicationDecorationDict() {
            return ApplicationDecorationDict;
        }

        // to enable child classes alter the dict
        protected virtual Dictionary<string, string> GetCustomPathDict() {
            return CustomPathDict;
        }

        protected virtual RouteInfo InnerGetRouteInfo() {
            if (_routeInfo != null) {
                if (_routeInfo.contextPath == null) {
                    // workaround - context path only ready after the firt http req
                    _routeInfo.contextPath = GetContextPath();
                }
                return _routeInfo;
            }

            var routeInfo = new RouteInfo { schemaInfo = new Dictionary<string, SchemaInfo>(StringComparer.OrdinalIgnoreCase) };
            MetadataProvider.Applications(true).Where(a => a.IsSupportedOnPlatform(ClientPlatform.Web)).ForEach(app => {
                var listSchema = app.MainListSchema ?? GetSchema(app, SchemaStereotype.List, "list");
                var newDetailSchema = app.MainNewDetailSchema ?? GetSchema(app, SchemaStereotype.DetailNew, "newdetail");
                var detailSchema = app.MainDetailSchema ?? GetSchema(app, SchemaStereotype.Detail, "detail", "editdetail");
                var mainSchemaInfo = new SchemaInfo {
                    listSchema = listSchema?.SchemaId,
                    newDetailSchema = newDetailSchema?.SchemaId,
                    detailSchema = detailSchema?.SchemaId,
                    ListApplicationName = listSchema?.ApplicationName,
                    DetailApplicationName = detailSchema?.ApplicationName,
                    DetailNewApplicationName = newDetailSchema?.ApplicationName
                };
                routeInfo.schemaInfo.Add(app.ApplicationName.ToLowerInvariant(), mainSchemaInfo);
            });
            routeInfo.contextPath = GetContextPath();

            _routeInfo = routeInfo;
            return _routeInfo;
        }

        protected virtual string GetContextPath() {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            if (fullContext == null) {
                return null;
            }
            var contextPath = fullContext.Context;
            if (contextPath == null) {
                return null;
            }
            if (contextPath.EndsWith("/")) {
                contextPath = contextPath.Substring(0, contextPath.Length - 1);
            }
            return contextPath;
        }

        protected virtual ApplicationSchemaDefinition GetSchema(CompleteApplicationMetadataDefinition appMetadata,
            SchemaStereotype stereotype, params string[] ids) {
            var schemas = appMetadata.Schemas();
            var stereotypeSchemas = schemas.Where(pair => stereotype.Equals(pair.Value.Stereotype) && CheckPlatform(pair.Value) && !pair.Value.Abstract).ToList();
            if (stereotypeSchemas.Count() == 1) {
                return stereotypeSchemas.First().Value;
            }
            if (ids == null || ids.Length == 0) {
                return null;
            }
            var idSchemas = stereotypeSchemas.Where(pair => ids.Any(id => id.EqualsIc(pair.Value.SchemaId))).ToList();
            return idSchemas.Count() == 1 ? idSchemas.First().Value : null;
        }

        protected virtual bool CheckPlatform(ApplicationSchemaDefinition schema) {
            return schema.Platform == null || ClientPlatform.Web.Equals(schema.Platform);
        }
    }
}