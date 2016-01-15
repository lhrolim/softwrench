using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util {
    public class FirstSolarWoValidationHelper : ISingletonComponent {

        private readonly MaximoHibernateDAO _dao;

        private const string BaseLocationQuery =
            "select location,wonum,description from workorder where status not in('CLOSED','COMPLETED') and location in ({0})";

        private const string BaseAssetQuery =
            "select assetnum,wonum,description from workorder where status not in('CLOSED','COMPLETED') and assetnum in ({0})";

        public FirstSolarWoValidationHelper(MaximoHibernateDAO dao) {
            _dao = dao;
        }


        public IDictionary<string, List<string>> DoValidateIdsThatHaveWorkorders(string baseQuery, string itemid, ICollection<AssociationOption> items, string classification) {
            var sb = new StringBuilder();
            sb.AppendFormat(baseQuery, BaseQueryUtil.GenerateInString(items.Select(i => i.Value)));
            if (classification != null) {
                sb.AppendFormat(" and classstructureid = '{0}'", classification);
            }
            var queryResult = _dao.FindByNativeQuery(sb.ToString());

            var result = new Dictionary<string, List<string>>();

            foreach (var row in queryResult) {
                var itemId = row[itemid];
                var wonum = row["wonum"];
                var summary = row["description"];
                if (!result.ContainsKey(itemId)) {
                    result.Add(itemId, new List<string> { wonum });
                } else {
                    result[itemId].Add(wonum);
                }
            }
            return result;
        }


        [NotNull]
        public IDictionary<string, List<string>> ValidateIdsThatHaveWorkordersForLocation(ICollection<AssociationOption> items, string classification) {
            return DoValidateIdsThatHaveWorkorders(BaseLocationQuery, "location", items, classification);
        }

        [NotNull]
        public IDictionary<string, List<string>> ValidateIdsThatHaveWorkordersForAsset(ICollection<AssociationOption> items, string classification) {
            return DoValidateIdsThatHaveWorkorders(BaseAssetQuery, "assetnum", items, classification);
        }

    }
}
