using System.Collections.Generic;
using System.Dynamic;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    public class KongsbergWorklogDataSet : MaximoApplicationDataSet {

        private readonly AttachmentDao _attachmentDAO;

        public KongsbergWorklogDataSet(AttachmentDao attachmentDAO) {
            _attachmentDAO = attachmentDAO;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject.Fields;

            var docinfourl = WorklogAttachmentUrl(datamap[application.IdFieldName]);

            if (docinfourl == null) return result;

            var fileUrl = AttachmentHandler.GetFileUrl(docinfourl);
            datamap.Add("attachment_url", fileUrl);

            return result;
        }

        private string WorklogAttachmentUrl(object worklogid) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("OWNERID", worklogid));

            var attachment = _attachmentDAO.SingleByOwner("WORKLOG", worklogid);
            return attachment != null ? attachment.urlname : null;
        }

        public override string ApplicationName() {
            return "worklog";
        }

        public override string SchemaId() {
            return "detail";
        }

        public override string ClientFilter() {
            return "kongsberg";
        }
    }
}
