using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using BaseQueryBuilder = softWrench.sW4.Data.Persistence.Relational.BaseQueryBuilder;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.dataset {
    class PescoGlobalSearchDataSet : MaximoApplicationDataSet {
        private readonly IMaximoHibernateDAO _maximoDao;
        private readonly EntityRepository _entityRepository;
        private readonly IWhereClauseFacade _whereClauseFacade;

        private readonly IDictionary<string, string> _entities = new Dictionary<string, string>
        {
            {"sr", "Service Request"},
            {"workorder", "Work Order"},
            {"asset", "Asset"},
            {"location", "Location"}
        };

        private readonly IDictionary<string, string> _applications = new Dictionary<string, string>
        {
            {"servicerequest", "Service Request"},
            {"workorder", "Work Order"},
            {"asset", "Asset"},
            {"location", "Location"}
        };

        private readonly IDictionary<string, string> _baseQueries = new Dictionary<string, string>
        {
            {"servicerequest", "select ticketid as userrecordid, CAST(ticketuid AS VARCHAR(15)) as recordid, description, reportdate as createdate, changedate, 'sr' as recordtype, 'Service Request' as recordtypelabel, 'servicerequest' as appname, 'editdetail' as appschema from sr {0}"},
            {"workorder", "select wonum as userrecordid, CAST(workorderid AS VARCHAR(15)) as recordid, description, reportdate as createdate, changedate, 'workorder' as recordtype ,'Work Order' as recordtypelabel, 'workorder' as appname, 'editdetail' as appschema from workorder {0}"},
            {"asset", "select assetnum as userrecordid, CAST(assetid AS VARCHAR(15)) as recordid, description, '' as createdate, changedate, 'asset' as recordtype, 'Asset' as recordtypelabel, 'asset' as appname, 'detail' as appschema from asset {0}"},
            {"location", "select location as userrecordid, CAST(locationsid AS VARCHAR(15)) as recordid, description, '' as createdate, changedate, 'location' as recordtype,'Location' as recordtypelabel, 'location' as appname, 'locationdetail' as appschema from locations {0}"}
        };

        public PescoGlobalSearchDataSet(IMaximoHibernateDAO maximoDao, IWhereClauseFacade whereClauseFacade, EntityRepository entityRepository) {
            _maximoDao = maximoDao;
            _whereClauseFacade = whereClauseFacade;
            _entityRepository = entityRepository;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application,
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
            searchDto = searchDto.QuickSearchDTO == null
                ? FilterWhereClauseHandler.HandleDTO(application.Schema, searchDto)
                : QuickSearchWhereClauseHandler.HandleDTO(application.Schema, searchDto);

            var paginationData = PaginationData.GetInstance(searchDto, entityMetadata);
            // Build the applicable where clause
            var queryParameter = new InternalQueryRequest { SearchDTO = searchDto };
            var compositeWhereBuilder = BaseQueryBuilder.GetCompositeBuilder(entityMetadata, queryParameter);
            var whereClause = compositeWhereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO);
            var query = GetWrappedUnionQuery(application, searchDto);
            var tasks = new Task[1];
            var ctx = ContextLookuper.LookupContext();
            // Count query
            tasks[0] = Task.Factory.NewThread(c => {
                Quartz.Util.LogicalThreadContext.SetData("context", c);
                if (searchDto.NeedsCountUpdate) {
                    var boundEntityCountQuery = new BindedEntityQuery(string.Format(query, "count(*) as recordCount", whereClause, ""), compositeWhereBuilder.GetParameters());
                    var totalCountResult = _maximoDao.FindByNativeQuery(boundEntityCountQuery.Sql, boundEntityCountQuery.Parameters);
                    var recordCountString = ((ExpandoObject)totalCountResult[0]).FirstOrDefault(v => v.Key == "recordCount").Value.ToString();
                    int.TryParse(recordCountString, out totalCount);
                }
            }, ctx);

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

            Task.WaitAll(tasks);
            return new ApplicationListResult(totalCount, searchDto, records, application.Schema, null);
        }

        /// <summary>
        /// Provider for filter on screen
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<IAssociationOption> RecordTypes(FilterProviderParameters parameters) {
            var result = new List<IAssociationOption>();

            foreach (var key in _entities.Keys) {
                if (MetadataProvider.Entity(key) != null) {
                    result.Add(new AssociationOption(key, _entities[key]));
                }
            }

            return result;
        }

        public string BuildUnionQuery(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var sb = new StringBuilder();
            foreach (var key in _applications.Keys) {
                var baseQuery = _baseQueries[key];
                var whereClause = _whereClauseFacade.Lookup(key);
                var whereClauseString = " WHERE " + whereClause.Query;
                sb.Append(baseQuery.Fmt(whereClauseString)).Append(" UNION ");
            }
            var queryString = sb.ToString();
            if (queryString.EndsWith(" UNION ")) {
                queryString = queryString.Substring(0, queryString.Length - " UNION ".Length);
            }

            return queryString;
        }

        public string GetWrappedUnionQuery(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var sb = new StringBuilder();
            // Begin wrapper
            sb.Append("select {0} from (");
            // Append union query
            sb.Append(BuildUnionQuery(application, searchDto));
            // Close wraper
            sb.Append(") as globalsearch {1} {2}");
            return sb.ToString();
        }




        public override string ApplicationName() {
            return "globalsearch";
        }

        public override string ClientFilter() {
            return "pesco";
        }


    }
}
