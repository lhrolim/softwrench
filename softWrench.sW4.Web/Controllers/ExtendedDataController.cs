using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {

    public class ExtendedDataController : DataController {
        public ExtendedDataController(I18NResolver i18NResolver, IContextLookuper lookuper,CompositionExpander compositionExpander)
            : base(i18NResolver, lookuper, compositionExpander) {
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

            var baseDataSet = DataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId);


            var response = baseDataSet.UpdateAssociations(applicationMetadata, request, currentData);

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
        public IGenericResponseResult ExpandCompositions(String application, [FromUri]DetailRequest detailRequest,
            [FromUri]CompositionExpanderHelper.CompositionExpansionOptions options) {
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(detailRequest.Key, user, ClientPlatform.Web);
            //            var result = (ApplicationDetailResult)DataSetProvider.LookupAsBaseDataSet(application).Get(applicationMetadata, user, detailRequest);
            var compositionSchemas = CompositionBuilder.InitializeCompositionSchemas(applicationMetadata.Schema);
            return CompositionExpander.Expand(SecurityFacade.CurrentUser(), compositionSchemas, options);
        }

        [HttpPost]
        public IApplicationResponse OpenDetailWithInitialData(string application, [FromUri] DataRequestAdapter request, JObject initialData) {
            request.InitialData = initialData;
            var response = Get(application, request);
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
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <param name="currentData"></param>
        /// <returns></returns>
        [NotNull]
        [HttpPost]
        public IGenericResponseResult GetCompositionData(string application, [FromUri] CompositionFetchRequest request, JObject currentData) {
            var user = SecurityFacade.CurrentUser();
            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(request.Key, user, ClientPlatform.Web);
            ContextLookuper.FillContext(request.Key);
            return DataSetProvider.LookupDataSet(application,applicationMetadata.Schema.SchemaId)
                .GetCompositionData(applicationMetadata, request, currentData);
        }


    }
}