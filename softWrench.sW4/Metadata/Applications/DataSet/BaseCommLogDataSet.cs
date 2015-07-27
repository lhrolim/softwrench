using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Quartz.Util;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseCommLogDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;

        public BaseCommLogDataSet(ISWDBHibernateDAO swdbDAO) {
            _swdbDAO = swdbDAO;
        }

        public IEnumerable<IAssociationOption> EmailPostFilter(AssociationPostFilterFunctionParameters postParams) {

            InMemoryUser currentUser = SecurityFacade.CurrentUser();

            var addresses = _swdbDAO.FindByQuery<Email.Email>("FROM Email WHERE UserID = '{0}'".FormatInvariant(currentUser.MaximoPersonId));

            foreach (var address in addresses) {
                postParams.Options.Add(new AssociationOption(address.EmailAddress, address.EmailAddress));
            }

            return postParams.Options;
        }

        public override string ApplicationName() {
            return "commlog";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
