using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API.Tab;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers {


    [Authorize]
    [SWControllerConfiguration]
    public class TabController : ApiController {

        private readonly IContextLookuper _contextLookuper;
        private readonly DataSetProvider _dataSetProvider;

        public TabController(IContextLookuper contextLookuper, DataSetProvider dataSetProvider) {
            _contextLookuper = contextLookuper;
            _dataSetProvider = dataSetProvider;
        }


        /// <summary>
        /// Loads any relevant data for a given tab according to the DataSet implementation
        /// </summary>
        /// <param name="tabRequestWrapper"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IGenericResponseResult> GetLazyTabData(TabRequestWrapperDTO tabRequestWrapper) {
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            var application = tabRequestWrapper.Application;
            var request = tabRequestWrapper.Request;
            var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(request.Key, user, ClientPlatform.Web);

            _contextLookuper.FillContext(request.Key, new Dictionary<string, object>());

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata, applicationMetadata, tabRequestWrapper.Data, request.Id);
            tabRequestWrapper.Request.CrudData = cruddata;

            var tabResult = await _dataSetProvider
                .LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                .GetTabLazyData(applicationMetadata, tabRequestWrapper.Request);

            return tabResult;
        }


    }
}