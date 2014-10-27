using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    public class WorkOrderBatchWhereClauseProvider : ISingletonComponent {
        public static string GetCrewIdQuery(bool appendDescription) {
            var beginDate = DateUtil.ParsePastAndFuture("14days", -1);
            return @"SELECT {0} FROM alndomain a WHERE a.domainid = 'CREWID' 
                AND    a.siteid IN ('CT','RO','ALF','BRF','COF','CUF','GAF','JOF','JSF','KIF','PAF','SHF','WCF')
                and exists (select NULL from workorder wo where a.siteid = wo.siteid and a.value = wo.crewid and wo.status = 'WORKING' and wo.schedfinish >= {1});"
                .Fmt(appendDescription ? "value,description" : "value", beginDate);
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
