using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    public class WoBatchWhereClauseProvider : ISingletonComponent {
        public static string GetCrewIdQuery(bool appendDescription)
        {
            var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
            var date = beginDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            return @"SELECT {0} FROM alndomain a WHERE a.domainid = 'CREWID' 
                AND    a.siteid = '{1}'
                and exists (select NULL from workorder wo where a.siteid = wo.siteid and a.value = wo.crewid and wo.status = 'WORKING' and wo.schedfinish >= '{2}')"
                .Fmt(appendDescription ? "value,description" : "value", SecurityFacade.CurrentUser().SiteId, date);
        }

        public static string WoCountQuery(string context) {
            if (ApplicationConfiguration.IsMSSQL(ApplicationConfiguration.DBType.Swdb)) {
                return "CASE WHEN itemids = '' then 0 else len(itemids) - len(replace(itemids, ',', ''))+1 end";
            }
            if (ApplicationConfiguration.IsMySql()) {
                return "CASE WHEN itemids = '' then 0 else length(itemids) - length(replace(itemids, ',', ''))+1 end";
            }
            throw new NotSupportedException("db2 not yet implemented");
        }


        /// <summary>
        /// Users can create new WO batches out of queries. Those queries use the following fields/filters:
        ///-Site (user default site; cannot be changed)
        ///-Crew (Show list of Crews for the Default Site where one or more Work Orders in WORKING status exist)
        ///-WO Status (WORKING; cannot be changed)
        ///-Schedule Start (required; default to today-14)
        //-Schedule End (required; default to today)
        /// </summary>
        /// <returns></returns>
        public string CreateBatchWhereClause() {

            var crewIdQuery = GetCrewIdQuery(false).Fmt(GetCrewIdQuery(false));
            return "siteid = '{0}' and status = 'WORKING' and crewid in  ({1}) ".Fmt(SecurityFacade.CurrentUser().SiteId, crewIdQuery);
        }
    }
}
