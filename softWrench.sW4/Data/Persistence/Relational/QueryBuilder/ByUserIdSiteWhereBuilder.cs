using cts.commons.persistence.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using System;
using System.Collections.Generic;
using cts.commons.portable.Util;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    /// <summary>
    /// When creating new items on maximo sometimes it returns just the userId + the site. Therefore this has to be used instead of the ByIdSiteWhereBuilder
    /// </summary>
    public class ByUserIdSiteWhereBuilder : IWhereBuilder {

        private const string ByUserIdQueryParameter = "userid";
        private const string BySiteIdQueryParameter = "siteid";


        private readonly EntityMetadata _entityMetadata;
        private readonly Tuple<string, string> _userIdSiteTuple;

        public ByUserIdSiteWhereBuilder(EntityMetadata entityMetadata, [NotNull]Tuple<string, string> userIdSiteTuple) {
            _entityMetadata = entityMetadata;
            _userIdSiteTuple = userIdSiteTuple;
        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var idQualified = BaseQueryUtil.QualifyAttribute(_entityMetadata, _entityMetadata.Schema.UserIdAttribute);
            var siteIdAttribute = _entityMetadata.Schema.SiteIdAttribute;
            if (siteIdAttribute == null || _userIdSiteTuple.Item2 == null) {
                return "{0} = {1}{2}".Fmt(idQualified, HibernateUtil.ParameterPrefix, ByUserIdQueryParameter);
            }
            var siteIdQualified = BaseQueryUtil.QualifyAttribute(_entityMetadata, siteIdAttribute);
            return "({0} = {1}{2} and  {3} = {1}{4})".Fmt(idQualified, HibernateUtil.ParameterPrefix, ByUserIdQueryParameter, siteIdQualified, BySiteIdQueryParameter);
        }

        public IDictionary<string, object> GetParameters() {
            var result = new Dictionary<string, object> {{ByUserIdQueryParameter, _userIdSiteTuple.Item1}};
            if (_userIdSiteTuple.Item2 != null) {
                result.Add(BySiteIdQueryParameter, _userIdSiteTuple.Item2);
            }
            return result;
        }
    }
}
