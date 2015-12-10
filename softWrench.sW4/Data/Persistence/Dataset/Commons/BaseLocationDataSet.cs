using System.Text;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class BaseLocationDataSet : MaximoApplicationDataSet {
        public SearchRequestDto BuildAssettransWhereClause(CompositionPreFilterFunctionParameters parameter) {
            var originalEntity = parameter.OriginalEntity;
            var location = originalEntity.GetAttribute("location");
            var siteid = originalEntity.GetAttribute("siteid");
            var orgid = originalEntity.GetAttribute("orgid");
            
            var sb = new StringBuilder();
            //base section
            sb.AppendFormat(@"(assettrans.fromloc = '{0}' AND assettrans.siteid = '{1}' AND assettrans.orgid = '{2}')
                            OR (assettrans.toloc = '{0}' AND assettrans.tositeid = '{1}' AND assettrans.toorgid = '{2}')",
                            location, siteid, orgid);
            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(sb.ToString());

            return parameter.BASEDto;
        }

        public override string ApplicationName() {
            return "location";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
