using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    class BaseWorkorderDataSet : BaseTicketDataSet {


        public SearchRequestDto FilterStatusCodes(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("( MAXVALUE != 'HISTEDIT' )");
            return filter;
        }

        /* Need to add this prefilter function for the problem codes !! */
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters) {
            return ProblemCodeFilterByFailureClassFunction(parameters);
        }

        private SearchRequestDto ProblemCodeFilterByFailureClassFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var failurecodeid = parameters.OriginalEntity.GetAttribute("failurelist_.failurelist");
            if (failurecodeid == null) {
                return filter;
            }
            filter.AppendSearchEntry("parent", failurecodeid.ToString());
            return filter;
        }


        public IEnumerable<IAssociationOption> GetWOPriorityType(OptionFieldProviderParameters parameters) {
            var query = @"SELECT description AS LABEL,
	                             CAST(value AS INT) AS VALUE 
                          FROM numericdomain
                          WHERE domainid = 'WO PRIORITY'";

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result != null && result.Any()) {
                foreach (var record in result) {
                    list.Add(new AssociationOption(record["VALUE"], $"{record["VALUE"]} - {record["LABEL"]}"));
                }
            } else {
                // If no values are found, then default to numeric selection 1-5
                list.Add(new AssociationOption("1", "1"));
                list.Add(new AssociationOption("2", "2"));
                list.Add(new AssociationOption("3", "3"));
                list.Add(new AssociationOption("4", "4"));
                list.Add(new AssociationOption("5", "5"));
            }

            return list;
        }

        public IEnumerable<IAssociationOption> GetWOClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "WORKORDER");
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
