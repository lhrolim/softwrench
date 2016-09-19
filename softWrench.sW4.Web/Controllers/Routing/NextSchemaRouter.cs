using System;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;

namespace softWrench.sW4.Web.Controllers.Routing {
    public class NextSchemaRouter {

        private static ILog Log = LogManager.GetLogger(typeof(NextSchemaRouter));

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


        public async Task<IApplicationResponse> RedirectToNextSchema(RouterParameters routerParameter) {
            var nextMetadata = routerParameter.NextApplication;
            var targetMocked = routerParameter.TargetMocked;
            var targetResult = routerParameter.TargetResult;
            string id = null;
            Tuple<string, string> userIdSiteTuple = null;
            if (targetResult != null) {
                id = targetResult.Id;
                if (id == null) {
                    userIdSiteTuple = new Tuple<string, string>(targetResult.UserId, targetResult.SiteId);
                }
            }


            var applicationName = nextMetadata.Name;

            var dataSet = DataSetProvider.GetInstance().LookupDataSet(applicationName, nextMetadata.Schema.SchemaId);

            if (routerParameter.NoApplicationRedirectDetected && targetResult != null && targetResult.ResultObject != null) {
                return new GenericApplicationResponse {
                    Id = id,
                    TimeStamp = DateTime.Now.FromServerToRightKind(),
                    ResultObject = targetResult.ResultObject
                };
            }

            if (routerParameter.NoApplicationRedirectDetected) {

                if (routerParameter.NextAction == null) {
                    Log.DebugFormat("No redirect needed");
                    return new BlankApplicationResponse {
                        Id = id,
                        TimeStamp = DateTime.Now.FromServerToRightKind()
                    };
                }

                Log.DebugFormat("redirecting to custom controller/action {0}/{1} ", routerParameter.NextController, routerParameter.NextAction);
                return new ActionRedirectResponse {
                    Controller = routerParameter.NextController,
                    Action = routerParameter.NextAction
                };
            }

            var nextSchema = nextMetadata.Schema;


            if (nextSchema.Stereotype == SchemaStereotype.Detail || nextSchema.Stereotype == SchemaStereotype.DetailNew) {
                if (targetMocked) {
                    Log.DebugFormat("retrieving mocked detail results");
                    return MockingUtils.GetMockedDataMap(applicationName, nextSchema, nextMetadata);
                }
                var detailRequest = new DetailRequest(nextSchema.GetSchemaKey(), null) { Id = id, UserIdSitetuple = userIdSiteTuple };
                var response = await dataSet.Get(nextMetadata, SecurityFacade.CurrentUser(), detailRequest);
                return response;
            }
            if (nextSchema.Stereotype == SchemaStereotype.List) {
                var paginatedSearchRequestDto = PaginatedSearchRequestDto.DefaultInstance(nextSchema);
                var applicationKey = new ApplicationKey(nextSchema);
                if (routerParameter.CheckPointContext.ContainsKey(applicationKey)) {
                    Log.DebugFormat("applying checkpoint search");
                    paginatedSearchRequestDto = routerParameter.CheckPointContext[applicationKey].ListContext;
                }
                return await dataSet.Get(nextMetadata, routerParameter.User, new DataRequestAdapter(paginatedSearchRequestDto));
            }
            throw new NotImplementedException("missing implementation for this kind of schema redirection");
        }
    }
}
