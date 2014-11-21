using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Persistence.Dataset.Commons;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class OtbInvreserveDataSet : MaximoApplicationDataSet {

        public SearchRequestDto FilterLots(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var binnum = (string) parameters.OriginalEntity.GetAttribute("invbalancesBin.binnum");
            if (binnum != null) {
                filter.AppendSearchEntry("binnum", binnum.ToUpper());
            }

            return filter;
        }

        public override string ApplicationName() {
            return "reservedMaterials";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}