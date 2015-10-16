using System;
using System.Collections.Generic;
using System.Text;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Relational {

    internal class EntityQueryBuilder : BaseQueryBuilder {


        //TODO: cache queries
        public BindedEntityQuery ById(EntityMetadata entityMetadata, string id) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { Id = id }, QueryCacheKey.QueryMode.Detail);
        }

        public BindedEntityQuery ByUserIdSite(EntityMetadata entityMetadata, Tuple<string,string> userIdSiteTuple) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { UserIdSiteTuple = userIdSiteTuple }, QueryCacheKey.QueryMode.Detail);
        }

        public BindedEntityQuery AllRows(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { SearchDTO = searchDto }, QueryCacheKey.QueryMode.List);
        }

        public BindedEntityQuery AllRowsForSync(EntityMetadata entityMetadata, Rowstamps rowstamp) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { Rowstamps = rowstamp, SearchDTO = new SearchRequestDto() }, QueryCacheKey.QueryMode.Sync);
        }

        public BindedEntityQuery CountRows(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return TemplateQueryBuild(entityMetadata, new InternalQueryRequest { SearchDTO = searchDto }, QueryCacheKey.QueryMode.Count);
        }

        public BindedEntityQuery CountRowsFromConstraint(EntityMetadata entityMetadata, softwrench.sw4.user.classes.entities.DataConstraint constraint) {
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
            buffer.Append(whereBuilder.BuildWhereClause(entityMetadata.Name));
            return new BindedEntityQuery(buffer.ToString(), whereBuilder.GetParameters());
        }


    }
}