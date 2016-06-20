using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.dataset {
    class ToshibaServiceRequestDataSet : BaseServiceRequestDataSet {
        private readonly IMaximoHibernateDAO _maximoDao;
        public ToshibaServiceRequestDataSet(ISWDBHibernateDAO swdbDao, IMaximoHibernateDAO maximoDao) : base(swdbDao) {
            _maximoDao = maximoDao;
        }

        public SearchRequestDto FilterQSRWorklogs(CompositionPreFilterFunctionParameters parameter) {
            parameter.BASEDto.AppendWhereClause(" clientviewable = 1 ");
            return parameter.BASEDto;
        }

        public override string ApplicationName() {
            return "servicerequest,quickservicerequest";
        }

        public override string ClientFilter() {
            return "tgcs";
        }
    }
}
