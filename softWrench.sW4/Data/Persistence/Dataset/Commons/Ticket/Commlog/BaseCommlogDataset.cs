using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Email;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog {
    public class BaseCommlogDataset : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _swdbDAO;
        private readonly AttachmentDao _attachmentDAO;
        private AttachmentHandler _attachmentHandler;


        public BaseCommlogDataset(ISWDBHibernateDAO swdbDAO, AttachmentDao attachmentDAO) {
            _swdbDAO = swdbDAO;
            _attachmentDAO = attachmentDAO;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);

            var datamap = result.ResultObject.Fields;
            var id = datamap[application.IdFieldName];

            var attachments = _attachmentDAO.ByOwner("COMMLOG", id);

            if (!attachments.Any()) return result;
            var displayableAttachments = attachments.Select(attachment => new {
                    name = attachment.document,
                    url = AttachmentHandler.GetFileUrl(attachment.urlname)
                });
            datamap.Add("attachments", displayableAttachments);
            return result;
        }

        public IEnumerable<IAssociationOption> EmailPostFilter(AssociationPostFilterFunctionParameters postParams) {

            var currentUser = SecurityFacade.CurrentUser();

            var addresses = _swdbDAO.FindByQuery<EmailHistory>(EmailHistory.byUserId, currentUser.Login);

            foreach (var address in addresses) {
                postParams.Options.Add(new AssociationOption(address.EmailAddress.ToLower().Trim(), address.EmailAddress.ToLower().Trim()));
            }

            return postParams.Options;
        }

        public override string ApplicationName() {
            return "commlog";
        }
        
    }
}
