using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util {

    public class FirstSolarFacilityUtil {


        public static string BaseFacilityQuery(string columnName) {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString(columnName, facilities.Select(f => f + "%"), true);
                sb.AppendFormat("({0})", locationQuery);
            }
            return sb.ToString();
        }

    }
}
