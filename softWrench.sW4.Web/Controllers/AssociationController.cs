using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using cts.commons.web.Attributes;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Association.Lookup;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class AssociationController : ApiController {

        private readonly DataSetProvider _dataSetProvider;
        private readonly ApplicationAssociationResolver _associationResolver;
        private readonly FilterWhereClauseHandler _filterWhereClauseHandler;
        private readonly IContextLookuper _contextLookuper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(AssociationController));


        public AssociationController(DataSetProvider dataSetProvider, ApplicationAssociationResolver associationResolver,
            FilterWhereClauseHandler filterWhereClauseHandler, IContextLookuper contextLookuper) {
            _dataSetProvider = dataSetProvider;
            _associationResolver = associationResolver;
            _filterWhereClauseHandler = filterWhereClauseHandler;
            _contextLookuper = contextLookuper;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="associationKey"></param>
        /// <param name="labelSearchString"></param>
        /// <param name="currentData">The current state of the detail object on the screen. Needed cause there can be whereclauses 
        /// relying on external attributes</param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<IAssociationOption> GetFilteredOptions([FromUri]ApplicationMetadataSchemaKey key, [FromUri]string associationKey, [FromUri]string labelSearchString,
            JObject currentData) {

            //this is the main application, such as sr
            var application = key.ApplicationName;

            var app = MetadataProvider.Application(application).ApplyPoliciesWeb(key);
            var association = BuildAssociation(app, associationKey);

            var filter = new PaginatedSearchRequestDto { QuickSearchData = labelSearchString };

            // filter.AppendWhereClause(_filterWhereClauseHandler.GenerateFilterLookupWhereClause(association.OriginalLabelField, labelSearchString, app.Schema));
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), MetadataProvider.EntityByApplication(application), app, currentData);

            //adopting to use an association to keep same existing service
            var result = _associationResolver.ResolveOptions(app.Schema, cruddata, association, filter);
            return result;
        }



        /// <summary>
        /// Returns a list of lookup options based upon the criteria passed by the client side
        /// </summary>
        ///
        [NotNull]
        [HttpPost]
        public GenericResponseResult<LookupOptionsFetchResultDTO> GetLookupOptions([FromUri] LookupOptionsFetchRequestDTO request, JObject currentData) {

            var application = request.ParentKey.ApplicationName;

            _contextLookuper.FillContext(request.ParentKey);
            var applicationMetadata = MetadataProvider.Application(application).ApplyPoliciesWeb(request.ParentKey);


            var baseDataSet = _dataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId);

            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), MetadataProvider.EntityByApplication(application),
                applicationMetadata, currentData);


            var response = baseDataSet.GetLookupOptions(applicationMetadata, request, cruddata);

            return new GenericResponseResult<LookupOptionsFetchResultDTO>(response);
        }

        [HttpPost]
        public IAssociationOption LookupSingleAssociation([FromUri] ApplicationMetadataSchemaKey key, string associationKey, string associationValue, JObject currentData) {
            if (associationValue == null) {
                return null;
            }

            Log.DebugFormat("retrieving single association value for {0}:{1} app {0} ", associationKey, associationValue, key.ApplicationName);
            //TODO: make specific method for single association to increase performance/encapsulation
            var result = DoGetAssociations(key, new SingleAssociationPrefetcherRequest() { AssociationsToFetch = associationKey }, currentData);
            if (result.PreFetchLazyOptions.ContainsKey(associationKey)) {
                return result.PreFetchLazyOptions[associationKey][associationValue.ToLower()];
            }
            return null;
        }

        /// <summary>
        /// 
        /// Brings all the association data of a given schema, including any eager list of options and all the prefetchedlazy options 
        /// (i.e the list of options required to match the already "setup" associations of the main entity).
        /// 
        ///  It also brings up any eager composition prefetched associations
        /// 
        /// <see cref="AssociationMainSchemaLoadResult"/>
        /// 
        /// </summary>
        ///
        [NotNull]
        [HttpPost]
        public GenericResponseResult<AssociationMainSchemaLoadResult> GetSchemaOptions([FromUri] ApplicationMetadataSchemaKey key, [FromUri] bool showmore, JObject currentData) {
            var result = DoGetAssociations(key, new SchemaAssociationPrefetcherRequest() { IsShowMoreMode = showmore }, currentData);

            return new GenericResponseResult<AssociationMainSchemaLoadResult>(result);
        }

        private AssociationMainSchemaLoadResult DoGetAssociations(ApplicationMetadataSchemaKey key, IAssociationPrefetcherRequest associationPrefetcherRequest,
            JObject currentData) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
            _contextLookuper.FillContext(key);
            var applicationMetadata = MetadataProvider
                .Application(key.ApplicationName)
                .ApplyPolicies(key, user, ClientPlatform.Web);

            var baseDataSet = _dataSetProvider.LookupDataSet(key.ApplicationName, applicationMetadata.Schema.SchemaId);


            var entityMetadata = MetadataProvider.Entity(applicationMetadata.Entity);
            var cruddata = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata,
                applicationMetadata, currentData);

            var result = baseDataSet.BuildAssociationOptions(cruddata, applicationMetadata.Schema,
                associationPrefetcherRequest);
            return result;
        }

        private static ApplicationAssociationDefinition BuildAssociation(ApplicationMetadata application, string associationKey) {
            var registeredAssociation =
                application.Schema.Associations().FirstOrDefault(a => a.AssociationKey.Equals(associationKey));
            return registeredAssociation;

        }

    }
}