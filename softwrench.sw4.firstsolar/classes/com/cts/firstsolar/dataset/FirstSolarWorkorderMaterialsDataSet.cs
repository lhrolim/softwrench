using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using System.ComponentModel.Composition;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Entities;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarWorkorderMaterialsDataSet : WorkorderMaterialsDataSet {

      
        [Import]
        public FirstSolarFacilityUtil FirstSolarFacilityUtil { get; set; }

        public override SearchRequestDto filterItems(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            var entity = preParams.OriginalEntity as Entity;
            var storeLoc =  preParams.OriginalEntity.GetStringAttribute("storeloc");
            var category = entity?.GetUnMappedAttribute("category");
            var categoryClause = !string.IsNullOrEmpty(category) && !"any".EqualsIc(category) ? $" and inventory.category = '{category}'" : "";

            filter.AppendWhereClause($"(item.itemnum IN (SELECT inventory.itemnum from inventory inventory where status = 'ACTIVE' and inventory.location = '{storeLoc}' and siteid = '{preParams.OriginalEntity.GetStringAttribute("siteid")}' {categoryClause} ))");
            return filter;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
