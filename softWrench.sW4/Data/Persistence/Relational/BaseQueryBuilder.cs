﻿using log4net;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

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


            var unionQuery = TemplateQueryBuild(slicedEntityMetadata.UnionSchema, new InternalQueryRequest() { SearchDTO = queryParameter.SearchDTO.unionDTO }, queryModeToPropagate);

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
            buffer.Append(QuerySelectBuilder.BuildSelectAttributesClause(entityMetadata, queryMode, queryParameter.SearchDTO));
            buffer.Append(QueryFromBuilder.Build(entityMetadata, queryParameter.SearchDTO));
            buffer.Append(compositeWhereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO));
            if (queryMode != QueryCacheKey.QueryMode.Count && queryMode != QueryCacheKey.QueryMode.Detail && !entityMetadata.HasUnion() && queryMode != QueryCacheKey.QueryMode.Union) {
                buffer.Append(QuerySearchSortBuilder.BuildSearchSort(entityMetadata, queryParameter.SearchDTO));
            }
            var queryString = buffer.ToString();
            if (queryParameter.Cacheable()) {
                //                entityMetadata.QueryStringCache.Add(cacheKey, queryString);
            }
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

            whereBuilders.Add(SimpleInjectorGenericFactory.Instance.GetObject<DataConstraintsWhereBuilder>(typeof(DataConstraintsWhereBuilder)));
            whereBuilders.Add(new MultiTenantCustomerWhereBuilder());
            return new CompositeWhereBuilder(whereBuilders);
        }

        protected List<SearchParameterUtils> GetSearchParameterUtilsList(EntityMetadata entityMetadata, SearchRequestDto searchRequestDto) {
            var searchParameterUtilsList = new List<SearchParameterUtils>();
            if (searchRequestDto.ValuesDictionary != null) {
                var entityAttributes = entityMetadata.Schema.Attributes;

                foreach (var param in searchRequestDto.ValuesDictionary) {
                    foreach (var entityAttribute in entityAttributes) {
                        if (param.Key == entityAttribute.Name) {
                            searchParameterUtilsList.Add(new SearchParameterUtils(param.Key, param.Value.Value, entityAttribute.Type));
                        }
                    }
                }
            }
            return searchParameterUtilsList;
        }

        protected class InternalQueryRequest {

            public SearchRequestDto SearchDTO;

            public Rowstamps Rowstamps { get; set; }

            public string Id { get; set; }

            public bool Cacheable() {
                return SearchDTO == null;
            }

        }



    }
}
