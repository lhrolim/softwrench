using System;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class ComToolTransDataSet : BaseApplicationDataSet {
        public SearchRequestDto CheckPlannedTools(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            return filter;
        }

        public override string ApplicationName() {
            return "tooltrans";
        }

        public override string ClientFilter() {
            return "manchester";
        }
    }
}