using cts.commons.persistence.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class ByIdWhereBuilder : IWhereBuilder {

        private const string ByIdQueryParameter = "id";
        private readonly EntityMetadata _entityMetadata;
        private readonly string _id;

        public ByIdWhereBuilder(EntityMetadata entityMetadata, string id) {
            _entityMetadata = entityMetadata;
            _id = id;
        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            return String.Format("{0} = {1}{2}", BaseQueryUtil.QualifyAttribute(_entityMetadata, _entityMetadata.Schema.IdAttribute),HibernateUtil.ParameterPrefix,ByIdQueryParameter);
        }

        public IDictionary<string, object> GetParameters() {
            return new Dictionary<string, object> { { ByIdQueryParameter, _id } };
        }
    }
}
