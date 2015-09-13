using System.Collections.Generic;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Email;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseCommLogDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;

        public BaseCommLogDataSet(ISWDBHibernateDAO swdbDAO) {
            _swdbDAO = swdbDAO;
        }

        public SearchRequestDto EmailPreFilter(AssociationPreFilterFunctionParameters preparams){
            preparams.BASEDto.AppendWhereClause("1=2");
            return preparams.BASEDto;
        }

        public IEnumerable<IAssociationOption> EmailPostFilter(AssociationPostFilterFunctionParameters postParams) {

            var currentUser = SecurityFacade.CurrentUser();

            var addresses = _swdbDAO.FindByQuery<EmailHistory>(EmailHistory.byUserId, currentUser.Login);



            foreach (var address in addresses) {
                postParams.Options.Add(new AssociationOption(address.EmailAddress.ToLower().Trim(), address.EmailAddress.ToLower().Trim()));
            }

//            for (var i = 0; i < 10000; i++) {
//                postParams.Options.Add(new AssociationOption("testemail" + i + "@a.com", "testemail" + i + "@a.com"));
//            }

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
