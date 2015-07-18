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
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers {

    public class ExtendedDataController : DataController {
        public ExtendedDataController(I18NResolver i18NResolver, IContextLookuper lookuper, CompositionExpander expander)
            : base(i18NResolver, lookuper, expander) {
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
            return COMPOSITIONExpander.Expand(SecurityFacade.CurrentUser(), compositionSchemas, options);
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
        /// this method is used only to verify whether the user is still authenticated on the server or not, since for ie9 attachment submission no ajax would be made and then the logout screen would never appear.
        /// </summary>
        /// <returns></returns>
        [ApplicationAuthorize]
        [HttpGet]
        public void PingServer() {
            
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

            ContextLookuper.FillContext(request.Key);

            var compositionData = DataSetProvider
                .LookupDataSet(application)
                .GetCompositionData(applicationMetadata, request, dto.Data);

            return compositionData;
        }


    }
}