using System.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using System;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Worklog {
    public class BaseWorklogDataSet : MaximoApplicationDataSet {

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject.Fields;

            var docinfolist = MaximoHibernateDAO.GetInstance().FindByNativeQuery("select top(1) docinfoid from DOCLINKS where ownertable=? and ownerid=? order by createdate desc", "WORKORDER", datamap[application.IdFieldName]);
            if (!docinfolist.Any()) return result;

            var docinfoid = docinfolist.First()["docinfoid"];
            var file = AttachmentHandler.DownloadViaHttpById(docinfoid);
            datamap.Add("attachment_content", Convert.ToBase64String(file.Item1, Base64FormattingOptions.None));

            return result;
        }

        public override string ApplicationName() {
            return "worklog";
        }

        public override string SchemaId() {
            return "detail";
        }
    }
}
