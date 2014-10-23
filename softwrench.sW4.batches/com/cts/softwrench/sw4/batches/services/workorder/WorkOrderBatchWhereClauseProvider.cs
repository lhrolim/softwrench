using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    public class WorkOrderBatchWhereClauseProvider : ISingletonComponent {

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
            return "siteid = '{0}' and status = 'WORKING'".Fmt(SecurityFacade.CurrentUser().SiteId);
        }

    }
}
