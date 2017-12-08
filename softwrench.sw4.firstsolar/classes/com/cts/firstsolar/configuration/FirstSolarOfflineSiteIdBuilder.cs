using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
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

            var secondaryOrg = user.UserPreferences?.GetGenericProperty(FirstSolarConstants.SecondaryOrg)?.Value;
            var secondarySite = user.UserPreferences?.GetGenericProperty(FirstSolarConstants.SecondarySite)?.Value;


            if (!hasSiteid) {
                if (secondaryOrg != null) {
                    return " {0}.orgid in ({1}) or {0}.orgid is null ".Fmt(entityName, BaseQueryUtil.GenerateInString(new List<string>() { user.OrgId, secondaryOrg }));
                }

                return " {0}.orgid = '{1}' or {0}.orgid is null ".Fmt(entityName, user.OrgId);
            }

            if (!hasOrgid) {
                if (secondarySite != null) {
                    return " {0}.siteid in ({1}) or {0}.siteid is null ".Fmt(entityName, BaseQueryUtil.GenerateInString(new List<string>() { user.SiteId, secondarySite }));
                }

                return " {0}.siteid = '{1}' or {0}.siteid is null ".Fmt(entityName, user.SiteId);
            }

            if (!string.IsNullOrEmpty(secondarySite)) {
                var orgs = new List<string>() { user.OrgId, secondaryOrg };
                var sites = new List<string>() { user.SiteId, secondarySite };
                return " ({0}.orgid in ({1}) or {0}.orgid is null) and ({0}.siteid in ({2}) or {0}.siteid is null) ".Fmt(entityName, BaseQueryUtil.GenerateInString(orgs), BaseQueryUtil.GenerateInString(sites));
            }

            return " ({0}.orgid = '{1}' or {0}.orgid is null) and ({0}.siteid = '{2}' or {0}.siteid is null) ".Fmt(entityName, user.OrgId, user.SiteId);
            // TODO: verify if some associations will conflict when siteid is null. In that case: result = list.orderby(a => a.siteid).tohashset(<equals = a.UserId>)
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }
    }
}
