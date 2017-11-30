using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using System.ComponentModel.Composition;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderMaterialsDataSet : WorkorderMaterialsDataSet {

      
        [Import]
        public FirstSolarFacilityUtil FirstSolarFacilityUtil { get; set; }

        public override SearchRequestDto filterItems(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            var siteId = preParams.OriginalEntity.GetAttribute("siteid");
            var facilityQuery = FirstSolarFacilityUtil.BaseFacilityQuery("invbalances.location");
            var storeLocQuery = "1=1";
            var storeLocNumber =  preParams.OriginalEntity.GetStringAttribute("storeloc");
            if (storeLocNumber != null){
                storeLocQuery = $"location = '{storeLocNumber}'";
            }
            filter.AppendWhereClause($"(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{siteId}' and  ({facilityQuery}) and ({storeLocQuery}) ) )");

            return filter;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
