using System;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class OtbServiceRequestDataSet : MaximoApplicationDataSet {
        /* Need to add this prefilter function for the problem codes !! 
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters)
        {

        }*/

        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            return AssetFilterBySiteFunction(parameters);
        }


        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}