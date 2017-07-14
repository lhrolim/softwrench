using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarCustomGlobalFedQueries : ISingletonComponent {

        public string FacilityQuery(string context) {
            if (!ApplicationConfiguration.IsProd()) {
                return " SUBSTRING({0}.location, 0, 5) ".Fmt(context);
            }
            return " (CASE WHEN exists (select * from onmparms o where {0}.location like o.value + '%') THEN (select top 1 G.scadA_GUID from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on  (o.description=G.assettitle or o.value=G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') WHEN 1=1 then SUBSTRING({0}.location, 0, 5) END) ".Fmt(context);
        }

        public string PlannerQuery(string context) {
            if (!ApplicationConfiguration.IsProd()) {
                return " 'Test Planner' ";
            }
            return " (select top 1 G.onM_Planner_Scheduler from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on (o.description = G.assettitle or o.value = G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') ".Fmt(context);
        }
    }
}
