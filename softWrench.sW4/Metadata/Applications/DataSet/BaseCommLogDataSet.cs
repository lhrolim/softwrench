using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseCommLogDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;

        public BaseCommLogDataSet(ISWDBHibernateDAO swdbDAO) {
            _swdbDAO = swdbDAO;
        }

        public IEnumerable<IAssociationOption> EmailPostFilter(AssociationPostFilterFunctionParameters postParams) {

            InMemoryUser currentUser = SecurityFacade.CurrentUser();

            var addresses = _swdbDAO.FindByQuery<Email.Email>("FROM Email WHERE UserID = {0}", currentUser.MaximoPersonId).ToList();

            return (from item in postParams.Options where item.Label != null && item.Value.Equals(postParams.OriginalEntity.Attributes["itemnum"]) select new AssociationOption(item.Label, item.Label)).Cast<IAssociationOption>().ToList();
        }

        public override string ApplicationName() {
            return "commlog";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
