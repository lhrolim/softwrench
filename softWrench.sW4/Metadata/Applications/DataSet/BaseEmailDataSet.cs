using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Email;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseEmailDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;

        public BaseEmailDataSet(ISWDBHibernateDAO swdbDAO) {
            _swdbDAO = swdbDAO;
        }

        public IEnumerable<AssociationOption> GetEmailTypeOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT value, description as label FROM alndomain WHERE domainid = 'EMAILTYPE'").ToList();
            return rows.Select(row => new AssociationOption(row["value"], row["label"])).ToList();
        } 

        public override string ApplicationName() {
            return "email";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
