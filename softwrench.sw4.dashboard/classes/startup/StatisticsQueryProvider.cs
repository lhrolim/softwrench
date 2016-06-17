using System.Collections.Generic;
using ctes = softwrench.sw4.dashboard.classes.service.statistics.StatisticsConstants;

namespace softwrench.sw4.dashboard.classes.startup {

    public class StatisticsQueryProvider {


        /// <summary>
        /// Complete SELECT statistics query for wo.status: includes the statuses's descriptions as labels.
        /// </summary>
        private static readonly string WoStatusWhereclauseCompleteQuery = string.Format(
            @"select COALESCE(CAST(status as varchar), 'NULL') as status, count(*) as {0}, s.description as {1} 
                from workorder 
                left join synonymdomain s
       	            on status = s.value
  	            {2}
                group by status,s.description
                order by countBy desc",
            ctes.FIELD_VALUE_VARIABLE_NAME, ctes.FIELD_LABEL_VARIABLE_NAME, ctes.CONTEXT_FILTER_VARIABLE_NAME);

        /// <summary>
        /// Complete SELECT statistics query for sr.status: includes the statuses's descriptions as labels.
        /// </summary>
        private static readonly string SRStatusWhereclauseCompleteQuery = string.Format(
            @"select COALESCE(CAST(status as varchar), 'NULL') as status, count(*) as {0}, s.description as {1} 
                from sr 
                left join synonymdomain s
       	            on status = s.value
  	            {2}
                group by status,s.description
                order by countBy desc",
            ctes.FIELD_VALUE_VARIABLE_NAME, ctes.FIELD_LABEL_VARIABLE_NAME, ctes.CONTEXT_FILTER_VARIABLE_NAME);


        public static IDictionary<string, string> BaseStatisticsSelectQueries = new Dictionary<string, string>{
            {"dashboard:wo.status.top5",  WoStatusWhereclauseCompleteQuery},
            {"dashboard:sr.status.top5",  SRStatusWhereclauseCompleteQuery},
            {"dashboard:sr.status.line",  SRStatusWhereclauseCompleteQuery},
            {"dashboard:sr.status.pie",  SRStatusWhereclauseCompleteQuery },
        };

        /// <summary>
        /// Use this method to register a custom select where clause for a specific dashboard
        /// </summary>
        /// <param name="metadataId"></param>
        /// <param name="query"></param>
        public static void AddCustomSelectQuery(string metadataId, string query) {
            BaseStatisticsSelectQueries.Add(metadataId, query);
        }


    }
}
