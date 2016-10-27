using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using BaseQueryBuilder = softWrench.sW4.Data.Persistence.Relational.BaseQueryBuilder;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    class BaseGlobalSearchDataSet : MaximoApplicationDataSet {
        private readonly IMaximoHibernateDAO _maximoDao;
        private readonly EntityRepository _entityRepository;
        private readonly IWhereClauseFacade _whereClauseFacade;

        private readonly IDictionary<string, string> _entities = new Dictionary<string, string>
        {
            {"sr", "Service Request"},
            {"incident", "Incident"},
            {"workorder", "Work Order"},
            {"asset", "Asset"},
            {"location", "Location"}
        };

        private readonly IDictionary<string, string> _applications = new Dictionary<string, string>
        {
            {"servicerequest", "Service Request"},
            {"incident", "Incident"},
            {"workorder", "Work Order"},
            {"asset", "Asset"},
            {"location", "Location"}
        };

        private readonly IDictionary<string, string> _baseQueries = new Dictionary<string, string>
        {
            {"servicerequest", "select ticketid as userrecordid, CAST(ticketuid AS VARCHAR(15)) as recordid, description, reportdate as createdate, changedate, 'sr' as recordtype, 'Service Request' as recordtypelabel, 'servicerequest' as appname, 'editdetail' as appschema from sr WHERE {0}"},
            {"incident", "select ticketid as userrecordid, CAST(ticketuid AS VARCHAR(15)) as recordid, description, reportdate as createdate, changedate, 'incident' as recordtype, 'Incident' as recordtypelabel, 'incident' as appname, 'editdetail' as appschema from ticket WHERE class = 'INCIDENT' AND {0}"},
            {"workorder", "select wonum as userrecordid, CAST(workorderid AS VARCHAR(15)) as recordid, description, reportdate as createdate, changedate, 'workorder' as recordtype ,'Work Order' as recordtypelabel, 'workorder' as appname, 'editdetail' as appschema from workorder WHERE {0}"},
            {"asset", "select assetnum as userrecordid, CAST(assetid AS VARCHAR(15)) as recordid, description, '' as createdate, changedate, 'asset' as recordtype, 'Asset' as recordtypelabel, 'asset' as appname, 'detail' as appschema from asset WHERE {0}"},
            {"location", "select location as userrecordid, CAST(locationsid AS VARCHAR(15)) as recordid, description, '' as createdate, changedate, 'location' as recordtype,'Location' as recordtypelabel, 'location' as appname, 'locationdetail' as appschema from locations WHERE {0}"}
        };

        public BaseGlobalSearchDataSet(IMaximoHibernateDAO maximoDao, IWhereClauseFacade whereClauseFacade, EntityRepository entityRepository) {
            _maximoDao = maximoDao;
            _whereClauseFacade = whereClauseFacade;
            _entityRepository = entityRepository;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application,
         PaginatedSearchRequestDto searchDto) {
            var totalCount = searchDto.TotalCount;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var schema = application.Schema;
            searchDto.QueryAlias = application.Name + "." + schema.SchemaId;
            var propertyValue = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListSchemaOrderBy);
            if (searchDto.SearchSort == null && propertyValue != null) {
                //if the schema has a default sort defined, and we didn´t especifally asked for any sort column, apply the default schema
                searchDto.SearchSort = propertyValue;
            }

            FilterWhereClauseHandler.HandleDTO(application.Schema, searchDto);
            QuickSearchWhereClauseHandler.HandleDTO(application.Schema, searchDto);

            var paginationData = PaginationData.GetInstance(searchDto, entityMetadata);
            // Build the applicable where clause
            var queryParameter = new InternalQueryRequest { SearchDTO = searchDto };
            var compositeWhereBuilder = BaseQueryBuilder.GetCompositeBuilder(entityMetadata, queryParameter);
            var whereClause = compositeWhereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO);
            var query = await GetWrappedUnionQuery(application, searchDto);
            var ctx = ContextLookuper.LookupContext();
            // Count query

            if (searchDto.NeedsCountUpdate) {
                var boundEntityCountQuery = new BindedEntityQuery(string.Format(query, "count(*) as recordCount", whereClause, ""), compositeWhereBuilder.GetParameters());
                var totalCountResult = await _maximoDao.FindByNativeQueryAsync(boundEntityCountQuery.Sql, boundEntityCountQuery.Parameters);
                var recordCountString = ((ExpandoObject)totalCountResult[0]).FirstOrDefault(v => v.Key == "recordCount").Value.ToString();
                int.TryParse(recordCountString, out totalCount);
            }


            // Record query
            // Append sort
            if (queryParameter.SearchDTO.SearchSort == null) {
                //default sorting
                queryParameter.SearchDTO.SearchSort = "changedate desc";
            }

            var sort = QuerySearchSortBuilder.BuildSearchSort(entityMetadata, queryParameter.SearchDTO);
            var boundEntityQuery = new BindedEntityQuery(string.Format(query, "*", whereClause, sort), compositeWhereBuilder.GetParameters());
            var recordQueryResult = _maximoDao.FindByNativeQuery(boundEntityQuery.Sql, boundEntityQuery.Parameters, paginationData, searchDto.QueryAlias);
            var records = recordQueryResult.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => _entityRepository.BuildDataMap(entityMetadata, r))
               .ToList();

            return new ApplicationListResult(totalCount, searchDto, records, application.Schema, null);
        }

        /// <summary>
        /// Provider for filter on screen
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<IAssociationOption> RecordTypes(FilterProviderParameters parameters) {
            var result = new List<IAssociationOption>();
            var user = SecurityFacade.CurrentUser();
            var customerApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, user);
            foreach (var key in _entities.Keys.Where(k => customerApps.Any(a => a.Entity.EqualsIc(k)))) {
                if (MetadataProvider.Entity(key) != null) {
                    result.Add(new AssociationOption(_entities[key], _entities[key]));
                }
            }

            return result;
        }

        public async Task<string> BuildUnionQuery(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var user = SecurityFacade.CurrentUser();
            var customerApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, user);
            var sb = new StringBuilder();
            foreach (var key in _applications.Keys.Where(k => customerApps.Any(a => a.ApplicationName.EqualsIc(k)))) {
                var baseQuery = _baseQueries[key];
                var whereClause = await _whereClauseFacade.LookupAsync(key);
                if (whereClause != null) {
                    sb.Append(baseQuery.Fmt(whereClause.Query)).Append(" UNION ");
                }
            }
            var queryString = sb.ToString();
            if (queryString.EndsWith(" UNION ")) {
                queryString = queryString.Substring(0, queryString.Length - " UNION ".Length);
            }

            return queryString;
        }

        public async Task<string> GetWrappedUnionQuery(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var sb = new StringBuilder();
            // Begin wrapper
            sb.Append("select {0} from (");
            // Append union query
            sb.Append(await BuildUnionQuery(application, searchDto));
            // Close wrapper
            sb.Append(") as globalsearch {1} {2}");
            return sb.ToString();
        }

        public override string ApplicationName() {
            return "globalsearch";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
