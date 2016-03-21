using System.Linq;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog {
    static class CommlogHelper {
        public static CompositionFetchResult SetCommlogReadStatus(ApplicationMetadata application, CompositionFetchRequest request, CompositionFetchResult compList) {
            var user = SecurityFacade.CurrentUser();

            if (user == null) {
                return compList;
            }

            var commData = SWDBHibernateDAO.GetInstance().FindByQuery<MaxCommReadFlag>(MaxCommReadFlag.ByItemIdAndUserId, application.Name, request.Id, user.DBId);

            if (!compList.ResultObject.ContainsKey("commlog_")) {
                return compList;
            }

            var commlogs = compList.ResultObject["commlog_"].ResultList;

            foreach (var commlog in commlogs) {
                var readFlag = (from c in commData
                                where c.CommlogId.ToString() == commlog["commloguid"].ToString()
                                select c.ReadFlag).FirstOrDefault();

                commlog["read"] = readFlag;
            }
            return compList;
        }
    }
}
