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
        public static string SRStatusDaily = @"select day(creationdate) as day, count(creationdate) as countBy from sr
                {statistics_context_filter}
                group by day(creationdate)
                order by day ";


        /// <summary>
        /// Complete SELECT statistics query for sr.dailytickets: includes the statuses's descriptions as labels.
        /// </summary>
        private const string SRStatusDailyWhere = @"month(creationdate) = {0}";
                


        public string GetTicketCountQuery() {
            return SRStatusDailyWhere.Fmt(DateTime.Now.Month);
        }

        

    }
}
