using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;


namespace softWrench.sW4.Metadata.Applications.DataSet {

    class InvuseDataSet : MaximoApplicationDataSet {
        public SearchRequestDto FilterLocation(AssociationPreFilterFunctionParameters parameters) {
            return LocationFilterByType(parameters);
        }

        public SearchRequestDto LocationFilterByType(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            const string type = "STOREROOM";
            filter.AppendSearchEntry("location.type", type.ToUpper());
            return filter;
        }

        public SearchRequestDto FilterBin(AssociationPreFilterFunctionParameters parameters) {
            return BinFilterByLocation(parameters);
        }

        public SearchRequestDto BinFilterByLocation(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = parameters.OriginalEntity.GetAttribute("fromstoreloc") as string;
            if (location != null) {
                filter.AppendSearchEntry("invbalances.location", location.ToUpper());
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