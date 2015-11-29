using System;
using System.Net;
using System.Web.Http;
using cts.commons.web.Attributes;
using JetBrains.Annotations;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers {


    [Authorize]
    [SWControllerConfiguration]
    public class CompositionController : ApiController {

        private readonly IContextLookuper _contextLookuper;
        private readonly DataSetProvider _dataSetProvider;
        private readonly CompositionExpander _compositionExpander;

        public CompositionController(IContextLookuper contextLookuper, DataSetProvider dataSetProvider, CompositionExpander compositionExpander) {
            _contextLookuper = contextLookuper;
            _dataSetProvider = dataSetProvider;
            _compositionExpander = compositionExpander;
        }


        /// <summary>
        /// For a given entity, represented by the tuple application+id, retrieves all the composition´s data
        /// that were requested in compositions parameter.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="detailRequest"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpGet]
        public IGenericResponseResult ExpandCompositions(String application, [FromUri]DetailRequest detailRequest,
            [FromUri]CompositionExpanderHelper.CompositionExpansionOptions options) {
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(detailRequest.Key, user, ClientPlatform.Web);
            //            var result = (ApplicationDetailResult)DataSetProvider.LookupAsBaseDataSet(application).Get(applicationMetadata, user, detailRequest);
            var compositionSchemas = CompositionBuilder.InitializeCompositionSchemas(applicationMetadata.Schema);
            return _compositionExpander.Expand(SecurityFacade.CurrentUser(), compositionSchemas, options);
        }



        /// <summary>
        ///  Returns the datamap populated with composition data
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>datamap populated with composition data</returns>
        [NotNull]
        [HttpPost]
        public IGenericResponseResult GetCompositionData(CompositionRequestWrapperDTO dto) {
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            var application = dto.Application;
            var request = dto.Request;
            var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(request.Key, user, ClientPlatform.Web);

            _contextLookuper.FillContext(request.Key);

            var compositionData = _dataSetProvider
                .LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                .GetCompositionData(applicationMetadata, request, dto.Data);

            return compositionData;
        }


    }
}