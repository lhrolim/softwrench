using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Web.Models.Home;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    public class RouteController : Controller {
        private const string Index = "~/Views/Home/Index.cshtml";

        private readonly HomeService _homeService;
        private readonly EntityRepository _entityRepository;
        private readonly RouteService _routeService;

        public RouteController(HomeService homeService, EntityRepository entityRepository, RouteService routeService) {
            _homeService = homeService;
            _entityRepository = entityRepository;
            _routeService = routeService;
        }

        public async Task<ActionResult> Route(string application, string extra, string uid, [FromUri] string siteid) {
            var user = SecurityFacade.CurrentUser();

            // redirects if change password is needed
            if (await _homeService.VerifyChangePassword(user, Response)) {
                return null;
            }

            // verify if it's a special case
            var customPath = _routeService.CustomPath(application, user);
            if (customPath != null) {
                var model = _homeService.BaseHomeModel(Request, user);
                model.Url = customPath;
                model.FromRoute = true;
                return View(Index, model);
            }

            application = _routeService.UndecorateApplication(application);

            var appMetadata = GetAppMetadata(application);
            // no application -> page not found
            if (appMetadata == null) {
                return NotFound(user);
            }

            // has uid -> routes with uid
            if (!string.IsNullOrEmpty(uid)) {
                return RouteWithUid(user, appMetadata, uid);
            }

            var schema = GetSchema(appMetadata, extra);
            // no schema -> page not found
            if (schema == null) {
                return NotFound(user);
            }

            // detailnew
            if (SchemaStereotype.DetailNew.Equals(schema.Stereotype)) {
                return RouteView(user, appMetadata, schema, null);
            }

            // list
            if (SchemaStereotype.List.Equals(schema.Stereotype)) {
                return RouteListView(user, appMetadata, schema);
            }

            // list or detailnew -> no need for id
            if (!SchemaStereotype.Detail.Equals(schema.Stereotype)) {
                return RouteView(user, appMetadata, schema, null);
            }

            // is detail

            var entityName = appMetadata.Entity;
            // no entity -> page not found
            if (string.IsNullOrEmpty(entityName)) {
                return NotFound(user);
            }
            var entityMetadata = MetadataProvider.Entity(entityName);
            // no entity -> page not found
            if (entityMetadata == null) {
                return NotFound(user);
            }

            // no userid -> routes with uid
            if (string.IsNullOrEmpty(appMetadata.UserIdFieldName) || appMetadata.UserIdFieldName.Equals(appMetadata.IdFieldName)) {
                return RouteWithUid(user, appMetadata, extra, schema);
            }

            var userId = extra;
            if (!string.IsNullOrEmpty(siteid)) {
                return RouteWithUserIdSiteId(user, appMetadata, userId, siteid);
            }

            var datamaps = await _entityRepository.GetIdAndSiteIdByUserId(entityMetadata, userId);

            // no uids found -> page not found
            if (datamaps.Count == 0) {
                return NotFound(user);
            }

            // multiples uids found -> route to choose page
            if (datamaps.Count > 1) {
                return ManyUserIds(user, application, schema.SchemaId, userId);
            }

            var datamap = datamaps.First();
            // no uid found -> page not found
            if (string.IsNullOrEmpty(entityMetadata.IdFieldName) || !datamap.ContainsKey(entityMetadata.IdFieldName)) {
                return NotFound(user);
            }
            var foundUid = datamap[entityMetadata.IdFieldName];
            return foundUid == null ? NotFound(user) : RouteView(user, appMetadata, schema, foundUid.ToString());
        }

        private ActionResult RouteWithUid(InMemoryUser user, CompleteApplicationMetadataDefinition appMetadata, string uid, ApplicationSchemaDefinition detailSchema = null) {
            detailSchema = detailSchema ?? GetDetailSchema(appMetadata);
            return detailSchema == null ? NotFound(user) : RouteView(user, appMetadata, detailSchema, uid);
        }

        private ActionResult RouteWithUserIdSiteId(InMemoryUser user, CompleteApplicationMetadataDefinition appMetadata, string iserId, string siteId, ApplicationSchemaDefinition detailSchema = null) {
            detailSchema = detailSchema ?? GetDetailSchema(appMetadata);
            return detailSchema == null ? NotFound(user) : RouteView(user, appMetadata, detailSchema, iserId, siteId);
        }

      

        private ActionResult RouteView(InMemoryUser user, IApplicationIdentifier appMetadata, [NotNull]ApplicationSchemaDefinition schema, string id) {
            var model = _homeService.BaseHomeModel(Request,user, schema);
            model.Url = _homeService.GetUrlFromApplication(appMetadata.ApplicationName, schema, id);
            return View(Index, model);
        }

        private ActionResult RouteView(InMemoryUser user, IApplicationIdentifier appMetadata, ApplicationSchemaDefinition schema, string userid, string siteid) {
            var model = _homeService.BaseHomeModel(Request,user, schema);
            model.Url = _homeService.GetUrlFromApplication(appMetadata.ApplicationName, schema, userid, siteid);
            return View(Index, model);
        }

        private ActionResult RouteListView(InMemoryUser user, IApplicationIdentifier appMetadata, ApplicationSchemaDefinition schema) {
            var model = _homeService.BaseHomeModel(Request,user, schema);
            model.Url = _homeService.GetUrlFromApplication(appMetadata.ApplicationName, schema, null);
            model.RouteListInfo = new RouteListInfo {
                ApplicationName = appMetadata.ApplicationName,
                Schemaid = schema.SchemaId
            };
            return View(Index, model);
        }

        private ActionResult NotFound(InMemoryUser user) {
            var model = _homeService.BaseHomeModel(Request, user);
            model.Url = "api/generic/RoutePage/PageNotFound";
            model.Message = "Page not found.";
            model.MessageType = "error";
            model.FromRoute = true;
            return View(Index, model);
        }

        private ActionResult ManyUserIds(InMemoryUser user, string application, string schemaid, string userid) {
            var model = _homeService.BaseHomeModel(Request, user);
            model.Url = string.Format("api/generic/RoutePage/ManyUserIds?application={0}&schemaid={1}&userid={2}", application, schemaid, userid);
            model.Title = "Choose one";
            model.FromRoute = true;
            return View(Index, model);
        }

        private static CompleteApplicationMetadataDefinition GetAppMetadata(string applicationName) {
            try {
                return MetadataProvider.Application(applicationName);
            } catch (Exception) {
                return null;
            }
        }

        private ApplicationSchemaDefinition GetSchema(CompleteApplicationMetadataDefinition appMetadata, string extra) {
            var schemaInfo = GetSchemaInfo(appMetadata.ApplicationName);
            if (schemaInfo == null) {
                return null;
            }

            // extra null - tries to find the list schema
            if (string.IsNullOrEmpty(extra)) {
                return InnerGetSchema(appMetadata, schemaInfo.listSchema);
            }

            // extra "new" - tries to find the detailnew schema
            if ("new".EqualsIc(extra)) {
                return InnerGetSchema(appMetadata, schemaInfo.newDetailSchema);
            }

            // tries to find the detail schema
            return GetDetailSchema(appMetadata);
        }

        private ApplicationSchemaDefinition GetDetailSchema(CompleteApplicationMetadataDefinition appMetadata) {
            var schemaInfo = GetSchemaInfo(appMetadata.ApplicationName);
            return schemaInfo == null ? null : InnerGetSchema(appMetadata, schemaInfo.detailSchema);
        }

        private ApplicationSchemaDefinition InnerGetSchema(CompleteApplicationMetadataDefinition appMetadata, string schemaId) {
            return string.IsNullOrEmpty(schemaId) ? null : appMetadata.Schema(new ApplicationMetadataSchemaKey(schemaId, null, ClientPlatform.Web));
        }

        private SchemaInfo GetSchemaInfo(string application) {
            var routeInfo = _routeService.GetRouteInfo();
            return !routeInfo.schemaInfo.ContainsKey(application) ? null : routeInfo.schemaInfo[application];
        }
    }
}