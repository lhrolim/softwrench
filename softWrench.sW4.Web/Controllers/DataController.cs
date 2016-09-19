using cts.commons.web.Attributes;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Controllers.Routing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class DataController : ApiController {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(DataController));
        private readonly NextSchemaRouter _nextSchemaRouter = new NextSchemaRouter();
        protected readonly DataSetProvider DataSetProvider = DataSetProvider.GetInstance();
        private readonly SuccessMessageHandler _successMessageHandler = new SuccessMessageHandler();
        protected readonly CompositionExpander CompositionExpander;
        private readonly I18NResolver _i18NResolver;
        protected readonly IContextLookuper ContextLookuper;
        private readonly IAuditManager _auditManager;

        public DataController(I18NResolver i18NResolver, IContextLookuper contextLookuper, CompositionExpander expander, IAuditManager auditManager) {
            _i18NResolver = i18NResolver;
            ContextLookuper = contextLookuper;
            CompositionExpander = expander;
            _auditManager = auditManager;
        }

        /// <summary>
        ///  Method responsible for fetching either list or detail data from the server
        /// </summary>
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [NotNull]
        public async Task<IApplicationResponse> Get(string application, [FromUri] DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            RequestUtil.ValidateMockError(Request);

            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web, request.SchemaFieldsToDisplay);

            var securityModeCheckResult = user.VerifySecurityMode(applicationMetadata, request);

            if (securityModeCheckResult.Equals(InMemoryUserExtensions.SecurityModeCheckResult.Block)) {
                throw new SecurityException("You do not have permission to access this application. Please contact your administrator");
            }


            ContextLookuper.FillContext(request.Key);
            var response = await DataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId).Get(applicationMetadata, user, request);
            response.Title = _i18NResolver.I18NSchemaTitle(response.Schema);
            var schemaMode = request.Key.Mode ?? response.Schema.Mode;

            response.Mode = schemaMode.ToString().ToLower();
            if (securityModeCheckResult.Equals(InMemoryUserExtensions.SecurityModeCheckResult.OutPut)) {
                response.Mode = "output";
            }

            return response;
        }








        /// <summary>
        /// API Method to handle Delete operations
        /// </summary>
        public async Task<IApplicationResponse> Delete([FromUri]OperationDataRequest operationDataRequest) {
            operationDataRequest.Operation = OperationConstants.CRUD_DELETE;
            var response =await DoExecute(operationDataRequest, new JObject());
            var application = operationDataRequest.ApplicationName;
            var id = operationDataRequest.Id;
            var defaultMsg = String.Format("{0} {1} deleted successfully", application, operationDataRequest.UserId);
            response.SuccessMessage = _i18NResolver.I18NValue("general.defaultcommands.delete.confirmmsg", defaultMsg, new object[]{
                application, id});
            return response;
        }

        /// <summary>
        /// API Method to handle Update operations
        /// </summary>
        [HttpPut]
        public async Task<IApplicationResponse> Put([FromBody]JsonRequestWrapper wrapper) {
            var operationDataRequest = wrapper.RequestData;
            if (operationDataRequest.Operation == null) {
                //TODO: when Operation api is refactored, remove this one
                operationDataRequest.Operation = OperationConstants.CRUD_UPDATE;
            }
            return await DoExecute(operationDataRequest, wrapper.Json);
        }

        /// <summary>
        /// API Method to handle Insert operations
        /// </summary>
        public async Task<IApplicationResponse> Post([FromBody]JsonRequestWrapper wrapper) {
            wrapper.RequestData.Operation = OperationConstants.CRUD_CREATE;
            if (wrapper.RequestData.CurrentSchemaKey == null) {
                var schemaRepresentation = MetadataProvider.LocateNewSchema(wrapper.RequestData.ApplicationName);
                if (schemaRepresentation != null) {
                    wrapper.RequestData.CurrentSchemaKey = schemaRepresentation.SchemaId;
                }
            }
            return await DoExecute(wrapper.RequestData, wrapper.Json);
        }
        //
        //        /// <summary>
        //        /// API Method to handle Insert operations
        //        /// </summary>
        //        public IApplicationResponse CreateBatch([FromUri]OperationDataRequest operationDataRequest, [NotNull] JObject json) {
        //            operationDataRequest.IsBatch = true;
        //            return DoExecute(operationDataRequest, json);
        //        }

        /// <summary>
        /// API Method to handle generic operations
        /// </summary>
        [HttpPost]
        //TODO: modify here, and on mobile in order to have the same api as the other methods
        public async Task<IApplicationResponse> Operation(String application, String operation, JObject json, ClientPlatform platform, string id = "") {
            var currentschemaKey = _nextSchemaRouter.GetSchemaKeyFromJson(application, json, true);
            var nextSchemaKey = _nextSchemaRouter.GetSchemaKeyFromJson(application, json, false);
            currentschemaKey.Platform = platform;
            var mockMaximo = MockingUtils.IsMockingMaximoModeActive(json);
            var operationRequest = new OperationDataRequest {
                ApplicationName = application,

                Id = id,
                MockMaximo = mockMaximo,
                Operation = operation,
                Platform = platform
            };

            return await DoExecute(operationRequest, json, currentschemaKey, nextSchemaKey);
        }


        private async Task<IApplicationResponse> DoExecute(OperationDataRequest operationDataRequest, JObject json, ApplicationMetadataSchemaKey resolvedSchema = null, ApplicationMetadataSchemaKey resolvedNextSchema = null) {
            MockingUtils.EvalMockingErrorModeActive(json, Request);
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            if (Log.IsDebugEnabled) {
                Log.Debug(json.ToString(Newtonsoft.Json.Formatting.Indented, new StringEnumConverter()));
            }



            var platform = operationDataRequest.Platform;
            var currentschemaKey = resolvedSchema ??
                                   SchemaUtil.GetSchemaKeyFromString(operationDataRequest.CurrentSchemaKey, platform);

            ContextLookuper.FillContext(currentschemaKey);

            var application = operationDataRequest.ApplicationName;

            var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(currentschemaKey, user, platform);
            var mockMaximo = operationDataRequest.MockMaximo;

            //mocked instance by default
            var maximoResult = new TargetResult(null, null, null);
            var operation = operationDataRequest.Operation;

            if (!mockMaximo) {
                //TODO: Async
                maximoResult = DataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                    .Execute(applicationMetadata, json, operationDataRequest);
            }
            if (currentschemaKey.Platform == ClientPlatform.Mobile) {
                //mobile requests doesn´t have to handle success messages or redirections
                return null;
            }



            var routerParameters = new RouterParameters(applicationMetadata, platform, operationDataRequest.RouteParametersDTOHandled, operation, mockMaximo, maximoResult, user, resolvedNextSchema);

            var response = await _nextSchemaRouter.RedirectToNextSchema(routerParameters);
            response.SuccessMessage = _successMessageHandler.FillSuccessMessage(applicationMetadata, maximoResult, operation);
            response.WarningDto = maximoResult.WarningDto;

            // TODO: Implement some sort of interception
            if (true.Equals(applicationMetadata.AuditEnabled)) {
                _auditManager.CreateAuditEntry(
                    operationDataRequest.Operation,
                    applicationMetadata.Name,
                    maximoResult.Id,
                    maximoResult.UserId,
                    json.ToString(),
                    DateTime.Now.FromServerToRightKind());
            }

            return response;

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
        public async Task<IApplicationResponse> Search(string application, string searchFields, string searchText, string schema = "list") {
            var app = MetadataProvider.Application(application);
            var schemas = app.Schemas();
            var key = new ApplicationMetadataSchemaKey(schema, SchemaMode.input, ClientPlatform.Web);
            ApplicationSchemaDefinition appSchema;

            if (!schemas.TryGetValue(key, out appSchema)) {
                throw new InvalidOperationException("schema not found");
            }

            var searchRequestDto = PaginatedSearchRequestDto.DefaultInstance(appSchema);
            searchRequestDto.SetFromSearchString(appSchema, searchFields.Split(','), searchText);

            var dataResponse = await Get(application, new DataRequestAdapter() { Key = key, SearchDTO = searchRequestDto });
            //fixing the filter parameters used so that it is applied on next queries
            ((ApplicationListResult)dataResponse).PageResultDto.BuildFixedWhereClause(app.Entity);
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
                if (matchesMode && matchesStereotype && matchesName && !schema.Value.Abstract) {
                    resultingSchemas.Add(schema.Value);
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