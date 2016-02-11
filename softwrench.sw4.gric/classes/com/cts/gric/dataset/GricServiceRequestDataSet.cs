using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.gric.classes.com.cts.gric.dataset {
    class GricServiceRequestDataSet : BaseServiceRequestDataSet {

        public GricServiceRequestDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        public SearchRequestDto FilterStatusCodes(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("( MAXVALUE != 'HISTEDIT' )");
            return filter;
        }


        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return "gric";
        }

      
    }
}
