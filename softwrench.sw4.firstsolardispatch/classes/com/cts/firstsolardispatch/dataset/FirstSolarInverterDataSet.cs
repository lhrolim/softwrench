using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.dataset {
    public class FirstSolarInverterDataSet : SWDBApplicationDataset {


        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var locationPrefix = parameters.OriginalEntity.GetStringAttribute("locationprefix");
            filter.AppendWhereClause("asset.location like '" + locationPrefix + "%' and asset.status in ('ACTIVE', 'OPERATING')");
            if (string.IsNullOrEmpty(filter.SearchSort) || "assetuid".Equals(filter.SearchSort)) {
                filter.SearchSort = "assetnum";
            }
            return filter;
        }

        public override string ApplicationName() {
            return "_inverter";
        }
    }
}
