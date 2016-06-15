using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration.Dashboard {
    public class ChicagoDashBoardWhereClausedProvider : ISingletonComponent {
        /// <summary>
        /// Complete SELECT statistics query for sr.dailytickets: includes the statuses's descriptions as labels.
        /// </summary>
        private const string SR_STATUS_DAILY = @"select day(creationdate) as day, count(creationdate) as countBy from sr
                where month(creationdate) = {0}
                and SR.pluspcustomer like 'CPS-00'
                group by day(creationdate)
                order by day ";


        public string GetTicketCountQuery() {
            return SR_STATUS_DAILY.Fmt(DateTime.Now.Month);
        }

        

    }
}
