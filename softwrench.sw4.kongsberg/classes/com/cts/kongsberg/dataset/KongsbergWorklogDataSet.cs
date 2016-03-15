using System.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.dataset {
    public class KongsbergWorklogDataSet : MaximoApplicationDataSet {

        private static string TOP_ATTACHMENT_URL_BY_WORKLOGID = @"select top(1) I.urlname from docinfo I
	                                                                inner join doclinks L
		                                                                on I.docinfoid = L.docinfoid
	                                                                where L.ownertable='WORKLOG' and L.ownerid=?
	                                                                order by L.createdate desc";

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject.Fields;

            var docinfourllist = MaximoHibernateDAO.GetInstance().FindByNativeQuery(TOP_ATTACHMENT_URL_BY_WORKLOGID, datamap[application.IdFieldName]);
            if (!docinfourllist.Any()) return result;

            var docinfourl = docinfourllist.First()["urlname"];
            var fileUrl = AttachmentHandler.GetFileUrl(docinfourl);
            datamap.Add("attachment_url", fileUrl);

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
