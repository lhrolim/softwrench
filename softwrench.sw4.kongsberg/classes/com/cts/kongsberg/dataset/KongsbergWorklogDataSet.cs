using System.Collections.Generic;
using System.Dynamic;
using cts.commons.portable.Util;
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

            var attachment = _attachmentDAO.SingleByOwner("WORKLOG", datamap[application.IdFieldName]);
            if (attachment == null || string.IsNullOrEmpty(attachment.urlname)) return result;

            var docinfourl = attachment.urlname;

            var fileUrl = (string) AttachmentHandler.GetFileUrl(docinfourl);
            var isImage = fileUrl.ContainsAnyIgnoreCase(new[] {"png", "bmp", "jpg", "jpeg", "gif"}); // TODO: use actual mime type detection

            datamap.Add("attachment_is_image", isImage);
            // isimage ? image's url : list of file links
            var attachmentToDisplay = isImage ? fileUrl : (object) new[] {
                new { name = attachment.document, url = fileUrl }
            };
            datamap.Add("attachment_url", attachmentToDisplay);

            return result;
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
