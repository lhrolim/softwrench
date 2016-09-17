using cts.commons.persistence.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using System.Collections.Generic;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    /// <summary>
    /// This one is only used to find wntris with the same user id and different siteids
    /// </summary>
    public class ByUserIdWhereBuilder : IWhereBuilder {

        private const string ByUserIdQueryParameter = "userid";


        private readonly EntityMetadata _entityMetadata;
        private readonly string _userIdValue;

        public ByUserIdWhereBuilder(EntityMetadata entityMetadata, [NotNull]string userIdValue) {
            _entityMetadata = entityMetadata;
            _userIdValue = userIdValue;
        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var idQualified = BaseQueryUtil.QualifyAttribute(_entityMetadata, _entityMetadata.Schema.UserIdAttribute);
            return "{0} = {1}{2}".Fmt(idQualified, HibernateUtil.ParameterPrefix, ByUserIdQueryParameter);
        }

        public IDictionary<string, object> GetParameters() {
            return new Dictionary<string, object> { { ByUserIdQueryParameter, _userIdValue } };
        }
    }
}
