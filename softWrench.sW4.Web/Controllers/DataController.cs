using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Controllers.Routing;
using softWrench.sW4.Web.Security;
using softWrench.sW4.Web.SPF;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using softWrench.sW4.Web.Util;
using System.Diagnostics;
using softWrench.sW4.AUTH;

namespace softWrench.sW4.Web.Controllers {

    [ApplicationAuthorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class DataController : ApiController {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(DataController));
        private readonly NextSchemaRouter _nextSchemaRouter = new NextSchemaRouter();
        protected readonly DataSetProvider DataSetProvider = DataSetProvider.GetInstance();
        private readonly SuccessMessageHandler _successMessageHandler = new SuccessMessageHandler();
        protected readonly CompositionExpander COMPOSITIONExpander;
        private readonly I18NResolver _i18NResolver;
        protected readonly IContextLookuper ContextLookuper;

        public DataController(I18NResolver i18NResolver, IContextLookuper contextLookuper, CompositionExpander compositionExpander) {
            _i18NResolver = i18NResolver;
            ContextLookuper = contextLookuper;
            COMPOSITIONExpander = compositionExpander;
        }

        private const string MockingMaximoKey = "%%mockmaximo";
        private const string MockingErrorKey = "%%mockerror";

        /// <summary>
        ///  Method responsible for fetching either list or detail data from the server
        /// </summary>
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [NotNull]
        public IApplicationResponse Get(string application, [FromUri] DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            RequestUtil.ValidateMockError(Request);

            ValidateHashSecurity(request);

            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web);
            ContextLookuper.FillContext(request.Key);
            var response = DataSetProvider.LookupAsBaseDataSet(application).Get(applicationMetadata, user, request);
            response.Title = _i18NResolver.I18NSchemaTitle(response.Schema);
            var schemaMode = request.Key.Mode ?? response.Schema.Mode;
            response.Mode = schemaMode.ToString().ToLower();

            return response;
        }

        private static void ValidateHashSecurity(DataRequestAdapter request) {
            if (request.Id == null) {
                return;
            }

            if (request.Id != null && request.HmacHash == null) {
                throw new InvalidOperationException(
                    "You don´t have enough permissions to see that register. contact your administrator");
            }
            if (!AuthUtils.HmacShaEncode(request.Id).Equals(request.HmacHash)) {
                throw new InvalidOperationException(
                    "You don´t have enough permissions to see that register. contact your administrator");
            }
        }


        /// <summary>
        /// API Method to provide updated Association Options, for given application, according the Association Update Request
        /// This method will provide options for depedant associations, lookup association and autocomplete association.
        /// </summary>
        ///
        [NotNull]
        [HttpPost]
        public GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>> UpdateAssociation(string application,
            [FromUri] AssociationUpdateRequest request, JObject currentData) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            ContextLookuper.FillContext(request.Key);
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web);

            var baseDataSet = DataSetProvider.LookupAsBaseDataSet(application);


            var response = baseDataSet.UpdateAssociations(applicationMetadata, request, currentData);

            return response;
        }
        //TODO: have u considered creating an object, much like DataRequestAdapter, for combining these parameters?
        // ==> This could be set in shared project and reused by the Ipad App.
        // ==> On Angular, we could create a .js class also (still lots to do there...)
        /// <summary>
        /// API Method to handle Delete operations
        /// </summary>
        public IApplicationResponse Delete([NotNull] string application, [NotNull] string id,
            ClientPlatform platform, [NotNull] string currentSchemaKey, string nextSchemaKey = null, bool mockmaximo = false) {

            var schemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, currentSchemaKey, platform);
            ContextLookuper.FillContext(schemaKey);
            var nextschemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, nextSchemaKey, platform);
            var response = DoExecute(application, new JObject(), id, OperationConstants.CRUD_DELETE, schemaKey, mockmaximo, nextschemaKey, platform);
            var defaultMsg = String.Format("{0} {1} deleted successfully", application, id);
            response.SuccessMessage = _i18NResolver.I18NValue("general.defaultcommands.delete.confirmmsg", defaultMsg, null, new object[]{
                application, id});
            return response;
        }

        /// <summary>
        /// API Method to handle Update operations
        /// </summary>
        [HttpPut]
        public IApplicationResponse Put([FromUri] string application, [FromUri] string id, [NotNull] JObject json,
             ClientPlatform platform, string currentSchemaKey = null, string nextSchemaKey = null, bool mockmaximo = false) {

            var schemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, currentSchemaKey, platform);
            ContextLookuper.FillContext(schemaKey);
            var nextschemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, nextSchemaKey, platform);
            return DoExecute(application, json, id, OperationConstants.CRUD_UPDATE, schemaKey, mockmaximo, nextschemaKey, platform);
        }

        /// <summary>
        /// API Method to handle Insert operations
        /// </summary>
        public IApplicationResponse Post([NotNull] string application, JObject json,
            ClientPlatform platform, [NotNull] string currentSchemaKey, string nextSchemaKey = null, bool mockmaximo = false) {

            Log.InfoFormat("PERFORMANCE - Data controller POST started at {0}.", DateTime.Now);
            var before = Stopwatch.StartNew();

            if (Log.IsDebugEnabled) {
                Log.DebugFormat("json received: " + json.ToString());
            }
            var schemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, currentSchemaKey, platform);
            ContextLookuper.FillContext(schemaKey);
            var nextschemaKey = _nextSchemaRouter.GetSchemaKeyFromString(application, nextSchemaKey, platform);
            var response = DoExecute(application, json, null, OperationConstants.CRUD_CREATE, schemaKey, mockmaximo, nextschemaKey, platform);

            var msDelta = LoggingUtil.MsDelta(before);
            Log.InfoFormat("PERFORMANCE - Data controller POST executed in {0} ms.", msDelta);

            return response;
        }

        /// <summary>
        /// API Method to handle generic operations
        /// </summary>
        [HttpPost]
        //TODO: modify here, and on mobile in order to have the same api as the other methods
        public IApplicationResponse Operation(String application, String operation, JObject json, ClientPlatform platform, string id = "") {
            MockingUtils.EvalMockingErrorModeActive(json, Request);
            var currentschemaKey = _nextSchemaRouter.GetSchemaKeyFromJson(application, json, true);
            var nextSchemaKey = _nextSchemaRouter.GetSchemaKeyFromJson(application, json, false);
            currentschemaKey.Platform = platform;
            //            nextSchemaKey.Platform = platform;
            var mockMaximo = MockingUtils.IsMockingMaximoModeActive(json);
            return DoExecute(application, json, id, operation, currentschemaKey, mockMaximo, nextSchemaKey, platform);
        }


        private IApplicationResponse DoExecute(string application, JObject json, string id, string operation,
            ApplicationMetadataSchemaKey currentschemaKey, bool mockMaximo, ApplicationMetadataSchemaKey nextSchemaKey, ClientPlatform platform) {
            MockingUtils.EvalMockingErrorModeActive(json, Request);
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            if (Log.IsDebugEnabled) {
                Log.Debug(json.ToString(Newtonsoft.Json.Formatting.Indented, new StringEnumConverter()));
            }
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(currentschemaKey, user, platform);

            ContextLookuper.SetInternalQueryExecution();

            var maximoResult = new MaximoResult(id, null);
            if (!mockMaximo) {
                maximoResult = DataSetProvider.LookupAsBaseDataSet(application)
                    .Execute(applicationMetadata, json, id, operation);
            }
            if (currentschemaKey.Platform == ClientPlatform.Mobile) {
                //mobile requests doesn´t have to handle success messages or redirections
                return null;
            }
            if (nextSchemaKey == null) {
                //keep on same schema unless explicetely told otherwise
                nextSchemaKey = currentschemaKey;
            }

            if (nextSchemaKey != null) {
                var response = _nextSchemaRouter.RedirectToNextSchema(applicationMetadata, operation,
                        maximoResult.Id, platform, currentschemaKey, nextSchemaKey, mockMaximo);
                response.SuccessMessage = _successMessageHandler.FillSucessMessage(applicationMetadata, maximoResult.Id, operation);
                return response;
            }
            return new BlankApplicationResponse() {
                SuccessMessage = _successMessageHandler.FillSucessMessage(applicationMetadata, maximoResult.Id, operation)
            };
        }


        /// <summary>
        /// Used for performing the menu search operation
        /// </summary>
        /// <param name="application"></param>
        /// <param name="searchFields"></param>
        /// <param name="searchText"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        [HttpGet]
        public IApplicationResponse Search(string application, string searchFields, string searchText, string schema = "list") {
            var user = SecurityFacade.CurrentUser();
            var app = MetadataProvider.Application(application);
            var schemas = app.Schemas();
            var key = new ApplicationMetadataSchemaKey(schema, SchemaMode.input, ClientPlatform.Web);
            ApplicationSchemaDefinition appSchema;

            if (!schemas.TryGetValue(key, out appSchema)) {
                throw new InvalidOperationException("schema not found");
            }

            var searchRequestDto = PaginatedSearchRequestDto.DefaultInstance(appSchema);
            searchRequestDto.SetFromSearchString(appSchema, searchFields.Split(','), searchText);

            var dataResponse = Get(application, new DataRequestAdapter() { Key = key, SearchDTO = searchRequestDto });
            //fixing the filter parameters used so that it is applied on next queries
            ((ApplicationListResult)dataResponse).PageResultDto.BuildFixedWhereClause(searchRequestDto, app.Entity);
            dataResponse.Title = appSchema.Title;
            dataResponse.Mode = SchemaMode.input.ToString().ToLower();
            return dataResponse;
            //            return View("Index", new ApplicationModel(application, "list", SchemaMode.input.ToString().ToLower(), appSchema.Title, dataResponse));
        }

        /// <summary>
        /// Used to returning a list of valid schemas to be choosen for a subsequent operation
        /// </summary>
        /// <param name="application"></param>
        /// <param name="title"></param>
        /// <param name="label"></param>
        /// <param name="placeholder"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public SchemaChoosingDataResponse ListSchemas(string application, string title, string label, string placeholder, [FromUri]ListSchemaFilter filter) {
            var resultingSchemas = new List<ApplicationSchemaDefinition>();
            var schemas = MetadataProvider.Application(application).Schemas();
            foreach (var schema in schemas) {
                var matchesMode = schema.Key.Mode == null || (schema.Key.Mode == filter.Mode);
                var matchesStereotype = filter.Stereotype == null || filter.Stereotype == schema.Value.Stereotype;
                var matchesName = filter.NamePattern == null || schema.Key.SchemaId.StartsWith(filter.NamePattern);
                if (matchesMode && matchesStereotype && matchesName) {
                    if (!schema.Value.Abstract) {
                        resultingSchemas.Add(schema.Value);
                    } else {
                        var fixedTitle = schema.Value.GetProperty(ApplicationSchemaPropertiesCatalog.WindowPopupHeaderTitle);
                        if (fixedTitle != null) {
                            title = fixedTitle;
                        }
                    }
                }
            }


            return new SchemaChoosingDataResponse(resultingSchemas, label, placeholder) { Title = title };
        }

        public class ListSchemaFilter {
            public string NamePattern {
                get; set;
            }
            public SchemaStereotype? Stereotype {
                get; set;
            }
            public SchemaMode Mode {
                get; set;
            }



        }




    }
}