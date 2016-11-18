using System.Collections.Generic;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational {

    public class WhereBuilderManager : ISingletonComponent {


        public string BuildWhereClause(EntityMetadata entityMetadata, InternalQueryRequest queryParameter, bool disregardWhere=false) {
            var compositeWhereBuilder = GetCompositeBuilder(entityMetadata, queryParameter, disregardWhere);
            return compositeWhereBuilder.BuildWhereClause(entityMetadata.Name, queryParameter.SearchDTO);
        }


        public IWhereBuilder GetCompositeBuilder(EntityMetadata entityMetadata, InternalQueryRequest queryParameter, bool disregardWhere = false) {
            var whereBuilders = new List<IWhereBuilder>();
            if (queryParameter.Id != null) {
                //TODO: make some kind of hash to determine if this is needed...
                whereBuilders.Add(new ByIdWhereBuilder(entityMetadata, queryParameter.Id));
                return new CompositeWhereBuilder(whereBuilders);
            } else if (queryParameter.UserIdSiteTuple != null) {
                whereBuilders.Add(new ByUserIdSiteWhereBuilder(entityMetadata, queryParameter.UserIdSiteTuple));
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
                whereBuilders.AddRange(SimpleInjectorGenericFactory.Instance.GetObjectsOfType<IDynamicWhereBuilder>(typeof(IDynamicWhereBuilder)));

            }
            whereBuilders.Add(new MultiTenantCustomerWhereBuilder());
            return new CompositeWhereBuilder(whereBuilders, disregardWhere);
        }

        protected static List<SearchParameterUtils> GetSearchParameterUtilsList(EntityMetadata entityMetadata, SearchRequestDto searchRequestDto) {
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
    }


}
