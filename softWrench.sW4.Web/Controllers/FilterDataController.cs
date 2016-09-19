using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class FilterDataController : ApiController {


        private readonly DataSetProvider _dataSetProvider;
        private readonly ApplicationAssociationResolver _associationResolver;
        private readonly FilterWhereClauseHandler _filterWhereClauseHandler;
        private readonly IContextLookuper _contextLookuper;


        public FilterDataController(DataSetProvider dataSetProvider, ApplicationAssociationResolver associationResolver, FilterWhereClauseHandler filterWhereClauseHandler, IContextLookuper contextLookuper) {
            _dataSetProvider = dataSetProvider;
            _associationResolver = associationResolver;
            _filterWhereClauseHandler = filterWhereClauseHandler;
            _contextLookuper = contextLookuper;
        }


        /// <summary>
        /// Returns a list of options to be displayed on the screen for a filter, either eager or lazy loaded
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filterProvider">as described on MetadataOptionFilter</param>
        /// <param name="filterAttribute"></param>
        /// <param name="labelSearchString"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<IAssociationOption>> GetFilterOptions([FromUri]ApplicationMetadataSchemaKey key,
            string filterProvider, string filterAttribute, string labelSearchString) {

            var application = key.ApplicationName;

            //this is the main application, such as sr
            var app = MetadataProvider.Application(application).ApplyPoliciesWeb(key);

            

            if (filterProvider.StartsWith("@")) {
                var schema = app.Schema;
                var filterParam = new FilterProviderParameters(labelSearchString, filterAttribute, schema);
                return GenericSwMethodInvoker.Invoke<IEnumerable<IAssociationOption>>(schema, filterProvider, filterParam);
            }


            var association = BuildAssociation(app, filterProvider, filterAttribute);

            var filter = new PaginatedSearchRequestDto();

            filter.AppendWhereClause(_filterWhereClauseHandler.GenerateFilterLookupWhereClause(association.OriginalLabelField, labelSearchString, app.Schema));
            filter.QuickSearchDTO = QuickSearchDTO.Basic(labelSearchString);
            //let´s limit the filter adding an extra value so that we know there´re more to be brought
            //TODO: add a count call
            if (!association.EntityAssociation.Cacheable) {
                filter.PageSize = 21;
            }
            //adopting to use an association to keep same existing service
            var result = await _associationResolver.ResolveOptions(app.Schema, Entity.GetInstance(MetadataProvider.EntityByApplication(application)), association, filter);
            return result;

        }




        private static ApplicationAssociationDefinition BuildAssociation(ApplicationMetadata application, string filterProvider,
            string filterAttribute) {
            var registeredAssociation =
                application.Schema.Associations().FirstOrDefault(a => a.Target.Equals(filterAttribute));
            if (registeredAssociation != null) {
                return registeredAssociation;
            }
            return ApplicationAssociationFactory.GetFilterInstance(application.Name, filterProvider, filterAttribute);
        }
    }
}