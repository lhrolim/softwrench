using System.Linq;
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

            var attachments = _attachmentDAO.ByOwner("WORKLOG", datamap[application.IdFieldName]);

            if (!attachments.Any()) return result;

            if (attachments.Count == 1) {
                var attachment = attachments.First();
                var docinfourl = (string)attachment.urlname;

                var fileUrl = AttachmentHandler.GetFileUrl(docinfourl);
                var isImage = fileUrl.ContainsAnyIgnoreCase(new[] { "png", "bmp", "jpg", "jpeg", "gif" }); // TODO: use actual mime type detection

                datamap.Add("attachment_is_image", isImage);
                // isimage ? image's url : list of file links
                var attachmentToDisplay = isImage ? fileUrl : (object)new[] {
                    new { name = attachment.document, url = fileUrl }
                };
                datamap.Add("attachment_url", attachmentToDisplay);

                return result;
            }

            var attachmentLinks = attachments.Select(a => new {
                name = a.document,
                url = AttachmentHandler.GetFileUrl((string)a.urlname)
            });

            datamap.Add("attachment_url", attachmentLinks);

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
