using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Security;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Util.TransactionStatistics;

namespace softWrench.sW4.Web.Controllers {

    public class ExtendedDataController : DataController {
        public ExtendedDataController(I18NResolver i18NResolver, IContextLookuper lookuper,CompositionExpander compositionExpander, IAuditManager auditManager, TransactionStatisticsService txService, UserMainSecurityApplier userSecurityApplier)
            : base(i18NResolver, lookuper, compositionExpander, auditManager, txService, userSecurityApplier) {
        }

        /// <summary>
        /// API Method to provide updated Association Options, for given application, according the Association Update Request
        /// This method will provide options for depedant associations, lookup association and autocomplete association.
        /// </summary>
        ///
        [NotNull]
        [HttpPost]
        public async Task<GenericResponseResult<IDictionary<string, BaseAssociationUpdateResult>>> UpdateAssociation(string application,
            [FromUri] AssociationUpdateRequest request, JObject currentData) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            ContextLookuper.FillContext(request.Key);
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web);

            var baseDataSet = DataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId);


            var response = await baseDataSet.UpdateAssociations(applicationMetadata, request, currentData);

            return response;
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
        public async Task<IGenericResponseResult> ExpandCompositions(String application, [FromUri]DetailRequest detailRequest,
            [FromUri]CompositionExpanderHelper.CompositionExpansionOptions options) {
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(detailRequest.Key, user, ClientPlatform.Web);
            //            var result = (ApplicationDetailResult)DataSetProvider.LookupAsBaseDataSet(application).Get(applicationMetadata, user, detailRequest);
            var compositionSchemas = CompositionBuilder.InitializeCompositionSchemas(applicationMetadata.Schema);
            return await CompositionExpander.Expand(SecurityFacade.CurrentUser(), compositionSchemas, options);
        }

        [HttpPost]
        public async Task<IApplicationResponse> OpenDetailWithInitialData(string application, [FromUri] DataRequestAdapter request, JObject initialData) {
            request.InitialData = initialData;
            var response = await Get(application, request);
            if (!string.IsNullOrEmpty(request.Title)) {
                var newtitle = request.Title.Fmt(response.Title);
                response.Title = newtitle;
                response.Schema.Title = newtitle;
            }
            return response;
        }

        /// <summary>
        ///  Returns the datamap populated with composition data
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>datamap populated with composition data</returns>
        [NotNull]
        [HttpPost]
        public async Task<CompositionFetchResult> GetCompositionData(CompositionRequestWrapperDTO dto) {
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            var application = dto.Application;
            var request = dto.Request;
            var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(request.Key, user, ClientPlatform.Web);
            
            ContextLookuper.FillContext(request.Key);
            
            var compositionData = await DataSetProvider
                .LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                .GetCompositionData(applicationMetadata, request, dto.Data);
            
            return compositionData;
        }


    }
}