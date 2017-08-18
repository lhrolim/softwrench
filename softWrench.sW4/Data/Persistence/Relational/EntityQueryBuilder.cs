using System;
using System.Collections.Generic;
using System.Text;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Data.Persistence.Relational {

    internal class EntityQueryBuilder : BaseQueryBuilder {


        //TODO: cache queries
        public BindedEntityQuery ById(EntityMetadata entityMetadata, string id) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { Id = id }, QueryCacheKey.QueryMode.Detail);
        }

        public BindedEntityQuery ByUserIdSite(EntityMetadata entityMetadata, Tuple<string, string> userIdSiteTuple) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { UserIdSiteTuple = userIdSiteTuple }, QueryCacheKey.QueryMode.Detail);
        }

        public BindedEntityQuery AllRows(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { SearchDTO = searchDto }, QueryCacheKey.QueryMode.List);
        }

        public BindedEntityQuery AllRowsForSync(EntityMetadata entityMetadata, Rowstamps rowstamp, SearchRequestDto searchDto) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { Rowstamps = rowstamp, SearchDTO = searchDto }, QueryCacheKey.QueryMode.Sync);
        }

        public BindedEntityQuery CountRows(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { SearchDTO = searchDto }, QueryCacheKey.QueryMode.Count);
        }

        public BindedEntityQuery CountRowsFromConstraint(EntityMetadata entityMetadata) {
            var buffer = new StringBuilder(InitialStringBuilderCapacity);
            buffer.Append(QuerySelectBuilder.BuildSelectAttributesClause(entityMetadata, QueryCacheKey.QueryMode.Count, null));
            buffer.Append(QueryFromBuilder.Build(entityMetadata));

            var dataConstraintsWhereBuilder = SimpleInjectorGenericFactory.Instance.GetObject<DataConstraintsWhereBuilder>(typeof(DataConstraintsWhereBuilder));
            var list = new List<IWhereBuilder>{
                new EntityWhereClauseBuilder(),
                dataConstraintsWhereBuilder,
                new MultiTenantCustomerWhereBuilder()
            };
            var whereBuilder = new CompositeWhereBuilder(list);
            var whereClauseBuilt = whereBuilder.BuildWhereClause(entityMetadata.Name);

            buffer.Append(whereClauseBuilt);
            return new BindedEntityQuery(buffer.ToString(), whereBuilder.GetParameters());
        }

        public BindedEntityQuery IdAndSiteIdByUserId(EntityMetadata entityMetadata, string userid) {
            var buffer = new StringBuilder(InitialStringBuilderCapacity);

            // select
            var attributes = new List<EntityAttribute>{
                entityMetadata.Schema.IdAttribute
            };
            if (entityMetadata.Schema.SiteIdAttribute != null) {
                attributes.Add(entityMetadata.Schema.SiteIdAttribute);
            }
            buffer.Append(QuerySelectBuilder.BuildSelectAttributesClause(entityMetadata, QueryCacheKey.QueryMode.Detail, null, attributes));

            // from
            buffer.AppendFormat(" from {0} ", BaseQueryUtil.AliasEntity(entityMetadata.Name, entityMetadata.Name));

            // where
            var list = new List<IWhereBuilder>{
                new ByUserIdWhereBuilder(entityMetadata, userid)
            };
            var whereBuilder = new CompositeWhereBuilder(list);
            buffer.Append(whereBuilder.BuildWhereClause(entityMetadata.Name));

            return new BindedEntityQuery(buffer.ToString(), whereBuilder.GetParameters());
        }
    }
}