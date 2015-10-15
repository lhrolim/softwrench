using cts.commons.Util;
using log4net;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using cts.commons.simpleinjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational {

    abstract class BaseQueryBuilder {

        protected const int InitialStringBuilderCapacity = 1024;

        private static ILog _log = LogManager.GetLogger(typeof(BaseQueryBuilder));

        protected BindedEntityQuery TemplateQueryBuild(EntityMetadata entityMetadata, InternalQueryRequest queryParameter, QueryCacheKey.QueryMode queryMode) {
            string queryString;
            var before = Stopwatch.StartNew();
            var compositeWhereBuilder = GetCompositeBuilder(entityMetadata, queryParameter);
            var cacheKey = GetCacheKey(queryParameter, queryMode);
            if (!entityMetadata.QueryStringCache.TryGetValue(cacheKey, out queryString) || !queryParameter.Cacheable()) {
                queryString = DoBuildQueryString(entityMetadata, queryParameter, queryMode, compositeWhereBuilder,
                    cacheKey);
            } else {
                //where clauses should always be rebuild independent of cache, due to context variation, like modules, profiles, etc...
                var whereBuilder = SimpleInjectorGenericFactory.Instance.GetObject<DataConstraintsWhereBuilder>(typeof(DataConstraintsWhereBuilder));
                var whereConstraint = whereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO);
                queryString = String.Format(queryString, whereConstraint);
            }
            if (entityMetadata.HasUnion()) {
                return HandleUnion(entityMetadata as SlicedEntityMetadata, queryParameter, queryString, queryMode, compositeWhereBuilder.GetParameters());
            }

            _log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "query for {0}:{1} built", entityMetadata.Name, queryMode.ToString()));

            return new BindedEntityQuery(queryString, compositeWhereBuilder.GetParameters());
        }

        private BindedEntityQuery HandleUnion(SlicedEntityMetadata slicedEntityMetadata, InternalQueryRequest queryParameter, string queryString, QueryCacheKey.QueryMode queryMode,
            IEnumerable<KeyValuePair<string, object>> parameters) {

            var queryModeToPropagate = queryMode == QueryCacheKey.QueryMode.Count
                ? QueryCacheKey.QueryMode.Count
                : QueryCacheKey.QueryMode.Union;

            var unionSearchRequestDto = SearchRequestDto.GetUnionSearchRequestDto(queryParameter.SearchDTO, slicedEntityMetadata.UnionSchema);
            var unionQuery = TemplateQueryBuild(slicedEntityMetadata.UnionSchema, new InternalQueryRequest() { SearchDTO = unionSearchRequestDto }, queryModeToPropagate);

            queryString += (" union all " + unionQuery.Sql + " ");
            if (queryMode == QueryCacheKey.QueryMode.Count) {
                queryString = "select sum(cnt) from (" + queryString + ")";
                return new BindedEntityQuery(queryString, parameters.Union(unionQuery.Parameters));
            }
            //for unions, we need to do the order by in the end
            queryString += QuerySearchSortBuilder.BuildSearchSort(slicedEntityMetadata, queryParameter.SearchDTO);

            return new BindedEntityQuery(queryString, parameters.Union(unionQuery.Parameters));
        }

        private static string DoBuildQueryString(EntityMetadata entityMetadata, InternalQueryRequest queryParameter,
            QueryCacheKey.QueryMode queryMode, IWhereBuilder compositeWhereBuilder, QueryCacheKey cacheKey) {
            var buffer = new StringBuilder(InitialStringBuilderCapacity);
            var projectionBuilder = new StringBuilder(InitialStringBuilderCapacity);

            projectionBuilder.Append(QuerySelectBuilder.BuildSelectAttributesClause(entityMetadata, queryMode, queryParameter.SearchDTO));
            projectionBuilder.Append(QueryFromBuilder.Build(entityMetadata, queryParameter.SearchDTO));
            buffer.Append(projectionBuilder);
            buffer.Append(compositeWhereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO));

            var hasUnionWhereClauses = queryParameter.SearchDTO != null && queryParameter.SearchDTO.UnionWhereClauses != null;
            var isUnion = entityMetadata.HasUnion() || queryMode == QueryCacheKey.QueryMode.Union || hasUnionWhereClauses;

            if (queryMode != QueryCacheKey.QueryMode.Count && queryMode != QueryCacheKey.QueryMode.Detail && !isUnion) {
                buffer.Append(QuerySearchSortBuilder.BuildSearchSort(entityMetadata, queryParameter.SearchDTO));
            }
            if (hasUnionWhereClauses) {
                foreach (var unionWC in queryParameter.SearchDTO.UnionWhereClauses) {
                    buffer.Append(" union all ").Append(projectionBuilder).Append(" where (").Append(unionWC).Append(")");
                }
                buffer.Append(" order by 1 desc");
            }
            var queryString = buffer.ToString();
            return queryString;
        }

        protected virtual QueryCacheKey GetCacheKey(object queryParameter, QueryCacheKey.QueryMode queryMode) {
            return new QueryCacheKey(queryMode);
        }

        protected IWhereBuilder GetCompositeBuilder(EntityMetadata entityMetadata, InternalQueryRequest queryParameter) {
            IList<IWhereBuilder> whereBuilders = new List<IWhereBuilder>();
            if (queryParameter.Id != null) {
                //TODO: make some kind of hash to determine if this is needed...
                whereBuilders.Add(new ByIdWhereBuilder(entityMetadata, queryParameter.Id));
                return new CompositeWhereBuilder(whereBuilders);
            }
            whereBuilders.Add(new EntityWhereClauseBuilder());
            whereBuilders.Add(new FixedSearchWhereClauseBuilder());
            if (queryParameter.SearchDTO != null) {
                var searchParameterUtilsList = GetSearchParameterUtilsList(entityMetadata, queryParameter.SearchDTO);
                whereBuilders.Add(searchParameterUtilsList.Count == 0
                    ? new SearchUtils(queryParameter.SearchDTO, entityMetadata.Name, entityMetadata.GetTableName())
                    : new SearchUtils(queryParameter.SearchDTO, entityMetadata.Name, entityMetadata.GetTableName(), searchParameterUtilsList));
            }
            if (queryParameter.Rowstamps != null) {
                whereBuilders.Add(new RowstampQueryBuilder(queryParameter.Rowstamps));
            }
            if (!ApplicationConfiguration.IsUnitTest) {
                //TODO: better solution here, to allow to instantiate simpleinjector on tests
                whereBuilders.Add(
                    SimpleInjectorGenericFactory.Instance.GetObject<DataConstraintsWhereBuilder>(
                        typeof(DataConstraintsWhereBuilder)));
            }
            whereBuilders.Add(new MultiTenantCustomerWhereBuilder());
            return new CompositeWhereBuilder(whereBuilders);
        }

        protected List<SearchParameterUtils> GetSearchParameterUtilsList(EntityMetadata entityMetadata, SearchRequestDto searchRequestDto) {
            var searchParameterUtilsList = new List<SearchParameterUtils>();
            if (searchRequestDto.ValuesDictionary != null) {
                var entityAttributes = entityMetadata.Schema.Attributes;

                foreach (var param in searchRequestDto.ValuesDictionary) {
                    foreach (var entityAttribute in entityAttributes) {
                        if (param.Key.StartsWith(entityAttribute.Name)) {
                            searchParameterUtilsList.Add(new SearchParameterUtils(param.Key, param.Value.Value, entityAttribute.Type));
                        }
                    }
                }
            }
            return searchParameterUtilsList;
        }

        protected class InternalQueryRequest {

            public SearchRequestDto SearchDTO;

            public Rowstamps Rowstamps {
                get; set;
            }

            public string Id {
                get; set;
            }

            public bool Cacheable() {
                return SearchDTO == null;
            }

        }



    }
}
