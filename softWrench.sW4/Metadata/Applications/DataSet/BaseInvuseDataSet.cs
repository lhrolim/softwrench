using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;


namespace softWrench.sW4.Metadata.Applications.DataSet {

    class BaseInvuseDataSet : MaximoApplicationDataSet {
        public SearchRequestDto FilterLocation(AssociationPreFilterFunctionParameters parameters) {
            return LocationFilterByType(parameters);
        }

        public SearchRequestDto LocationFilterByType(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            const string type = "STOREROOM";
            filter.AppendSearchEntry("location.type", type.ToUpper());
            return filter;
        }

        public SearchRequestDto FilterFromStoreLoc(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            if (siteid != null && itemnum != null) {
                filter.AppendSearchEntry("inventory.siteid", siteid.ToString().ToUpper());
                filter.AppendSearchEntry("inventory.itemnum", itemnum.ToString().ToUpper());
            }
            return filter;
        }

        public SearchRequestDto FilterBin(AssociationPreFilterFunctionParameters parameters) {
            return BinFilterByLocation(parameters);
        }

        public SearchRequestDto BinFilterByLocation(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("location");
            if (siteid != null && location != null) {
                filter.AppendSearchEntry("inventory.siteid", siteid.ToString().ToUpper());
                filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
            }
            return filter;
        }

        public override string ApplicationName() {
            return "invuse";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}