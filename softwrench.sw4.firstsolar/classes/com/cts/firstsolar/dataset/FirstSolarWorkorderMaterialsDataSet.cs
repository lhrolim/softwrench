using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderMaterialsDataSet : WorkorderMaterialsDataSet {

      

        public override SearchRequestDto filterItems(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            var siteId = preParams.OriginalEntity.GetAttribute("siteid");
            var facilityQuery = FirstSolarFacilityUtil.BaseFacilityQuery("invbalances.location");
            filter.AppendWhereClause($"(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{siteId}' and  ({facilityQuery}) ) )");
            return filter;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
