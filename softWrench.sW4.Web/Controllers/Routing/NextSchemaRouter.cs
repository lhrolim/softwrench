using System;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Web.Common;

namespace softWrench.sW4.Web.Controllers.Routing {
    public class NextSchemaRouter {

        private readonly BaseApplicationDataSet _dataObjectSet = new BaseApplicationDataSet();
        private readonly DataSetProvider _dataSetProvider = DataSetProvider.GetInstance();

        private MockingUtils mockUtils =
            SimpleInjectorGenericFactory.Instance.GetObject<MockingUtils>(typeof(MockingUtils));

        /// <summary>
        /// this data is serialized into a '.' separated string, in the format, schemaId.mode.platform. 
        /// see aa_utils.js on the web client
        /// </summary>
        public ApplicationMetadataSchemaKey GetSchemaKeyFromString(string application, string schemaKey, ClientPlatform clientPlatform) {
            if (ClientPlatform.Mobile == clientPlatform) {
                //for now, the mobile schema can only be detail.input
                //TODO: fix it on mobile side
                return new ApplicationMetadataSchemaKey(SchemaStereotype.Detail.ToString().ToLower(), SchemaMode.input, ClientPlatform.Mobile);
            }
            if (schemaKey == null || String.IsNullOrWhiteSpace(schemaKey)) {
                return null;
            }
            return SchemaUtil.ParseKey(schemaKey, false);

            //            //sometimes the key may come incomplete, like without the mode, but if there´s a valid schema, it should be returned.
            //            // then, on the schema declaration, all the data will be present.
            //            key = MetadataProvider.Application(application).Schema(key).GetSchemaKey();
            //            return null;
        }

        /// <summary>
        /// this data is populated inside json. see aa_utils.js
        /// </summary>
        /// <param name="application"></param>
        /// <param name="json"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public ApplicationMetadataSchemaKey GetSchemaKeyFromJson(string application, JObject json, bool current) {
            var keytouse = current ? "%%currentschema" : "%%nextschema";
            var schema = json.Property(keytouse);
            if (current && schema == null) {
                //for compatibility issues with ios client
                return new ApplicationMetadataSchemaKey(SchemaStereotype.Detail.ToString().ToLower(), SchemaMode.input, ClientPlatform.Mobile);
            }
            if (schema == null) {
                return null;
            }
            var key = schema.Value.ToObject(typeof(ApplicationMetadataSchemaKey));
            json.Remove(keytouse);
            //sometimes the key may come incomplete, like without the mode, but if there´s a valid schema, it should be returned.
            // then, on the schema declaration, all the data will be present.
            return MetadataProvider.Application(application).Schema((ApplicationMetadataSchemaKey)key).GetSchemaKey();
        }


        public IApplicationResponse RedirectToNextSchema(ApplicationMetadata applicationMetadata, string operation, string id, ClientPlatform platform,
            ApplicationMetadataSchemaKey currentschemaKey, ApplicationMetadataSchemaKey nextSchemaKey, bool maximoMocked = false) {
            var applicationName = applicationMetadata.Name;
            if (nextSchemaKey == null) {
                if (HasActionRedirectionDefinedByProperties(applicationMetadata.Schema, operation)) {
                    return ActionRedirection(applicationMetadata.Schema, operation);
                }
                nextSchemaKey = ResolveNextSchemaKey(operation, currentschemaKey, platform, applicationMetadata);
            }
            //            var applicationName = currentMetadata.Name;
            var metadata = MetadataProvider.Application(applicationName);
            var resultSchema = metadata.Schema(nextSchemaKey, true);
            var user = SecurityFacade.CurrentUser();
            var nextMetadata = metadata.ApplyPolicies(nextSchemaKey, user, ClientPlatform.Web);
            var dataSet = _dataSetProvider.LookupAsBaseDataSet(applicationName);
            if (resultSchema.Stereotype == SchemaStereotype.Detail) {
                if (maximoMocked && id==null) {
                    return mockUtils.GetMockedDataMap(applicationName, resultSchema, nextMetadata);
                }
                var detailRequest = new DetailRequest(nextSchemaKey, null) { Id = id };
                detailRequest.CompositionsToFetch = operation != OperationConstants.CRUD_CREATE ? "#all" : null;
                var response = dataSet.Get(nextMetadata, SecurityFacade.CurrentUser(), detailRequest);
                return response;
            }
            if (resultSchema.Stereotype == SchemaStereotype.List) {
                var paginatedSearchRequestDto = PaginatedSearchRequestDto.DefaultInstance(resultSchema);
                return dataSet.Get(nextMetadata, user, new DataRequestAdapter(paginatedSearchRequestDto));
            }
            throw new NotImplementedException("missing implementation for this kind of schema redirection");
        }

        private IApplicationResponse ActionRedirection(ApplicationSchemaDefinition schema, string operation) {
            var properties = schema.Properties;
            var controllerAndAction = properties[(ApplicationSchemaPropertiesCatalog.OnCrudSaveEventAction)].Split('.');
            var controller = controllerAndAction[0];
            var action = controllerAndAction[1];
            //TODO: parameters
            return new ActionRedirectResponse {
                Controller = controller,
                Action = action
            };
        }

        private bool HasActionRedirectionDefinedByProperties(ApplicationSchemaDefinition schema, string operation) {
            if (operation == OperationConstants.CRUD_CREATE || operation == OperationConstants.CRUD_UPDATE) {
                //TODO: distinguish all kind of operators --> oncreateevent.controller
                return schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.OnCrudSaveEventAction);
            }
            return false;

        }

        /// <summary>
        /// First tries to locate the next schema from the command. If its not specified, then fallbacks for locating it from a schema property.
        ///  if not specified, continues on the current schema
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="currentschemaKey"></param>
        /// <param name="platform"></param>
        /// <param name="applicationMetadata"></param>
        /// <returns></returns>
        public ApplicationMetadataSchemaKey ResolveNextSchemaKey(string operation,
           ApplicationMetadataSchemaKey currentschemaKey, ClientPlatform platform,
           ApplicationMetadata applicationMetadata) {

            string nextSchemaId;
            var applicationCommand = ApplicationCommandUtils.GetApplicationCommand(applicationMetadata, operation);
            if (applicationCommand != null && !String.IsNullOrWhiteSpace(applicationCommand.NextSchemaId)) {
                nextSchemaId = applicationCommand.NextSchemaId;
            } else {
                //TODO: adjust metadatas to allow the entire schema string representation as a property ==> "nextschema.schema"
                nextSchemaId = applicationMetadata.Schema.Properties["nextschema.schemaid"];
            }

            if (String.IsNullOrWhiteSpace(nextSchemaId)) {
                //if none set, consider that the application should stay on the same schema
                return currentschemaKey;
            }
            //TODO: what about the mode?suppose the scenario, where we have to schemas named detail, one for input other for output...
            return GetSchemaKeyFromString(applicationMetadata.Name, nextSchemaId + ".." + platform, platform);

        }


    }
}
