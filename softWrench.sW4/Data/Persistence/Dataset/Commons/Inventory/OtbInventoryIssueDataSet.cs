using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {
    class OtbInventoryIssueDataSet : BaseInventoryIssueDataSet {

        public override SearchRequestDto FilterBins(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Add(new ProjectionField("binnum", "ISNULL(invbalances.binnum, '')"));
            filter.SearchSort = "invbalances.binnum,invbalances.lotnum";
            filter.SearchAscending = true;
            
            return filter;
        }


        public override string ApplicationName() {
            return "invissue";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}