using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util {

    public class FirstSolarFacilityUtil : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarFacilityUtil));

        [Import]
        public IContextLookuper ContextLookuper { get; set; }

        public string BaseFacilityQuery(string columnName) {
            var user = SecurityFacade.CurrentUser();
            var sb = new StringBuilder();


            if ((user.IsInRole(FirstSolarConstants.FacilityAdmin) || user.IsSwAdmin()) && !ContextLookuper.LookupContext().OfflineMode) {
                Log.WarnFormat("current user {0} is a facility admin showing it all", user.Login);
                return "1=1";
            }

            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                var locationQuery = BaseQueryUtil.GenerateOrLikeString(columnName, facilities.Select(f => f + "%"), true);
                sb.AppendFormat("({0})", locationQuery);
                return sb.ToString();
            }


            Log.WarnFormat("current user {0}  has no facilities selected", user.Login);
            return "1!=1";
        }

    }
}
