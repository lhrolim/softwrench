using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder {
    public class WorkorderToolTransDataSet : MaximoApplicationDataSet {
        public override string ApplicationName() {
            return "tooltrans";
        }

        public SearchRequestDto FilterRotating(AssociationPreFilterFunctionParameters preFilterFunction) {
            preFilterFunction.BASEDto.AppendWhereClause("rotating != 1");
            return preFilterFunction.BASEDto;
        }


    }
}
