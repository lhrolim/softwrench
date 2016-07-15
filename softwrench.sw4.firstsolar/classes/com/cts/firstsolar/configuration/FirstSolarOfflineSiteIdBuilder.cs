using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarOfflineSiteIdBuilder : IDynamicWhereBuilder {

        private readonly IContextLookuper _contextLookuper;

        public FirstSolarOfflineSiteIdBuilder(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }


        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            var context = _contextLookuper.LookupContext();
            if (!context.OfflineMode || entityName.Equals("workorder")) {
                return null;
            }
            var entity = MetadataProvider.Entity(entityName);
            var attributes = entity.Attributes(EntityMetadata.AttributesMode.NoCollections);

            var enumerable = attributes as IList<EntityAttribute> ?? attributes.ToList();
            var hasOrgid = enumerable.Any(a => a.Name.EqualsIc("orgid"));
            var hasSiteid = enumerable.Any(a => a.Name.EqualsIc("siteid"));

            if (!hasOrgid && !hasSiteid) {
                return null;
            }

            var user = SecurityFacade.CurrentUser();

            if (!hasSiteid) {
                return " {0}.orgid = '{1}' ".Fmt(entityName, user.OrgId);
            }

            if (!hasOrgid) {
                return " {0}.siteid = '{1}' ".Fmt(entityName, user.SiteId);
            }


            return " {0}.orgid = '{1}' and {0}.siteid = '{2}' ".Fmt(entityName, user.OrgId, user.SiteId);

        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
